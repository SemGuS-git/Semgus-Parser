using Semgus.Model.Smt.Terms;

using System.Diagnostics.CodeAnalysis;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Function-like object that can be expanded into other SMT expressions
    /// </summary>
    public class SmtMacro : IApplicable
    {
        /// <summary>
        /// Lambda list of this macro
        /// </summary>
        private readonly IList<MacroParameter> _lambdaList;

        /// <summary>
        /// Expansion function for this macro
        /// </summary>
        private readonly Func<SmtContext, SmtFunctionApplication, IEnumerable<SmtTerm>, SmtTerm> _expander;

        /// <summary>
        /// Function to compute return sort of this macro
        /// </summary>
        private readonly Func<IReadOnlyList<SmtSort>, SmtSort> _returnSortDeriver;

        /// <summary>
        /// Should this macro be expanded by default?
        /// </summary>
        private readonly bool _expandByDefault;

        /// <summary>
        /// Constructs a new SMT macro with the given parameters
        /// </summary>
        /// <param name="name">Macro name</param>
        /// <param name="source">Theory or logic this macro belongs to</param>
        /// <param name="lambdaList">Lambda list for this macro</param>
        /// <param name="returnSortDeriver">Function to compute return sort of this macro</param>
        /// <param name="expander">The expansion function</param>
        public SmtMacro(SmtIdentifier name,
                        ISmtSource source,
                        IEnumerable<MacroParameter> lambdaList,
                        Func<IReadOnlyList<SmtSort>, SmtSort> returnSortDeriver,
                        Func<SmtContext, SmtFunctionApplication, IEnumerable<SmtTerm>, SmtTerm> expander,
                        bool expandByDefault = true)
        {
            Name = name;
            Source = source;
            _lambdaList = lambdaList.ToList();
            _returnSortDeriver = returnSortDeriver;
            _expander = expander;
            _expandByDefault = expandByDefault;
        }

        /// <summary>
        /// Default macro types for declaring common macros.
        /// Corresponds to the :left-assoc, :right-assoc, :chainable,
        /// and :pairwise annotations in SMT-LIB2.
        /// </summary>
        public enum DefaultMacroType
        {
            LeftAssociative = 1,
            RightAssociative,
            Chainable,
            Pairwise
        }

        /// <summary>
        /// Constructs a new macro based on one of the default types
        /// </summary>
        /// <param name="baseFn">Function to expand into</param>
        /// <param name="defaultType">Type of default macro</param>
        /// <exception cref="ArgumentException">Thrown if given invalid macro type or function without valid ranks for that type</exception>
        /// <exception cref="NotImplementedException">Some unusual cases are not yet implemented</exception>
        public SmtMacro(SmtFunction baseFn, DefaultMacroType defaultType, bool expandByDefault = true)
        {
            Name = baseFn.Name;
            Source = baseFn.Source;
            _expandByDefault = expandByDefault;
            SmtFunctionRank? matchingRank = default;
            foreach (var template in baseFn.RankTemplates)
            {
                if (matchingRank is not null) break;
                if (template.Arity != 2) continue;

                switch (defaultType)
                {
                    case DefaultMacroType.Pairwise or DefaultMacroType.Chainable:
                        if (template.ReturnSort.Name == SmtCommonIdentifiers.BoolSortId
                            && template.ArgumentSorts[0] == template.ArgumentSorts[1])
                        {
                            matchingRank = template;
                        }
                        break;

                    case DefaultMacroType.LeftAssociative:
                        if (template.ArgumentSorts[0] == template.ReturnSort)
                        {
                            matchingRank = template;
                        }
                        break;

                    case DefaultMacroType.RightAssociative:
                        if (template.ArgumentSorts[1] == template.ReturnSort)
                        {
                            matchingRank = template;
                        }
                        break;

                    default:
                        throw new ArgumentException("Invalid default macro type: " + defaultType);
                }
            }

            if (matchingRank is null)
            {
                throw new ArgumentException($"No rank found for function {baseFn.Name} valid for default macro of type {defaultType}");
            }

            if (defaultType == DefaultMacroType.RightAssociative
                && matchingRank.ArgumentSorts[0] != matchingRank.ArgumentSorts[1])
            {
                throw new NotImplementedException($"Cannot create default macro for right associative function {baseFn.Name} with non-homogenous input sorts. File an issue on GitHub if you need this.");
                // This is "hard" because we have to have a single parameter at the end of our
                // macro's lambda list _before_ the rest parameter. We can do it, but only if
                // it's actually needed. No theory uses this feature currently.
            }

            _returnSortDeriver = r => matchingRank.ReturnSort;

            var llb = new LambdaListBuilder();
            llb.AddSingle(matchingRank.ArgumentSorts[0])
               .AddSingle(matchingRank.ArgumentSorts[1])
               .AddRest(matchingRank.ArgumentSorts[1]);

            _lambdaList = llb.Build().ToList();
            
            switch (defaultType)
            {
                case DefaultMacroType.LeftAssociative:
                    _expander = (ctx, _, args)
                        => LeftAssociativeExpander(baseFn, matchingRank, ctx, args.ToList());
                    break;

                case DefaultMacroType.RightAssociative:
                    _expander = (ctx, _, args)
                        => RightAssociativeExpander(baseFn, matchingRank, ctx, args.ToList());
                    break;

                case DefaultMacroType.Chainable:
                    _expander = (ctx, _, args)
                        => ChainableExpander(baseFn, matchingRank, ctx, args.ToList());
                    break;

                case DefaultMacroType.Pairwise:
                    throw new NotImplementedException("Pairwise expanders not implemented");

                default:
                    throw new ArgumentException("Not a valid macro type: " + defaultType);
            }
        }

        /// <summary>
        /// Expansion function for left associative default macros
        /// </summary>
        /// <param name="fn">Function to expand into</param>
        /// <param name="rank">Rank of function to expand into</param>
        /// <param name="ctx">SMT context</param>
        /// <param name="args">Macro arguments</param>
        /// <returns>The expanded macro</returns>
        private SmtTerm LeftAssociativeExpander(SmtFunction fn,
                                                SmtFunctionRank rank,
                                                SmtContext ctx,
                                                IList<SmtTerm> args)
        {
            return new SmtFunctionApplication(fn, rank, new List<SmtTerm>()
            {
                args[0],
                args.Count > 2 ?
                  LeftAssociativeExpander(fn, rank, ctx, args.Skip(1).ToList()) :
                  args[1]
            });
        }

        /// <summary>
        /// Expansion function for right associative default macros
        /// </summary>
        /// <param name="fn">Function to expand into</param>
        /// <param name="rank">Rank of function to expand into</param>
        /// <param name="ctx">SMT context</param>
        /// <param name="args">Macro arguments</param>
        /// <returns>The expanded macro</returns>
        private SmtTerm RightAssociativeExpander(SmtFunction fn,
                                                SmtFunctionRank rank,
                                                SmtContext ctx,
                                                IList<SmtTerm> args)
        {
            return new SmtFunctionApplication(fn, rank, new List<SmtTerm>()
            {
                args.Count > 2 ?
                  RightAssociativeExpander(fn, rank, ctx, args.SkipLast(1).ToList()) :
                  args[0],
                args[1]
            });
        }

        /// <summary>
        /// Expansion function for chainable default macros
        /// </summary>
        /// <param name="fn">Function to expand into</param>
        /// <param name="rank">Rank of function to expand into</param>
        /// <param name="ctx">SMT context</param>
        /// <param name="args">Macro arguments</param>
        /// <returns>The expanded macro</returns>
        private SmtTerm ChainableExpander(SmtFunction fn,
                                          SmtFunctionRank rank,
                                          SmtContext ctx,
                                          IList<SmtTerm> args)
        {
            SmtTerm[] children = new SmtTerm[args.Count - 1];
            for (int i = 0; i < args.Count - 1; ++i)
            {
                children[i] = new SmtFunctionApplication(fn, rank, new List<SmtTerm>()
                {
                    args[i],
                    args[i+1]
                });
            }

            return SmtTermBuilder.Apply(ctx, SmtCommonIdentifiers.AndFunctionId, children);
        }

        /// <summary>
        /// Expands this macro
        /// </summary>
        /// <param name="ctx">SMT context</param>
        /// <param name="appl">Application that is being expanded</param>
        /// <param name="args">Macro arguments</param>
        /// <returns>The expanded macro</returns>
        public SmtTerm DoExpand(SmtContext ctx, SmtFunctionApplication appl, IEnumerable<SmtTerm> args)
        {
            return _expander(ctx, appl, args);
        }

        /// <summary>
        /// The name of this macro
        /// </summary>
        public SmtIdentifier Name { get; }

        /// <summary>
        /// The theory this macro belongs to
        /// </summary>
        public ISmtSource Source { get; }

        /// <summary>
        /// Checks if this macro invocation should be expanded or no
        /// </summary>
        /// <param name="ctx">SMT context</param>
        /// <param name="rank">Rank being expanded</param>
        /// <returns>True if should be expanded, false if no</returns>
        public bool ShouldExpand(SmtContext ctx, SmtFunctionRank rank)
        {
            return _expandByDefault; // TODO: configure which macros should be expanded
        }

        /// <summary>
        /// Get a help string about available ranks
        /// </summary>
        /// <returns>Rank help string</returns>
        public string GetRankHelp()
        {
            List<string> argHelp = new();
            foreach (var p in _lambdaList)
            {
                if (p is SingleParameter single)
                {
                    argHelp.Add(single.Sort.Name.ToString());
                }
                else if (p is RestParameter rest)
                {
                    argHelp.Add("&rest");
                    argHelp.Add(rest.Sort.Name.ToString());
                }
                else
                {
                    argHelp.Add("???");
                }
            }
            return $"({string.Join(' ', argHelp)}) -> TBD"; // {_return.Name}";
        }

        /// <summary>
        /// Checks if there is any matching rank that has the given arity
        /// </summary>
        /// <param name="arity">Arity to check</param>
        /// <returns>True if there is a rank that has the given arity</returns>
        public bool IsArityPossible(int arity)
        {
            int poss = 0;
            foreach (var parameter in _lambdaList)
            {
                if (parameter is SingleParameter)
                {
                    poss += 1;
                }
            }
            return arity >= poss;
        }

        /// <summary>
        /// Attempts to get the next macro parameter when checking rank
        /// </summary>
        /// <param name="current">The current parameter</param>
        /// <param name="queue">The parameter queue</param>
        /// <returns>The next macro parameter to consider, or null if none left</returns>
        private MacroParameter? MaybeGetNextMacroParameter(MacroParameter? current, Queue<MacroParameter> queue)
        {
            if (current is null)
            {
                if (!queue.TryDequeue(out current))
                {
                    return default;
                }
            }
            return current;
        }

        /// <summary>
        /// Checks if two sorts match, taking into account resolved wild sorts
        /// </summary>
        /// <param name="a">First sort. Can be a sort parameter</param>
        /// <param name="b">Second sort. Must be a concrete sort</param>
        /// <param name="resolved">Dictionary of resolved sorts</param>
        /// <returns>True if the sorts match</returns>
        private bool CheckSortMatch(SmtSort a, SmtSort b, IDictionary<SmtSort, SmtSort> resolved)
        {
            if (a.IsSortParameter)
            {
                if (resolved.TryGetValue(a, out SmtSort? r))
                {
                    a = r;
                }
                else
                {
                    resolved.Add(a, b);
                    return true;
                }
            }

            if (a is SmtSort.WildcardSort wild)
            {
                return wild.Matches(b);
            }
            else
            {
                return a == b;
            }
        }

        /// <summary>
        /// Tries to resolve a rank for this macro
        /// </summary>
        /// <param name="rank">The resolved rank</param>
        /// <param name="returnSort">The return sort of the expression, if known</param>
        /// <param name="argumentSorts">Sorts of arguments</param>
        /// <returns>True if successfully resolved</returns>
        public bool TryResolveRank([NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            var paramQueue = new Queue<MacroParameter>(_lambdaList);
            MacroParameter? formal = default;
            IDictionary<SmtSort, SmtSort> resolvedSorts = new Dictionary<SmtSort, SmtSort>();
            for (int ix = 0; ix < argumentSorts.Length; ix++)
            {
                formal = MaybeGetNextMacroParameter(formal, paramQueue);
                if (formal is null)
                {
                    // Not enough arguments
                    rank = default;
                    return false;
                }
                var actual = argumentSorts[ix];

                if (formal is SingleParameter single)
                {
                    if (!CheckSortMatch(single.Sort, actual, resolvedSorts))
                    {
                        rank = default;
                        return false;
                    }
                    formal = default; // Get new formal
                }
                else if (formal is RestParameter rest)
                {
                    if (!CheckSortMatch(rest.Sort, actual, resolvedSorts))
                    {
                        rank = default;
                        return false;
                    }
                }
                else
                {
                    rank = default;
                    return false;
                }
            }

            formal = MaybeGetNextMacroParameter(formal, paramQueue);
            if (formal is not null && formal is not RestParameter)
            {
                rank = default;
                return false;
            }

            var computedReturn = _returnSortDeriver(argumentSorts);

            if (returnSort is not null && !CheckSortMatch(computedReturn, returnSort, resolvedSorts))
            {
                rank = default;
                return false;
            }
            else if (returnSort is null)
            {
                if (!computedReturn.IsSortParameter)
                {
                    returnSort = computedReturn;
                }
                else if (!resolvedSorts.TryGetValue(computedReturn, out returnSort))
                {
                    rank = default;
                    return false;
                }
            }

            rank = new SmtFunctionRank(returnSort, argumentSorts);
            return true;
        }

        /// <summary>
        /// Base class for macro parameters
        /// </summary>
        public abstract record MacroParameter;

        /// <summary>
        /// Macro parameter for a single argument of a given sort
        /// </summary>
        /// <param name="Sort">Sort for this parameter</param>
        public record SingleParameter(SmtSort Sort) : MacroParameter;

        /// <summary>
        /// Macro parameter for collecting up the rest of the arguments
        /// </summary>
        /// <param name="Sort">Sort for all the rest of the arguments</param>
        public record RestParameter(SmtSort Sort) : MacroParameter;

        /// <summary>
        /// Helper class for building macro lambda lists
        /// </summary>
        public class LambdaListBuilder
        {
            /// <summary>
            /// The list under construction
            /// </summary>
            private readonly List<MacroParameter> _lambdaList = new();

            /// <summary>
            /// Creates a new lambda list builder
            /// </summary>
            public LambdaListBuilder() { }

            /// <summary>
            /// Adds a single parameter of the given sort
            /// </summary>
            /// <param name="sort">Sort for this parameter</param>
            /// <returns>This builder</returns>
            public LambdaListBuilder AddSingle(SmtSort sort)
            {
                _lambdaList.Add(new SingleParameter(sort));
                return this;
            }

            /// <summary>
            /// Adds a rest parameter of the given sort
            /// </summary>
            /// <param name="sort">Sort for the rest of the arguments</param>
            /// <returns>This builder</returns>
            public LambdaListBuilder AddRest(SmtSort sort)
            {
                _lambdaList.Add(new RestParameter(sort));
                return this;
            }

            /// <summary>
            /// Builds the macro lambda list
            /// </summary>
            /// <returns>The lambda list</returns>
            public IEnumerable<MacroParameter> Build()
            {
                return _lambdaList;
            }
        }
    }
}
