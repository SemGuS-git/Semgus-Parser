using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

#nullable enable

namespace Semgus.Parser.Reader.Converters
{
    internal class TermConverter : AbstractConverter
    {
        private readonly DestructuringHelper _destructuringHelper;
        private readonly SmtConverter _converter;
        private readonly ISmtScopeProvider _scopeProvider;
        private readonly ISmtContextProvider _contextProvider;
        private readonly ILogger<TermConverter> _logger;
        
        public TermConverter(DestructuringHelper helper, SmtConverter converter, ISmtScopeProvider scopeProvider, ISmtContextProvider contextProvider, ILogger<TermConverter> logger)
        {
            _destructuringHelper = helper;
            _converter = converter;
            _scopeProvider = scopeProvider;
            _contextProvider = contextProvider;
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
                }

                if (_scopeProvider.Scope.TryGetVariableBinding(qid.Id, out var binding))
                {
                    to = new SmtVariable(qid.Id, binding);
                }
                else if (_contextProvider.Context.TryGetFunctionDeclaration(qid.Id, out var defn))
                {
                    if (defn.TryResolveRank(out var rank, qid.Sort /* No arguments */))
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
                    to = new ErrorTerm("Unable to resolve function or variable: " + qid.Id);
                    _logger.LogParseError("Unable to resolve function or variable: " + qid.Id, (from as SemgusToken)?.Position);
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
                                        scopeCx.Scope.AddVariableBinding(id, sort, SmtVariableBindingType.Existential);
                                    }
                                    if (_converter.TryConvert(ef.Child, out SmtTerm? child))
                                    {
                                        to = new SmtExistsBinder(child, scopeCx.Scope);
                                        return true;
                                    }
                                }
                                _logger.LogParseError("Invalid `exists` form.", form.Position);
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
                                        scopeCx.Scope.AddVariableBinding(id, sort, SmtVariableBindingType.Universal);
                                    }
                                    if (_converter.TryConvert(ff.Child, out SmtTerm? child))
                                    {
                                        to = new SmtExistsBinder(child, scopeCx.Scope);
                                        return true;
                                    }
                                }
                                to = new ErrorTerm("Invalid forall form.");
                                _logger.LogParseError("Invalid `forall` form.", form.Position);
                                return true;
                            }

                        case "match":
                            {
                                if (_converter.TryConvert(form, out MatchForm? mf))
                                {
                                    SmtSort argSort = mf.TermToMatch.Sort;
                                    if (argSort is not SemgusTermType tt)
                                    {
                                        to = new ErrorTerm("Unsupported match expression. Only valid on terms of type term type.");
                                        _logger.LogParseError("Unsupported match expression. Only valid on terms of type term type.", form.Position);
                                        return true;
                                    }

                                    IList<SmtMatchBinder> binders = new List<SmtMatchBinder>();
                                    foreach (var (pattern, terms) in mf.Patterns)
                                    {
                                        using var scopeCtx = _scopeProvider.CreateNewScope();
                                        IList<SmtMatchVariableBinding> bindings = new List<SmtMatchVariableBinding>();
                                        SemgusTermType.Constructor? constructor;
                                        if (_converter.TryConvert(pattern, out SmtIdentifier? symbol))
                                        {
                                            var nullaryCons = tt.Constructors.Where(c => c.Operator == symbol).ToList();
                                            if (nullaryCons.Any())
                                            {
                                                // Use this pattern. Nothing to bind.
                                                constructor = nullaryCons.First();

                                                // Verify that it is actually a nullary constructor
                                                if (constructor.Children.Length != 0)
                                                {
                                                    string msg = $"Constructor '{constructor.Operator}' in match expression expects {constructor.Children.Length} children, but written as nullary (with 0 children)";
                                                    _logger.LogParseError(msg, pattern.Position);
                                                    to = new ErrorTerm(msg);
                                                    return true;
                                                }
                                            }
                                            else
                                            {
                                                // It's just a symbol to bind to the whole term
                                                var vb = scopeCtx.Scope.AddVariableBinding(symbol, argSort, SmtVariableBindingType.Bound);
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
                                            var conses = tt.Constructors.Where(c => c.Operator == consId).ToList();
                                            if (!conses.Any())
                                            {
                                                to = new ErrorTerm($"No matching constructor found for type {tt.Name}: {consId}");
                                                _logger.LogParseError($"No matching constructor found for type {tt.Name}: {consId}", pattern.Position);
                                                return true;
                                            }
                                            constructor = conses.First(); // There should only be one, since constructors cannot be overloaded
                                            if (constructor.Children.Length != consList.Count - 1)
                                            {
                                                to = new ErrorTerm("Number of pattern arguments does not match structure definition");
                                                _logger.LogParseError("Number of pattern arguments does not match structure definition", pattern.Position);
                                                return true;
                                            }
                                            for (int argIx = 0; argIx < constructor.Children.Length; ++argIx)
                                            {
                                                var vb = scopeCtx.Scope.AddVariableBinding(consList[argIx + 1], constructor.Children[argIx], SmtVariableBindingType.Bound);
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
                                        if (convTerms.Count == 1)
                                        {
                                            binders.Add(new SmtMatchBinder(convTerms[0], scopeCtx.Scope, tt, constructor, bindings));
                                        }
                                        else
                                        {
                                            _contextProvider.Context.TryGetFunctionDeclaration(new("or"), out SmtFunction? orf);
                                            var boolsort = _contextProvider.Context.GetSortDeclaration(new("Bool"));
                                            if (!orf!.TryResolveRank(out var rank, boolsort, Enumerable.Repeat(boolsort, convTerms.Count).ToArray()))
                                            {
                                                throw new InvalidOperationException("Too many terms to match pattern.");
                                            }
                                            binders.Add(new SmtMatchBinder(new SmtFunctionApplication(orf, rank, convTerms), scopeCtx.Scope, tt, constructor, bindings));
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

                        if (_contextProvider.Context.TryGetFunctionDeclaration(af.Id.Id, out SmtFunction? defn))
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

                            if (defn.TryResolveRank(out var rank, af.Id.Sort, args.Select(a => a.Sort).ToArray()))
                            {
                                to = new SmtFunctionApplication(defn, rank, args);
                            }
                            else
                            {
                                to = new ErrorTerm("Unable to resolve rank for: " + af.Id.Id.Symbol);
                                _logger.LogParseError("Unable to resolve rank for: " + af.Id.Id.Symbol, form.Position);
                            }
                        }
                        else
                        {
                            to = new ErrorTerm("Cannot find matching definition for: " + af.Id.Id);
                            _logger.LogParseError("Cannot find matching definition for: " + af.Id.Id, form.Position);
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
                    throw new NotImplementedException("Literal bit vectors not yet supported.");

                default:
                    to = default;
                    return false;
            }
        }

        // Note: child terms for binders are SemgusTokens, since we need to update the scope before parsing them.
        private record AnnotationForm([Exactly("!")] SmtIdentifier _, SmtTerm Child, [Rest] IList<SemgusToken> Attributes);
        private record LetForm([Exactly("let")] SmtIdentifier _, IList<(SmtIdentifier, SemgusToken)> Bindings, SemgusToken Child);
        private record ForallForm([Exactly("forall")] SmtIdentifier _, IList<(SmtIdentifier, SmtSort)> Bindings, SemgusToken Child);
        private record ExistsForm([Exactly("exists")] SmtIdentifier _, IList<(SmtIdentifier, SmtSort)> Bindings, SemgusToken Child);
        private record MatchPattern(SemgusToken Pattern, [Rest] IList<SemgusToken> Terms);
        private record MatchForm([Exactly("match")] SmtIdentifier _, SmtTerm TermToMatch, IList<MatchPattern> Patterns);
        private record QualifiedIdentifier([Exactly("as")] SymbolToken? _, SmtIdentifier Id, SmtSort? Sort)
        {
            public QualifiedIdentifier(SmtIdentifier Id) : this(null, Id, null) { }
        }
        private record ApplicationForm(QualifiedIdentifier Id, [Rest] IList<SemgusToken> Arguments)
        {
            public ApplicationForm(SmtIdentifier id, [Rest] IList<SemgusToken> arguments) : this(new QualifiedIdentifier(id), arguments) { }
        }
    }
}
