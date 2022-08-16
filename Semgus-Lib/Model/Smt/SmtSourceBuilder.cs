using Semgus.Model.Smt.Terms;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Builder for creating SMT sources
    /// </summary>
    internal class SmtSourceBuilder
    {
        /// <summary>
        /// Source being built
        /// </summary>
        private readonly ISmtSource _source;

        /// <summary>
        /// Function dictionary
        /// </summary>
        private readonly Dictionary<SmtIdentifier, SmtFunction> _functions;

        /// <summary>
        /// Names of functions that the source declares but cannot construct ahead of time
        /// </summary>
        private readonly HashSet<SmtIdentifier> _onTheFlyFunctions;

        /// <summary>
        /// Macro dictionary
        /// </summary>
        private readonly Dictionary<SmtIdentifier, SmtMacro> _macros;

        /// <summary>
        /// Sort dictionary
        /// </summary>
        private readonly Dictionary<SmtIdentifier, SmtSort> _sorts;

        /// <summary>
        /// Creates a new source builder for the given source
        /// </summary>
        /// <param name="source">Source to build</param>
        public SmtSourceBuilder(ISmtSource source)
        {
            _source = source;
            _functions = new();
            _onTheFlyFunctions = new();
            _macros = new();
            _sorts = new();
        }

        /// <summary>
        /// The last function added
        /// </summary>
        private SmtFunction? _lastFunction = default;

        /// <summary>
        /// The last sort added
        /// </summary>
        private SmtSort? _lastSort = default;

        /// <summary>
        /// The last macro added
        /// </summary>
        private SmtMacro? _lastMacro = default;

        /// <summary>
        /// Declares the given function as the most recently built object
        /// </summary>
        /// <param name="fun">Built function</param>
        private void Current(SmtFunction fun)
        {
            ClearCurrent();
            _lastFunction = fun;
        }

        /// <summary>
        /// Declares the given sort as the most recently built object
        /// </summary>
        /// <param name="sort">Built sort</param>
        private void Current(SmtSort sort)
        {
            ClearCurrent();
            _lastSort = sort;
        }

        /// <summary>
        /// Declares the given macro as the most recently built object
        /// </summary>
        /// <param name="macro">Built macro</param>
        private void Current(SmtMacro macro)
        {
            ClearCurrent();
            _lastMacro = macro;
        }

        /// <summary>
        /// Clears all current objects
        /// </summary>
        private void ClearCurrent()
        {
            _lastFunction = default;
            _lastMacro = default;
            _lastSort = default;
        }

        /// <summary>
        /// Adds a function to this builder
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="val">Validation function</param>
        /// <param name="valCmt">Validation comment</param>
        /// <param name="retCalc">Return value calculator</param>
        /// <param name="ret">Template return value</param>
        /// <param name="args">Argument sorts</param>
        /// <returns>This builder</returns>
        public SmtSourceBuilder AddFn(string name,
                          Func<SmtFunctionRank, bool> val,
                          string? valCmt,
                          Func<SmtFunctionRank, SmtSort> retCalc,
                          SmtSort ret,
                          params SmtSort[] args)
        {
            SmtIdentifier id = new(name);
            SmtFunction? fun;
            if (_functions.TryGetValue(id, out fun))
            {
                fun.AddRankTemplate(new SmtFunctionRank(ret, args)
                {
                    Validator = val,
                    ReturnSortDeriver = retCalc,
                    ValidationComment = valCmt
                });
            }
            else
            {
                fun = new SmtFunction(id, _source, new SmtFunctionRank(ret, args)
                {
                    Validator = val,
                    ReturnSortDeriver = retCalc,
                    ValidationComment = valCmt
                });
                _functions.Add(id, fun);
            }
            Current(fun);
            return this;
        }

        /// <summary>
        /// Adds a function to this builder
        /// </summary>
        /// <param name="name">The function name</param>
        /// <param name="ret">The return sort</param>
        /// <param name="args">The argument sorts</param>
        /// <returns>This builder</returns>
        public SmtSourceBuilder AddFn(string name, SmtSort ret, params SmtSort[] args)
            => AddFn(new SmtIdentifier(name), ret, args);

        /// <summary>
        /// Adds a function to this builder
        /// </summary>
        /// <param name="id">The function identifier</param>
        /// <param name="ret">The return sort</param>
        /// <param name="args">The argument sorts</param>
        /// <returns>This builder</returns>
        public SmtSourceBuilder AddFn(SmtIdentifier id, SmtSort ret, params SmtSort[] args)
        {
            SmtFunction? fun;
            if (_functions.TryGetValue(id, out fun))
            {
                fun.AddRankTemplate(new SmtFunctionRank(ret, args));
            }
            else
            {
                fun = new SmtFunction(id, _source, new SmtFunctionRank(ret, args));
                _functions.Add(id, fun);
            }
            Current(fun);
            return this;
        }

        /// <summary>
        /// Adds a definition-missing hook to the most recently built function
        /// </summary>
        /// <param name="defnMissing">Definition missing hook</param>
        /// <returns>This builder</returns>
        /// <exception cref="InvalidOperationException">Thrown if most recently built object was not a function</exception>
        public SmtSourceBuilder DefinitionMissing(Func<SmtContext, SmtFunction, SmtFunctionRank, SmtLambdaBinder> defnMissing)
        {
            if (_lastFunction == null)
            {
                throw new InvalidOperationException($"{nameof(DefinitionMissing)} only valid after adding a function.");
            }
            _lastFunction.DefinitionMissingHook = defnMissing;
            return this;
        }

        /// <summary>
        /// Adds a function that is provided by the source, but must be constucted on-the-fly
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>This builder</returns>
        public SmtSourceBuilder AddOnTheFlyFn(string name)
        {
            _onTheFlyFunctions.Add(new(name));
            ClearCurrent();
            return this;
        }

        /// <summary>
        /// Adds a default macro
        /// </summary>
        /// <param name="id">Macro name. There must be a function already declared with this name</param>
        /// <param name="defType">Type of default definition</param>
        /// <returns>This builder</returns>
        public SmtSourceBuilder AddMacro(SmtIdentifier id, SmtMacro.DefaultMacroType defType)
        {
            SmtMacro macro = new SmtMacro(_functions[id], defType, expandByDefault: false);
            _macros.Add(id, macro);
            Current(macro);
            return this;
        }

        /// <summary>
        /// Adds a sort
        /// </summary>
        /// <param name="sort">Sort to add</param>
        /// <returns>This builder</returns>
        public SmtSourceBuilder AddSort(SmtSort sort)
        {
            _sorts.Add(sort.Name.Name, sort);
            Current(sort);
            return this;
        }

        /// <summary>
        /// Gets a dictionary of all available functions provided by this source
        /// </summary>
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions
            => _functions.ToDictionary(kvp => kvp.Key, kvp => (IApplicable)kvp.Value);

        /// <summary>
        /// Gets a set of all function symbols provided by this source
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols
            => new HashSet<SmtIdentifier>(_functions.Keys.Concat(_onTheFlyFunctions));

        /// <summary>
        /// Gets a dictionary of all available macros provided by this source
        /// </summary>
        public IReadOnlyDictionary<SmtIdentifier, SmtMacro> Macros
            => _macros;

        /// <summary>
        /// Gets a dictionary of all available sorts provided by this source
        /// </summary>
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts => _sorts;

        /// <summary>
        /// Gets a set of all sort symbols provided by this source
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols
            => new HashSet<SmtIdentifier>(_sorts.Keys);

        // Common lambdas
        /// <summary>
        /// Rank validator that always returns true
        /// </summary>
        public static Func<SmtFunctionRank, bool> NoRankValidation
            => r => true;

        /// <summary>
        /// Rank validator that checks that argument sorts are equal
        /// </summary>
        public static Func<SmtFunctionRank, bool> CheckArgumentSortsEqual
            => r => r.ArgumentSorts[0] == r.ArgumentSorts[1]; // TODO: check all args

        /// <summary>
        /// Sort deriver that chooses the first argument sort
        /// </summary>
        public static Func<SmtFunctionRank, SmtSort> UseFirstArgumentSort
            => r => r.ArgumentSorts[0];

        /// <summary>
        /// Sort deriver that chooses the return sort
        /// </summary>
        public static Func<SmtFunctionRank, SmtSort> UseReturnSort
            => r => r.ReturnSort;
    }
}
