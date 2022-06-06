using System;
using System.Collections.Generic;
using System.IO;

using Semgus.Parser.Reader;

using Semgus.Model.Smt.Terms;
using Microsoft.Extensions.Logging;
using Semgus.Model.Smt.Theories;
using Semgus.Model.Smt;
using Semgus.Model;
using System.Linq;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for adding a new constraint into the SemGuS problem
    /// Syntax: (constraint [predicate])
    /// </summary>
    internal class ConstraintCommand
    {
        private readonly ISmtConverter _converter;
        private readonly ISemgusProblemHandler _problemHandler;
        private readonly ISmtContextProvider _smtProvider;
        private readonly ISemgusContextProvider _semgusProvider;
        private readonly ISmtScopeProvider _scopeProvider;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<ConstraintCommand> _logger;

        public ConstraintCommand(ISmtConverter converter, ISemgusProblemHandler handler, ISmtContextProvider smtProvider, ISemgusContextProvider semgusProvider, ISmtScopeProvider scopeProvider, ISourceMap sourceMap, ILogger<ConstraintCommand> logger)
        {
            _converter = converter;
            _problemHandler = handler;
            _smtProvider = smtProvider;
            _semgusProvider = semgusProvider;
            _scopeProvider = scopeProvider;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        [Command("constraint")]
        public void Constraint(SemgusToken rawPredicate)
        {
            SmtTerm? predicate;
            if (_semgusProvider.Context.AnySygusFeatures)
            {
                predicate = ProcessSygusPredicate(rawPredicate);
            }
            else
            {
                if (!_converter.TryConvert(rawPredicate, out predicate))
                {
                    throw _logger.LogParseErrorAndThrow("Unable to parse constraint predicate.", _sourceMap[rawPredicate]);
                }
            }

            // Only Boolean constraints are valid
            var boolSort = _smtProvider.Context.GetSortOrDie(SmtCommonIdentifiers.BoolSortId, _sourceMap, _logger);
            if (predicate.Sort == boolSort)
            {
                _semgusProvider.Context.AddConstraint(predicate);
                _problemHandler.OnConstraint(_smtProvider.Context, _semgusProvider.Context, predicate);
            }
            else if (predicate.Sort == ErrorSort.Instance || predicate.Accept(new TermErrorSearcher()))
            {
                // No need to log the error again, since it should have been logged
                // when the error term was originally encountered. This is just noise.
                throw new FatalParseException("Term in constraint is in an error state due to previous errors: " + predicate, _sourceMap[predicate]);
            }
            else
            {
                throw _logger.LogParseErrorAndThrow("Term in constraint is not of Bool sort: " + predicate, _sourceMap[predicate]);
            }
        }

        private SmtTerm ProcessSygusPredicate(SemgusToken rawPredicate)
        {
            SmtContext smtCtx = _smtProvider.Context;
            SemgusContext semCtx = _semgusProvider.Context;

            // Bind the universal variables created with `declare-var`
            using var universalScope = _scopeProvider.CreateNewScope();
            var universalBindings = new List<SmtVariableBinding>();
            foreach (var (id, sort) in semCtx.SygusVars)
            {
                if (!universalScope.Scope.TryAddVariableBinding(id, sort, SmtVariableBindingType.Universal, smtCtx, out var binding, out var error))
                {
                    _logger.LogParseErrorAndThrow($"Unable to add implicit SyGuS binding for {id}: {error}", _sourceMap[rawPredicate]);
                }
                universalBindings.Add(binding);
            }

            SmtTerm? predicate;
            smtCtx.Push();
            try
            {
                // Temporarily bind the SyGuS synthfun names
                foreach (var (semTerm, semRel, sygFun, sygRank) in semCtx.SygusSynthFuns)
                {
                    smtCtx.AddFunctionDeclaration(sygFun);
                }

                // Do the actual conversion
                if (!_converter.TryConvert(rawPredicate, out predicate))
                {
                    throw _logger.LogParseErrorAndThrow("Unable to parse constraint predicate.", _sourceMap[rawPredicate]);
                }
            }
            finally
            {
                smtCtx.Pop();
            }

            // Add the universal binding term
            if (universalBindings.Count > 0)
            {
                predicate = SmtTermBuilder.Forall(smtCtx, universalScope.Scope, predicate);
            }

            // Transform the SyGuS function to the SemGuS relation
            foreach (var (semTerm, semRel, sygFun, sygRank) in semCtx.SygusSynthFuns)
            {
                try
                {
                    smtCtx.Push();
                    smtCtx.AddFunctionDeclaration(semRel);
                    var transformer = new FnTransformer(smtCtx, semRel, semTerm, sygFun, sygRank);
                    var (newTerm, data) = predicate.Accept(transformer);
                    predicate = transformer.AddBindings(data, newTerm);
                }
                finally
                {
                    smtCtx.Pop();
                }
            }

            return predicate;
        }

        private record TransformData(SmtFunctionApplication Application, SmtScope Bindings);
        private class FnTransformer : ISmtTermVisitor<(SmtTerm, IList<TransformData>)>
        {
            private readonly SmtContext _ctx;
            private readonly SmtFunction _rel;
            private readonly SmtFunction _term;
            private readonly SmtFunction _func;
            private readonly SmtFunctionRank _rank;

            public FnTransformer(SmtContext ctx, SmtFunction rel, SmtFunction term, SmtFunction func, SmtFunctionRank rank)
            {
                _ctx = ctx;
                _rel = rel;
                _term = term;
                _func = func;
                _rank = rank;
            }

            public SmtTerm AddBindings(IList<TransformData> data, SmtTerm child)
            {
                if (data.Count > 0)
                {
                    SmtTerm subchild = SmtTermBuilder.Apply(_ctx,
                                                            SmtCommonIdentifiers.AndFunctionId,
                                                            data.Select(d => d.Application)
                                                                .Cast<SmtTerm>()
                                                                .Append(child)
                                                                .ToArray());
                    foreach (var datum in data)
                    {
                        subchild = SmtTermBuilder.Exists(_ctx, datum.Bindings, subchild);
                    }
                    child = subchild;
                }
                return child;
            }

            public (SmtTerm, IList<TransformData>) VisitBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral) => (bitVectorLiteral, new List<TransformData>());

            public (SmtTerm, IList<TransformData>) VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral) => (decimalLiteral, new List<TransformData>());

            public (SmtTerm, IList<TransformData>) VisitExistsBinder(SmtExistsBinder existsBinder)
            {
                var (child, data) = existsBinder.Child.Accept(this);
                child = AddBindings(data, child);
                return (new SmtExistsBinder(child, existsBinder.NewScope), new List<TransformData>());
            }

            public (SmtTerm, IList<TransformData>) VisitForallBinder(SmtForallBinder forallBinder)
            {
                var (child, data) = forallBinder.Child.Accept(this);
                child = AddBindings(data, child);
                return (new SmtForallBinder(child, forallBinder.NewScope), new List<TransformData>());
            }

            public (SmtTerm, IList<TransformData>) VisitFunctionApplication(SmtFunctionApplication functionApplication)
            {
                List<SmtTerm> args = new();
                List<TransformData> vars = new();
                foreach (var a in functionApplication.Arguments)
                {
                    var (arg, data) = a.Accept(this);
                    args.Add(arg);
                    vars.AddRange(data);
                }

                if (functionApplication.Definition == _func)
                {
                    SmtScope scope = new(default);
                    SmtIdentifier output = GensymUtils.Gensym("_SyOut", "o");
                    scope.TryAddVariableBinding(output, _rank.ReturnSort, SmtVariableBindingType.Existential, _ctx, out var outputBinding, out var error);

                    List<SmtTerm> arguments = new();
                    arguments.Add(SmtTermBuilder.Apply(_ctx, _term.Name));
                    arguments.AddRange(args);
                    arguments.Add(new SmtVariable(output, outputBinding!));

                    var appl = SmtTermBuilder.Apply(_ctx,
                                                   _rel.Name,
                                                   arguments.ToArray());

                    vars.Add(new((SmtFunctionApplication)appl, scope));
                    return (new SmtVariable(output, outputBinding!), vars);
                }
                else
                {
                    return (new SmtFunctionApplication(functionApplication.Definition, functionApplication.Rank, args), vars);
                }
            }

            public (SmtTerm, IList<TransformData>) VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
            {
                var (child, vars) = lambdaBinder.Child.Accept(this);
                return (new SmtLambdaBinder(child, lambdaBinder.NewScope, lambdaBinder.ArgumentNames), vars);
            }

            public (SmtTerm, IList<TransformData>) VisitLetBinder(SmtLetBinder letBinder)
            {
                var (child, vars) = letBinder.Child.Accept(this);
                return (new SmtLetBinder(child, letBinder.NewScope), vars);
            }

            public (SmtTerm, IList<TransformData>) VisitMatchBinder(SmtMatchBinder matchBinder)
            {
                var (child, vars) = matchBinder.Child.Accept(this);
                return (new SmtMatchBinder(child, matchBinder.NewScope, matchBinder.ParentType, matchBinder.Constructor, matchBinder.Bindings), vars);
            }

            public (SmtTerm, IList<TransformData>) VisitMatchGrouper(SmtMatchGrouper matchGrouper)
            {
                List<TransformData> vars = new();
                List<SmtMatchBinder> binders = new();
                var (term, tVars) = matchGrouper.Term.Accept(this);
                vars.AddRange(tVars);
                foreach (var binder in matchGrouper.Binders)
                {
                    var (nChild, nVars) = binder.Accept(this);
                    vars.AddRange(nVars);
                    binders.Add((SmtMatchBinder)nChild);
                }
                return (new SmtMatchGrouper(term, matchGrouper.Sort, binders), vars);
            }

            public (SmtTerm, IList<TransformData>) VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral) => (numeralLiteral, new List<TransformData>());

            public (SmtTerm, IList<TransformData>) VisitStringLiteral(SmtStringLiteral stringLiteral) => (stringLiteral, new List<TransformData>());

            public (SmtTerm, IList<TransformData>) VisitVariable(SmtVariable variable) => (variable, new List<TransformData>());
        }
    }
}
