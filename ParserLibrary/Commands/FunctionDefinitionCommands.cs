using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Commands
{
    internal class FunctionDefinitionCommands
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _smtCtxProvider;
        private readonly ISemgusContextProvider _semgusCtxProvider;
        private readonly ISmtScopeProvider _scopeProvider;
        private readonly ISmtConverter _converter;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<FunctionDefinitionCommands> _logger;

        public FunctionDefinitionCommands(ISemgusProblemHandler handler,
                                    ISmtContextProvider smtCtxProvider,
                                    ISemgusContextProvider semgusCtxProvider,
                                    ISmtScopeProvider scopeProvider,
                                    ISmtConverter converter,
                                    ISourceMap sourceMap,
                                    ILogger<FunctionDefinitionCommands> logger)
        {
            _handler = handler;
            _smtCtxProvider = smtCtxProvider;
            _semgusCtxProvider = semgusCtxProvider;
            _scopeProvider = scopeProvider;
            _converter = converter;
            _sourceMap = sourceMap;
            _logger = logger;
        }
        public record DefinitionSignature(SmtIdentifier Name, IList<(SmtIdentifier Id, SmtSortIdentifier Sort)> Args, SmtSortIdentifier Ret);
        public record DeclarationSignature(SmtIdentifier Name, IList<SmtSortIdentifier> Args, SmtSortIdentifier Ret);

        [Command("define-funs-rec")]
        public void DefineFunsRec(IList<DefinitionSignature> signatures, IList<SemgusToken> definitions)
        {
            using var logScope = _logger.BeginScope($"while processing `define-funs-rec`:");

            if (signatures.Count != definitions.Count)
            {
                throw _logger.LogParseErrorAndThrow($"number of signatures ({signatures.Count}) does not match number of definitions {definitions.Count}", _sourceMap[signatures]);
            }

            List<(SmtFunction Decl, SmtFunctionRank Rank)> declarations = new();

            foreach (var signature in signatures)
            {
                var (decl, rank) = ProcessFunctionDeclaration(signature.Name, signature.Args.Select(s => s.Sort), signature.Ret);
                _smtCtxProvider.Context.AddFunctionDeclaration(decl);
                declarations.Add((decl, rank));
            }

            for (int ix = 0; ix < declarations.Count; ++ix)
            {
                ProcessFunctionDefinition(declarations[ix].Decl, declarations[ix].Rank, signatures[ix].Args.Select(a => a.Id), definitions[ix]);
            }
        }

        [Command("define-fun")]
        public void DefineFun(SmtIdentifier id, IList<(SmtIdentifier Id, SmtSortIdentifier Sort)> args, SmtSortIdentifier returnSort, SemgusToken definition)
        {
            using var logScope = _logger.BeginScope($"while processing `define-fun` for {id}:");

            var (decl, rank) = ProcessFunctionDeclaration(id, args.Select(a => a.Sort), returnSort);
            ProcessFunctionDefinition(decl, rank, args.Select(a => a.Id), definition);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        [Command("define-fun-rec")]
        public void DefineFunRec(SmtIdentifier id, IList<(SmtIdentifier Id, SmtSortIdentifier Sort)> args, SmtSortIdentifier returnSort, SemgusToken definition)
        {
            using var logScope = _logger.BeginScope($"while processing `define-fun-rec` for {id}:");

            var (decl, rank) = ProcessFunctionDeclaration(id, args.Select(a => a.Sort), returnSort);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
            ProcessFunctionDefinition(decl, rank, args.Select(a => a.Id), definition);
        }

        [Command("declare-fun")]
        public void DeclareFun(SmtIdentifier id, IList<SmtSortIdentifier> args, SmtSortIdentifier returnSort)
        {
            using var logScope = _logger.BeginScope($"while processing `declare-fun` for {id}:");

            var (decl, _) = ProcessFunctionDeclaration(id, args, returnSort);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        [Command("declare-const")]
        public void DeclareConst(SmtIdentifier id, SmtSortIdentifier sort)
        {
            using var logScope = _logger.BeginScope($"while processing `declare-const` for {id}:");

            var (decl, _) = ProcessFunctionDeclaration(id, Enumerable.Empty<SmtSortIdentifier>(), sort);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        private (SmtFunction, SmtFunctionRank) ProcessFunctionDeclaration(SmtIdentifier name, IEnumerable<SmtSortIdentifier> argIds, SmtSortIdentifier returnSortId)
        {
            using var logScope = _logger.BeginScope($"processing declaration for {name}:");

            var returnSort = _smtCtxProvider.Context.GetSortOrDie(returnSortId, _sourceMap, _logger);
            var args = argIds.Select(argId => _smtCtxProvider.Context.GetSortOrDie(argId, _sourceMap, _logger));
            var rank = new SmtFunctionRank(returnSort, args.ToArray());
            var decl = new SmtFunction(name, SmtTheory.UserDefined, rank);

            return (decl, rank);
        }

        private void ProcessFunctionDefinition(SmtFunction decl, SmtFunctionRank rank, IEnumerable<SmtIdentifier> arguments, SemgusToken token)
        {
            using var logScope = _logger.BeginScope($"processing definition for {decl.Name}:");
            using var scopeCtx = _scopeProvider.CreateNewScope();

            foreach (var (id, sort) in arguments.Zip(rank.ArgumentSorts))
            {
                scopeCtx.Scope.AddVariableBinding(id, sort, SmtVariableBindingType.Lambda);
            }

            if (!_converter.TryConvert(token, out SmtTerm? term))
            {
                throw _logger.LogParseErrorAndThrow($"Cannot convert function term: " + term, token.Position);
            }

            if (term.Sort == ErrorSort.Instance)
            {
                throw _logger.LogParseErrorAndThrow("Error encountered resolving term for definition of function " + decl.Name, token.Position);
            }

            if (term.Sort != rank.ReturnSort)
            {
                throw _logger.LogParseErrorAndThrow("Function return sort doesn't match term sort: " + decl.Name, token.Position);
            }

            SmtLambdaBinder lambda = new(term, scopeCtx.Scope, arguments);
            decl.AddDefinition(rank, lambda);

            MaybeProcessChcDefinition(decl, rank, lambda);
        }

        private void MaybeProcessChcDefinition(SmtFunction decl, SmtFunctionRank rank, SmtLambdaBinder defn)
        {
            using var logScope = _logger.BeginScope($"processing CHC for {decl.Name}:");

            // Rule: must return bool
            var boolSort = _smtCtxProvider.Context.GetSortOrDie(new SmtSortIdentifier("Bool"), _sourceMap, _logger);
            if (rank.ReturnSort != boolSort)
            {
                return; // Not a semantic relation
            }

            // Rule: first arg is a term type TODO: relax to having one anywhere
            if (rank.Arity < 1 || rank.ArgumentSorts[0] is not SemgusTermType tt)
            {
                return; // Not a semantic relation (probably)
            }
            
            // Rule: top-level is a match statement with a single variable term
            if (defn.Child is not SmtMatchGrouper grouper || grouper.Term is not SmtVariable variable)
            {
                return; // Not a top-level match statement
            }

            // Rule: term must be the left-most argument
            if (variable.Binding.DeclaringScope != defn.NewScope)
            {
                return; // Actually check if it's in the right place: TODO
            }

            // Now emit CHCs - start with the head
            SmtScope headScope = new(default);
            List<SmtVariableBinding> headBindings = new();
            for (int aIx = 0; aIx < defn.ArgumentNames.Count; ++aIx)
            {
                SmtVariableBinding b = new(defn.ArgumentNames[aIx], rank.ArgumentSorts[aIx], SmtVariableBindingType.Universal, headScope);
                headBindings.Add(b);
            }
            var head = new SemgusChc.SemanticRelation(decl, rank, headBindings.Select(b => new SmtVariable(b.Id, b)).ToList());

            List<SmtVariable>? inputs = default;
            List<SmtVariable>? outputs = default;

            var inputAttr = defn.Child.Annotations?.FirstOrDefault(a => a.Keyword.Name == "input");
            if (inputAttr is not null)
            {
                if (inputAttr.Value.ListValue is not null)
                {
                    inputs = new();
                    foreach (var v in inputAttr.Value.ListValue)
                    {
                        if (v.IdentifierValue is not null)
                        {
                            var vv = head.Arguments.FirstOrDefault(a => a.Name == v.IdentifierValue);
                            if (vv is null)
                            {
                                _logger.LogParseError($"Malformed input annotation for relation {decl.Name}: {v.IdentifierValue} not in parameter list.", _sourceMap[v]);
                            }
                            else
                            {
                                inputs.Add(vv);
                            }
                        }
                        else
                        {
                            _logger.LogParseError($"Malformed input annotation for relation {decl.Name}: expecting list of identifiers, but got type: {v.Type}", _sourceMap[v]);
                        }
                    }
                }
                else
                {
                    _logger.LogParseError($"Malformed input annotation for relation {decl.Name}: expecting list of variable identifiers.", _sourceMap[inputAttr]);
                }
            }

            var outputAttr = defn.Child.Annotations?.FirstOrDefault(a => a.Keyword.Name == "output");
            if (outputAttr is not null)
            {
                if (outputAttr.Value.ListValue is not null)
                {
                    outputs = new();
                    foreach (var v in outputAttr.Value.ListValue)
                    {
                        if (v.IdentifierValue is not null)
                        {
                            var vv = head.Arguments.FirstOrDefault(a => a.Name == v.IdentifierValue);
                            if (vv is null)
                            {
                                _logger.LogParseError($"Malformed output annotation for relation {decl.Name}: {v.IdentifierValue} not in parameter list.", _sourceMap[v]);
                            }
                            else
                            {
                                outputs.Add(vv);
                            }
                        }
                        else
                        {
                            _logger.LogParseError($"Malformed output annotation for relation {decl.Name}: expecting list of identifiers, but got type: {v.Type}", _sourceMap[v]);
                        }
                    }
                }
                else
                {
                    _logger.LogParseError($"Malformed output annotation for relation {decl.Name}: expecting list of variable identifiers.", _sourceMap[outputAttr]);
                }
            }

            // Each pattern is one or more CHC
            void procCHC(SmtMatchBinder binder, SmtTerm term)
            {
                using var logMatchScope = _logger.BeginScope($"in match {(binder.Constructor?.Operator.ToString() ?? "default")}:");

                SmtScope bodyScope = new(headScope);
                List<SmtVariableBinding> bodyBindings = new();
                if (term is SmtExistsBinder existsBinder)
                {
                    foreach (var binding in existsBinder.NewScope.LocalBindings)
                    {
                        bodyBindings.Add(new SmtVariableBinding(binding.Id, binding.Sort, SmtVariableBindingType.Universal, bodyScope));
                    }
                    term = existsBinder.Child;
                }

                List<SmtTerm> bodyParts = new();
                if (term is SmtFunctionApplication appl && appl.Definition.Name == new SmtIdentifier("and"))
                {
                    bodyParts.AddRange(appl.Arguments);
                }
                else
                {
                    bodyParts.Add(term);
                }

                List<SemgusChc.SemanticRelation> relList = new();
                SmtTerm? constraint = default;
                foreach (var c in bodyParts)
                {
                    if (c is SmtFunctionApplication cAppl)
                    {
                        var cRank = cAppl.Rank;
                        if (cRank.ReturnSort == boolSort && cRank.Arity > 0 && cRank.ArgumentSorts[0] is SemgusTermType && cAppl.Arguments.All(a => a is SmtVariable))
                        {
                            // Semantic relation. Probably.
                            relList.Add(new(cAppl.Definition, cRank, cAppl.Arguments.Select(a => (a as SmtVariable)!).ToList()));
                            continue;
                        }
                    }
                    if (constraint is not null)
                    {
                        throw _logger.LogParseErrorAndThrow($"Multiple constraints in CHC for {binder.Constructor!.Operator} in {decl.Name}. Only one first-order logic constraint is allowed.", _sourceMap[constraint]);
                    }
                    // TODO: check to make sure this doesn't contain semantic relations.
                    constraint = c;
                }

                _semgusCtxProvider.Context.AddChc(new SemgusChc(head, relList, constraint!, binder, headBindings.Concat(bodyBindings), inputs, outputs));
            }

            foreach (var pat in grouper.Binders)
            {
                if (pat.Child is SmtFunctionApplication appl)
                {
                    if (appl.Definition.Name == new SmtIdentifier("or"))
                    {
                        foreach (var t in appl.Arguments)
                        {
                            procCHC(pat, t);
                        }
                        continue;
                    }
                }
                procCHC(pat, pat.Child);
            }
        }
    }
}
