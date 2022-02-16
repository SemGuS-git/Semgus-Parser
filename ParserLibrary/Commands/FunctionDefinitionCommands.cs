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
        private readonly SmtConverter _converter;
        private readonly ILogger<FunctionDefinitionCommands> _logger;

        public FunctionDefinitionCommands(ISemgusProblemHandler handler,
                                    ISmtContextProvider smtCtxProvider,
                                    ISemgusContextProvider semgusCtxProvider,
                                    ISmtScopeProvider scopeProvider,
                                    SmtConverter converter,
                                    ILogger<FunctionDefinitionCommands> logger)
        {
            _handler = handler;
            _smtCtxProvider = smtCtxProvider;
            _semgusCtxProvider = semgusCtxProvider;
            _scopeProvider = scopeProvider;
            _converter = converter;
            _logger = logger;
        }
        public record DefinitionSignature(SmtIdentifier Name, IList<(SmtIdentifier Id, SmtSort Sort)> Args, SmtSort Ret);
        public record DeclarationSignature(SmtIdentifier Name, IList<SmtSort> Args, SmtSort Ret);

        [Command("define-funs-rec")]
        public void DefineFunsRec(IList<DefinitionSignature> signatures, IList<SemgusToken> definitions)
        {
            if (signatures.Count != definitions.Count)
            {
                throw new InvalidOperationException("In define-funs-rec: number of signatures does not match definitions");
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
        public void DefineFun(SmtIdentifier id, IList<(SmtIdentifier Id, SmtSort Sort)> args, SmtSort returnSort, SemgusToken definition)
        {
            var (decl, rank) = ProcessFunctionDeclaration(id, args.Select(a => a.Sort), returnSort);
            ProcessFunctionDefinition(decl, rank, args.Select(a => a.Id), definition);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        [Command("define-fun-rec")]
        public void DefineFunRec(SmtIdentifier id, IList<(SmtIdentifier Id, SmtSort Sort)> args, SmtSort returnSort, SemgusToken definition)
        {
            var (decl, rank) = ProcessFunctionDeclaration(id, args.Select(a => a.Sort), returnSort);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
            ProcessFunctionDefinition(decl, rank, args.Select(a => a.Id), definition);
        }

        [Command("declare-fun")]
        public void DeclareFun(SmtIdentifier id, IList<SmtSort> args, SmtSort returnSort)
        {
            var (decl, _) = ProcessFunctionDeclaration(id, args, returnSort);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        [Command("declare-const")]
        public void DeclareConst(SmtIdentifier id, SmtSort sort)
        {
            var (decl, _) = ProcessFunctionDeclaration(id, Enumerable.Empty<SmtSort>(), sort);
            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        private (SmtFunction, SmtFunctionRank) ProcessFunctionDeclaration(SmtIdentifier name, IEnumerable<SmtSort> args, SmtSort returnSort)
        {
            var rank = new SmtFunctionRank(returnSort, args.ToArray());
            var decl = new SmtFunction(name, SmtTheory.UserDefined, rank);

            return (decl, rank);
        }

        private void ProcessFunctionDefinition(SmtFunction decl, SmtFunctionRank rank, IEnumerable<SmtIdentifier> arguments, SemgusToken token)
        {
            using var scopeCtx = _scopeProvider.CreateNewScope();

            foreach (var (id, sort) in arguments.Zip(rank.ArgumentSorts))
            {
                scopeCtx.Scope.AddVariableBinding(id, sort, SmtVariableBindingType.Lambda);
            }

            if (!_converter.TryConvert(token, out SmtTerm? term))
            {
                throw new InvalidOperationException($"error: {token.Position}: Cannot convert function term: " + term);
            }

            if (term.Sort != rank.ReturnSort)
            {
                throw new InvalidOperationException("Function return sort doesn't match term sort: " + decl.Name);
            }

            SmtLambdaBinder lambda = new(term, scopeCtx.Scope, arguments);
            decl.AddDefinition(rank, lambda);

            MaybeProcessChcDefinition(decl, rank, lambda);
        }

        private void MaybeProcessChcDefinition(SmtFunction decl, SmtFunctionRank rank, SmtLambdaBinder defn)
        {
            // Rule: must return bool
            var boolSort = _smtCtxProvider.Context.GetSortDeclaration(new SmtIdentifier("Bool"));
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

            // Each pattern is one or more CHC
            void procCHC(SmtMatchBinder binder, SmtTerm term)
            {
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
                        throw new InvalidOperationException("Multiple constraints in CHC.");
                    }
                    // TODO: check to make sure this doesn't contain semantic relations.
                    constraint = c;
                }
                _semgusCtxProvider.Context.AddChc(new SemgusChc(head, relList, constraint!, binder, headBindings.Concat(bodyBindings)));
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
