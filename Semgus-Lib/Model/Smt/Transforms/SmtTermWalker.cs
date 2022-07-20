﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Terms;

namespace Semgus.Model.Smt.Transforms
{
    /// <summary>
    /// Code walker for SMT terms. There are two abstractions in this class:
    ///  - the Visit* methods
    ///  - the On* methods
    /// Both work cooperatively and a sub-class can choose to override either.
    /// Difference: the On* methods abstract the tree traversal and are called with
    /// the results after traversing all children. The Visit* methods are responsible
    /// for recursively invoking this walker on children (and optionally calling the
    /// On* methods, if required).
    /// </summary>
    /// <typeparam name="TData">Data passed up the tree traversal</typeparam>
    public abstract class SmtTermWalker<TData> : ISmtTermVisitor<(SmtTerm, TData)>
    {
        /// <summary>
        /// Factory for generating initial upward data
        /// </summary>
        private readonly Func<SmtTerm, TData> _defaultDataFactory;

        /// <summary>
        /// Creates a new code walker with an initial upward data factory
        /// </summary>
        /// <param name="defaultDataFactory">The upward data factory</param>
        public SmtTermWalker(Func<SmtTerm, TData> defaultDataFactory)
        {
            _defaultDataFactory = defaultDataFactory;
        }

        /// <summary>
        /// Creates a new code walker with an initial upward data factory
        /// </summary>
        /// <param name="defaultDataFactory">The upward data factory</param>
        public SmtTermWalker(Func<TData> defaultDataFactory)
        {
            _defaultDataFactory = _ => defaultDataFactory();
        }

        /// <summary>
        /// Creates a new code walker with a fixed piece of initial upward data
        /// </summary>
        /// <param name="defaultDataFactory">The upward data</param>
        public SmtTermWalker(TData defaultData)
        {
            _defaultDataFactory = _ => defaultData;
        }

        /// <summary>
        /// Merges upward data from siblings
        /// </summary>
        /// <param name="root">The term currently being traversed</param>
        /// <param name="data">The upward data to merge</param>
        /// <returns>Merged upward data. Defaults to the initial data.</returns>
        protected virtual TData MergeData(SmtTerm root, IEnumerable<TData> data)
            => _defaultDataFactory(root);

        public virtual (SmtTerm, TData) OnBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral)
            => (bitVectorLiteral, _defaultDataFactory(bitVectorLiteral));

