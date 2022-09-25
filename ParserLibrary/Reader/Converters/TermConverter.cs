using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Sorts;
using Semgus.Model.Smt.Terms;

namespace Semgus.Parser.Reader.Converters
{
    internal class TermConverter : AbstractConverter
    {
        private readonly DestructuringHelper _destructuringHelper;
        private readonly ISmtConverter _converter;
        private readonly ISmtScopeProvider _scopeProvider;
        private readonly ISmtContextProvider _contextProvider;
        private readonly ISuggestionGenerator _suggestionGenerator;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<TermConverter> _logger;
        
        public TermConverter(DestructuringHelper helper,
                             ISmtConverter converter,
                             ISmtScopeProvider scopeProvider,
                             ISmtContextProvider contextProvider,
                             ISuggestionGenerator suggestionGenerator,
                             ISourceMap sourceMap,
                             ILogger<TermConverter> logger)
        {
            _destructuringHelper = helper;
            _converter = converter;
            _scopeProvider = scopeProvider;
            _contextProvider = contextProvider;
            _suggestionGenerator = suggestionGenerator;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        public override bool CanConvert(Type from, Type to)
        {
            // Converting to specific term types not supported...I think
            // ...but there are some situations where we want to convert to a literal
            return to == typeof(SmtTerm) || to.IsAssignableTo(typeof(SmtLiteral));
        }

        // Note on return value: we only return false if we cannot convert to a term, structurally.
        // If it looks like we should be able to convert and can't, then we return true with to as an ErrorTerm.
        // A.k.a.: False means to try another converter. True + ErrorTerm means we know that this is the right converter, but the user messed up.
        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            // If we're explicitly asked for a literal, do that now
            if (tTo.IsAssignableTo(typeof(SmtLiteral)))
            {
                return ConvertToLiteral(from, out to);
            }

            // First, try bare identifiers. These include symbols, indexed identifiers (_ ...) and sort-qualified identifiers (as ...)
            SmtIdentifier? sid = default;
            QualifiedIdentifier? qid = default;
            if (_converter.TryConvert(from, out qid) || _converter.TryConvert(from, out sid))
            {
                if (qid is null)
                {
                    qid = new QualifiedIdentifier(sid!);
                    _sourceMap[qid] = _sourceMap[sid!];
                }

                if (_scopeProvider.Scope.TryGetVariableBinding(qid.Id, out var binding))
                {
                    to = new SmtVariable(qid.Id, binding);
                }
                else if (_contextProvider.Context.TryGetFunctionDeclaration(qid.Id, out var defn))
                {
                    if (defn.TryResolveRank(out var rank, GetSortOrDie(qid.Sort) /* No arguments */))
                    {
                        to = new SmtFunctionApplication(defn, rank, new List<SmtTerm>());
                    }
                    else
                    {
                        to = new ErrorTerm("Unable to resolve rank: " + qid.Id);
                        _logger.LogParseError("Unable to resolve rank: " + qid.Id, (from as SemgusToken)?.Position);
                    }
                }
                else
                {
                    string msg = "Unable to resolve function or variable: " + qid.Id;
                    to = new ErrorTerm(msg);

                    var similar = _suggestionGenerator.GetVariableSuggestions(qid.Id, _contextProvider.Context, _scopeProvider.Scope).ToList();
                    if (similar.Count > 0)
                    {
                        msg += "\n    Did you mean:\n";
                        foreach (var candidate in similar)
                        {
                            msg += $"     - {candidate}\n";
                        }
                    }
                    _logger.LogParseError(msg, _sourceMap[qid]);
                }
                return true;
            }

            // Function calls and special forms
            else if (from is IConsOrNil form)
            {
                if (form.IsNil())
                {
                    throw new InvalidOperationException("Parsing empty list not allowed in terms.");
                }

                var first = form.First();
                if (first is SymbolToken st)
                {
                    switch (st.Name)
                    {
                        case "!":
                            {
                                // Handle annotations by just adding the annotation
                                if (_converter.TryConvert(form, out AnnotationForm? af))
                                {
                                    if (_converter.TryConvert(af.Attributes, out IList<SmtAttribute>? attributes))
                                    {
                                        foreach (var attr in attributes) {
                                            af.Child.AddAttribute(attr);
                                        }
                                        to = af.Child;
                                        return true;
                                    }
                                }
                                throw new InvalidOperationException("Malformed annotation.");
                            }

                        case "let":
                            {
                                throw new NotImplementedException("Let not yet supported.");
                                if (_converter.TryConvert(form, out LetForm? lf))
                                {
                                    using var scopeCx = _scopeProvider.CreateNewScope();
                                    if (_converter.TryConvert(lf.Child, out SmtTerm? term))
                                    {

                                    }
                                }
                                throw new InvalidOperationException("Malformed let.");
                            }

                        case "exists":
                            {
                                if (_converter.TryConvert(form, out ExistsForm? ef))
                                {
                                    using var scopeCx = _scopeProvider.CreateNewScope();
                                    foreach (var (id, sort) in ef.Bindings)
                                    {
                                        if (!scopeCx.Scope.TryAddVariableBinding(id,
                                                                                 GetSortOrDie(sort),
                                                                                 SmtVariableBindingType.Existential,
                                                                                 _contextProvider.Context,
                                                                                 out var binding,
                                                                                 out var error))
                                        {
                                            _logger.LogParseError("invalid variable name: " + error, _sourceMap[id]);
                                            to = new ErrorTerm(error);
                                            return true;
                                        }
                                    }
                                    if (_converter.TryConvert(ef.Child, out SmtTerm? child))
                                    {
                                        to = new SmtExistsBinder(child, scopeCx.Scope);
                                        return true;
                                    }
                                }
                                _logger.LogParseError("Invalid `exists` form. Expected syntax: `(exists ((<var> <sort>)*) <term>)`.", form.Position);
                                to = new ErrorTerm("Invalid exists form.");
                                return true;
                            }

                        case "forall":
                            {
                                if (_converter.TryConvert(form, out ForallForm? ff))
                                {
                                    using var scopeCx = _scopeProvider.CreateNewScope();
                                    foreach (var (id, sort) in ff.Bindings)
                                    {
                                        if (!scopeCx.Scope.TryAddVariableBinding(id, GetSortOrDie(sort), SmtVariableBindingType.Universal,
                                                                                 _contextProvider.Context,
                                                                                 out var binding,
                                                                                 out var error))
                                        {
                                            _logger.LogParseError("invalid variable name: " + error, _sourceMap[id]);
                                            to = new ErrorTerm(error);
                                            return true;
                                        }
                                    }
                                    if (_converter.TryConvert(ff.Child, out SmtTerm? child))
                                    {
                                        to = new SmtForallBinder(child, scopeCx.Scope);
                                        return true;
                                    }
                                }
                                to = new ErrorTerm("Invalid forall form.");
                                _logger.LogParseError("Invalid `forall` form. Expected syntax: `(forall ((<var> <sort>)*) <term>)`.", form.Position);
                                return true;
                            }

                        case "match":
                            {
                                if (_converter.TryConvert(form, out MatchForm? mf))
                                {
                                    SmtSort argSort = mf.TermToMatch.Sort;
                                    IEnumerable<ISmtConstructor> constructors;
                                    if (argSort is SemgusTermType tt)
                                    {
                                        constructors = tt.Constructors;
                                    }
                                    else if (argSort is SmtDatatype dt)
                                    {
                                        constructors = dt.Constructors;
                                    }
                                    else
                                    {
                                        to = new ErrorTerm("Unsupported match expression. Only valid on terms of type term type.");

                                        // Don't re-log if we've already created an error sort
                                        if (argSort is not ErrorSort)
                                        {
                                            _logger.LogParseError("Unsupported match expression. Only valid on terms of type term type.", form.Position);
                                        }
                                        return true;
                                    }

                                    IList<SmtMatchBinder> binders = new List<SmtMatchBinder>();
                                    foreach (var (pattern, terms) in mf.Patterns)
                                    {
                                        using var scopeCtx = _scopeProvider.CreateNewScope();
                                        IList<SmtMatchVariableBinding> bindings = new List<SmtMatchVariableBinding>();
                                        ISmtConstructor? constructor;
                                        if (_converter.TryConvert(pattern, out SmtIdentifier? symbol))
                                        {
                                            var nullaryCons = constructors.Where(c => c.Name == symbol).ToList();
                                            if (nullaryCons.Any())
                                            {
                                                // Use this pattern. Nothing to bind.
                                                constructor = nullaryCons.First();

                                                // Verify that it is actually a nullary constructor
                                                if (constructor.Children.Count != 0)
                                                {
                                                    string msg = $"Constructor '{constructor.Name}' in match expression expects {constructor.Children.Count} children, but written as nullary (with 0 children)";
                                                    _logger.LogParseError(msg, pattern.Position);
                                                    to = new ErrorTerm(msg);
                                                    return true;
                                                }
                                            }
                                            else
                                            {
                                                // It's just a symbol to bind to the whole term
                                                if (!scopeCtx.Scope.TryAddVariableBinding(symbol,
                                                                                          argSort,
                                                                                          SmtVariableBindingType.Bound,
                                                                                          _contextProvider.Context,
                                                                                          out var vb,
                                                                                          out var error))
                                                {
                                                    _logger.LogParseError($"cannot bind match pattern `{symbol}`: " + error, _sourceMap[symbol]);
                                                    to = new ErrorTerm(error);
                                                    return true;
                                                }

                                                bindings.Add(new SmtMatchVariableBinding(vb, SmtMatchVariableBinding.FullTerm));
                                                constructor = default;
                                            }
                                        }
                                        else if (_converter.TryConvert(pattern, out IList<SmtIdentifier>? consList))
                                        {
                                            if (consList.Count == 0)
                                            {
                                                to = new ErrorTerm("Null pattern not allowed in match.");
                                                _logger.LogParseError("Null pattern not allowed in match.", pattern.Position);
                                                return true;
                                            }
                                            var consId = consList.First();
                                            var conses = constructors.Where(c => c.Name == consId).ToList();
                                            if (!conses.Any())
                                            {
                                                to = new ErrorTerm($"No matching constructor found for type {argSort.Name}: {consId}");
                                                _logger.LogParseError($"No matching constructor found for type {argSort.Name}: {consId}", pattern.Position);
                                                return true;
                                            }
                                            constructor = conses.First(); // There should only be one, since constructors cannot be overloaded
                                            if (constructor.Children.Count != consList.Count - 1)
                                            {
                                                to = new ErrorTerm("Number of pattern arguments does not match structure definition");
                                                _logger.LogParseError("Number of pattern arguments does not match structure definition", pattern.Position);
                                                return true;
                                            }
                                            for (int argIx = 0; argIx < constructor.Children.Count; ++argIx)
                                            {
                                                if (!scopeCtx.Scope.TryAddVariableBinding(consList[argIx + 1],
                                                                                          constructor.Children[argIx],
                                                                                          SmtVariableBindingType.Bound,
                                                                                          _contextProvider.Context,
                                                                                          out var vb,
                                                                                          out var error))
                                                {
                                                    _logger.LogParseError($"cannot bind match variable `{consList[argIx + 1]}`: " + error, _sourceMap[consList[argIx + 1]]);
                                                    to = new ErrorTerm(error);
                                                    return true;
                                                }
                                                bindings.Add(new SmtMatchVariableBinding(vb, argIx));
                                            }
                                        }
                                        else
                                        {
                                            to = new ErrorTerm("Invalid pattern in match: " + pattern);
                                            _logger.LogParseError("Invalid pattern in match: " + pattern, pattern.Position);
                                            return true;
                                        }

                                        List<SmtTerm> convTerms = new();
                                        foreach (var term in terms)
                                        {
                                            if (_converter.TryConvert(term, out SmtTerm? convTerm))
                                            {
                                                convTerms.Add(convTerm);
                                            }
                                            else
                                            {
                                                to = new ErrorTerm("Unable to convert match term to actual term.");
                                                _logger.LogParseError("Unable to convert match term to actual term.", pattern.Position);
                                                return true;
                                            }
                                        }

                                        // Make sure all terms are of type bool
                                        if (convTerms.Any(t => t.Sort == ErrorSort.Instance))
                                        {
                                            string msg = $"Pattern {constructor!.Name.Symbol} in match expression has error terms.";
                                            to = new ErrorTerm(msg);
                                            return true;
                                        }

                                        if (convTerms.Count == 1)
                                        {
                                            binders.Add(new SmtMatchBinder(convTerms[0], scopeCtx.Scope, argSort, constructor, bindings));
                                        }
                                        else
                                        {
                                            _contextProvider.Context.TryGetFunctionDeclaration(SmtCommonIdentifiers.OrFunctionId, out IApplicable? orf);
                                            var boolsort = GetSortOrDie(SmtCommonIdentifiers.BoolSortId);

                                            // Make sure all terms are of type bool
                                            if (convTerms.Any(t => t.Sort != boolsort))
                                            {
                                                string msg = $"Not all terms in pattern {constructor!.Name.Symbol} are of sort Bool.";
                                                to = new ErrorTerm(msg);
                                                _logger.LogParseError(msg, pattern.Position);
                                                return true;
                                            }

                                            if (!orf!.TryResolveRank(out var rank, boolsort, Enumerable.Repeat(boolsort, convTerms.Count).ToArray()))
                                            {
                                                throw new InvalidOperationException("Too many terms to match pattern.");
                                            }
                                            binders.Add(new SmtMatchBinder(new SmtFunctionApplication(orf, rank, convTerms), scopeCtx.Scope, argSort, constructor, bindings));
                                        }
                                    }

                                    if (binders.Count == 0)
                                    {
                                        to = new ErrorTerm("No binders in match expression.");
                                        _logger.LogParseError("No binders in match expression.", form.Position);
                                        return true;
                                    }
                                    SmtSort retSort = binders.First().Sort;
                                    if (binders.Any(b => b.Sort != retSort))
                                    {
                                        to = new ErrorTerm("Sort mismatch on match patterns.");
                                        _logger.LogParseError("Sort mismatch on match patterns.", form.Position);
                                        return true;
                                    }

                                    to = new SmtMatchGrouper(mf.TermToMatch, retSort, binders);
                                    return true;
                                }
                                to = new ErrorTerm("Invalid match form.");
                                _logger.LogParseError("Invalid match form.", form.Position);
                                return true;
                            }

                    }
                }

                {
                    // Function application
                    if (_converter.TryConvert(form, out ApplicationForm? af))
                    {
                        var argTerms = af.Arguments.Select((st) =>
                        {
                            if (_converter.TryConvert(st, out SmtTerm? term))
                            {
                                return term;
                            }
                            else
                            {
                                _logger.LogParseError("Cannot process term in function application: " + st, st.Position);
                                return new ErrorTerm("Cannot process term: " + st);
                            }
                        });

                        if (_contextProvider.Context.TryGetFunctionDeclaration(af.Id.Id, out IApplicable? defn))
                        {
                            var args = af.Arguments.Select(a =>
                            {
                                if (_converter.TryConvert(a, out SmtTerm? term))
                                {
                                    return term;
                                }
                                else
                                {
                                    _logger.LogParseError("Unable to construct term in function application: " + a, a.Position);
                                    return new ErrorTerm("Unable to construct term: " + a);
                                }
                            }).ToList();

                            SmtSort[] argSorts = args.Select(a => a.Sort).ToArray();
                            if (argSorts.Any(s => s == ErrorSort.Instance))
                            {
                                to = new ErrorTerm("Error in function application arguments.");
                                return true;
                            }

                            if (defn.TryResolveRank(out var rank, GetSortOrDie(af.Id.Sort), argSorts))
                            {
                                to = new SmtFunctionApplication(defn, rank, args);
                            }
                            else
                            {
                                string msg = "Unable to resolve rank for: " + af.Id.Id;
                                to = new ErrorTerm(msg);
                                msg += $"\n  Looking for signature ({string.Join(' ', argSorts.Select(s => s.Name))}) -> {af.Id.Sort?.Name.ToString() ?? "TBD"}";
                                msg += defn.GetRankHelp();
                                _logger.LogParseError(msg, _sourceMap[af.Id.Id]);
                            }
                        }
                        else
                        {
                            string msg = "Cannot find function definition for: " + af.Id.Id;
                            to = new ErrorTerm(msg);
                            var suggestions = _suggestionGenerator.GetFunctionSuggestions(af.Id.Id, _contextProvider.Context, af.Arguments.Count).ToList();
                            if (suggestions.Count > 0)
                            {
                                msg += "\n    Did you mean:\n";
                                foreach (var suggestion in suggestions)
                                {
                                    msg += $"     - {suggestion}\n";
                                }
                            }
                            _logger.LogParseError(msg, _sourceMap[af.Id.Id]);
                        }
                    }
                    else
                    {
                        to = new ErrorTerm("Expected a function identifier, but got: " + form.First());
                        _logger.LogParseError("Expected a function identifier, but got: " + form.First(), form.Position);
                    }
                }
                return true;
            }
            else
            // Finally, literals.
            {
                return ConvertToLiteral(from, out to);
            }
        }

