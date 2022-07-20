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
using Semgus.Model.Smt.Transforms;

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
                // Don't add the bindings if not referenced
                if (predicate.Accept(new VariableSearcher(universalBindings.Select(b => b.Id))))
                {
                    predicate = SmtTermBuilder.Forall(smtCtx, universalScope.Scope, predicate);
                }
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

        private class VariableSearcher : ISmtTermVisitor<bool>
        {
            private readonly List<SmtIdentifier> _names;

            public VariableSearcher(IEnumerable<SmtIdentifier> names)
            {
                _names = names.ToList();
            }

            public bool VisitBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral)
                => false;

            public bool VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral)
                => false;

            public bool VisitExistsBinder(SmtExistsBinder existsBinder)
                => existsBinder.Child.Accept(this);

            public bool VisitForallBinder(SmtForallBinder forallBinder)
                => forallBinder.Child.Accept(this);

            public bool VisitFunctionApplication(SmtFunctionApplication functionApplication)
            {
                bool anyVariables = false;
                foreach (var arg in functionApplication.Arguments)
                {
                    anyVariables |= arg.Accept(this);
                }
                return anyVariables;
            }

            public bool VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
            {
                throw new NotImplementedException();
            }

            public bool VisitLetBinder(SmtLetBinder letBinder)
            {
                throw new NotImplementedException();
            }

            public bool VisitMatchBinder(SmtMatchBinder matchBinder)
            {
                throw new NotImplementedException();
            }

            public bool VisitMatchGrouper(SmtMatchGrouper matchGrouper)
            {
                throw new NotImplementedException();
            }

            public bool VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral)
            {
                return false;
            }

            public bool VisitStringLiteral(SmtStringLiteral stringLiteral)
            {
                return false;
            }

            public bool VisitVariable(SmtVariable variable)
            {
                foreach (var name in _names)
                {
                    if (variable.Name == name)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private record TransformData(SmtFunctionApplication Application, SmtScope Bindings);
        private class FnTransformer : SmtTermWalker<IList<TransformData>>
        {
            protected override IList<TransformData> MergeData(SmtTerm root, IEnumerable<IList<TransformData>> data)
            {
                List<TransformData> mergedData = new();
                foreach (var datum in data)
                {
                    mergedData.AddRange(datum);
                }
                return mergedData;
            }

            private readonly SmtContext _ctx;
            private readonly SmtFunction _rel;
            private readonly SmtFunction _term;
            private readonly SmtFunction _func;
            private readonly SmtFunctionRank _rank;

            public FnTransformer(SmtContext ctx, SmtFunction rel, SmtFunction term, SmtFunction func, SmtFunctionRank rank)
                : base(() => new List<TransformData>())
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

            public override (SmtTerm, IList<TransformData>) OnExistsBinder(SmtExistsBinder existsBinder, SmtTerm child, IList<TransformData> data)
            {
                return base.OnExistsBinder(existsBinder, AddBindings(data, child), new List<TransformData>());
            }

            public override (SmtTerm, IList<TransformData>) OnForallBinder(SmtForallBinder forallBinder, SmtTerm child, IList<TransformData> data)
            {
                return base.OnForallBinder(forallBinder, AddBindings(data, child), new List<TransformData>());
            }

            public override (SmtTerm, IList<TransformData>) OnFunctionApplication(SmtFunctionApplication appl, IReadOnlyList<SmtTerm> arguments, IReadOnlyList<IList<TransformData>> up)
            {
                if (appl.Definition == _func)
                {
                    SmtScope scope = new(default);
                    SmtIdentifier output = GensymUtils.Gensym("_SyOut", "o");
                    scope.TryAddVariableBinding(output, _rank.ReturnSort, SmtVariableBindingType.Existential, _ctx, out var outputBinding, out var error);

                    List<SmtTerm> newArguments = new();
                    newArguments.Add(SmtTermBuilder.Apply(_ctx, _term.Name));
                    newArguments.AddRange(arguments);
                    newArguments.Add(new SmtVariable(output, outputBinding!));

                    var newAppl = SmtTermBuilder.Apply(_ctx,
                                                   _rel.Name,
                                                   newArguments.ToArray());

                    var data = MergeData(newAppl, up);
                    data.Add(new((SmtFunctionApplication)newAppl, scope));
                    return (new SmtVariable(output, outputBinding!), data);
                }
                else
                {
                    return base.OnFunctionApplication(appl, arguments, up);
                }
            }
        }
    }
}