        public (SmtTerm, TData) VisitBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral)
            => OnBitVectorLiteral(bitVectorLiteral);

        public virtual (SmtTerm, TData) OnDecimalLiteral(SmtDecimalLiteral decimalLiteral)
            => (decimalLiteral, _defaultDataFactory(decimalLiteral));

        public (SmtTerm, TData) VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral)
            => OnDecimalLiteral(decimalLiteral);

        public virtual void OnExistsScope(SmtScope scope) { }

        public virtual (SmtTerm, TData) OnExistsBinder(SmtExistsBinder existsBinder, SmtTerm child, TData up)
        {
            if (child != existsBinder.Child)
            {
                return (new SmtExistsBinder(child, existsBinder.NewScope), up);
            }
            else
            {
                return (existsBinder, up);
            }
        }

        public (SmtTerm, TData) VisitExistsBinder(SmtExistsBinder existsBinder)
        {
            OnExistsScope(existsBinder.NewScope);
            var (child, data) = existsBinder.Child.Accept(this);
            return OnExistsBinder(existsBinder, child, data);
        }

        public virtual void OnForallScope(SmtScope scope) { }

        public virtual (SmtTerm, TData) OnForallBinder(SmtForallBinder forallBinder, SmtTerm child, TData up)
        {
            if (child != forallBinder.Child)
            {
                return (new SmtForallBinder(child, forallBinder.NewScope), up);
            }
            else
            {
                return (forallBinder, up);
            }
        }

        public (SmtTerm, TData) VisitForallBinder(SmtForallBinder forallBinder)
        {
            OnForallScope(forallBinder.NewScope);
            var (child, data) = forallBinder.Child.Accept(this);
            return OnForallBinder(forallBinder, child, data);
        }

        public virtual (SmtTerm, TData) OnFunctionApplication(SmtFunctionApplication appl, IReadOnlyList<SmtTerm> arguments, IReadOnlyList<TData> up)
        {
            if (appl.Arguments.Zip(arguments).Where(x => x.First != x.Second).Any())
            {
                for (int i = 0; i < appl.Arguments.Count; ++i)
                {
                    if (appl.Arguments[i].Sort != arguments[i].Sort)
                    {
                        throw new InvalidOperationException($"Invalid walker: argument sort changed from {appl.Arguments[i].Sort} to {arguments[i].Sort}");
                    }
                }
                var newAppl = new SmtFunctionApplication(appl.Definition, appl.Rank, arguments.ToList());
                return (newAppl, MergeData(newAppl, up));
            }
            else
            {
                return (appl, MergeData(appl, up));
            }
        }

        public (SmtTerm, TData) VisitFunctionApplication(SmtFunctionApplication functionApplication)
        {
            List<SmtTerm> args = new();
            List<TData> data = new();
            foreach (var a in functionApplication.Arguments)
            {
                var (arg, datum) = a.Accept(this);
                args.Add(arg);
                data.Add(datum);
            }

            return OnFunctionApplication(functionApplication, args, data);
        }

        public (SmtTerm, TData) VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
        {
            var (child, data) = lambdaBinder.Child.Accept(this);
            if (child != lambdaBinder.Child)
            {
                return (new SmtLambdaBinder(child, lambdaBinder.NewScope, lambdaBinder.ArgumentNames), data);
            }
            else
            {
                return (lambdaBinder, data);
            }
        }

        public (SmtTerm, TData) VisitLetBinder(SmtLetBinder letBinder)
        {
            // TODO: when we have let support
            var (child, data) = letBinder.Child.Accept(this);
            if (child != letBinder.Child)
            {
                return (new SmtLetBinder(child, letBinder.NewScope), data);
            }
            else
            {
                return (letBinder, data);
            }
        }

        public (SmtTerm, TData) VisitMatchBinder(SmtMatchBinder matchBinder)
        {
            var (child, data) = matchBinder.Child.Accept(this);
            if (child != matchBinder.Child)
            {
                return (new SmtMatchBinder(child, matchBinder.NewScope, matchBinder.ParentType, matchBinder.Constructor, matchBinder.Bindings), data);
            }
            else
            {
                return (matchBinder, data);
            }
        }

        public (SmtTerm, TData) VisitMatchGrouper(SmtMatchGrouper matchGrouper)
        {
            List<TData> data = new();
            List<SmtMatchBinder> binders = new();
            var (term, datum) = matchGrouper.Term.Accept(this);
            data.Add(datum);
            foreach (var binder in matchGrouper.Binders)
            {
                var (nChild, nDatum) = binder.Accept(this);
                data.Add(nDatum);
                binders.Add((SmtMatchBinder)nChild);
            } // TODO: Don't make a new term if nothing changed
            var newMatchGrouper = new SmtMatchGrouper(term, matchGrouper.Sort, binders);
            return (newMatchGrouper, MergeData(newMatchGrouper, data));
        }

        public (SmtTerm, TData) OnNumeralLiteral(SmtNumeralLiteral numeralLiteral)
            => (numeralLiteral, _defaultDataFactory(numeralLiteral));

        public (SmtTerm, TData) VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral)
            => OnNumeralLiteral(numeralLiteral);

        public (SmtTerm, TData) OnStringLiteral(SmtStringLiteral stringLiteral)
            => (stringLiteral, _defaultDataFactory(stringLiteral));

        public (SmtTerm, TData) VisitStringLiteral(SmtStringLiteral stringLiteral)
            => OnStringLiteral(stringLiteral);

        public (SmtTerm, TData) OnVariable(SmtVariable variable)
            => (variable, _defaultDataFactory(variable));

        public (SmtTerm, TData) VisitVariable(SmtVariable variable)
            => OnVariable(variable);
    }
}