        private bool ConvertToLiteral(object from, out object? to)
        {
            switch (from)
            {
                case NumeralToken nt:
                    to = new SmtNumeralLiteral(_contextProvider.Context, nt.Value);
                    return true;

                case DecimalToken dt:
                    to = new SmtDecimalLiteral(_contextProvider.Context, dt.Value);
                    return true;

                case StringToken st:
                    to = new SmtStringLiteral(_contextProvider.Context, st.Value);
                    return true;

                case BitVectorToken bvt:
                    to = new SmtBitVectorLiteral(_contextProvider.Context, bvt.Value);
                    return true;

                default:
                    to = default;
                    return false;
            }
        }

        [return: NotNullIfNotNull("id")]
        private SmtSort? GetSortOrDie(SmtSortIdentifier? id) => _contextProvider.Context.GetSortOrDie(id, _sourceMap, _logger);

        // Note: child terms for binders are SemgusTokens, since we need to update the scope before parsing them.
        private record AnnotationForm([Exactly("!")] SmtIdentifier _, SmtTerm Child, [Rest] IList<SemgusToken> Attributes);
        private record LetForm([Exactly("let")] SmtIdentifier _, IList<(SmtIdentifier, SemgusToken)> Bindings, SemgusToken Child);
        private record ForallForm([Exactly("forall")] SmtIdentifier _, IList<(SmtIdentifier, SmtSortIdentifier)> Bindings, SemgusToken Child);
        private record ExistsForm([Exactly("exists")] SmtIdentifier _, IList<(SmtIdentifier, SmtSortIdentifier)> Bindings, SemgusToken Child);
        private record MatchPattern(SemgusToken Pattern, [Rest] IList<SemgusToken> Terms);
        private record MatchForm([Exactly("match")] SmtIdentifier _, SmtTerm TermToMatch, IList<MatchPattern> Patterns);
        private record QualifiedIdentifier([Exactly("as")] SymbolToken? _, SmtIdentifier Id, SmtSortIdentifier? Sort)
        {
            public QualifiedIdentifier(SmtIdentifier Id) : this(null, Id, null) { }
        }
        private record ApplicationForm(QualifiedIdentifier Id, [Rest] IList<SemgusToken> Arguments)
        {
            public ApplicationForm(SmtIdentifier id, [Rest] IList<SemgusToken> arguments) : this(new QualifiedIdentifier(id), arguments) { }
        }
    }
}
