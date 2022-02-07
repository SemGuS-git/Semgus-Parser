using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        public TermConverter(DestructuringHelper helper, SmtConverter converter, ISmtScopeProvider scopeProvider, ISmtContextProvider contextProvider)
        {
            _destructuringHelper = helper;
            _converter = converter;
            _scopeProvider = scopeProvider;
            _contextProvider = contextProvider;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return to == typeof(SmtTerm); // Converting to specific term types not supported...I think
        }

        // Note on return value: we only return false if we cannot convert to a term, structurally.
        // If it looks like we should be able to convert and can't, then we return true with to as an ErrorTerm.
        // A.k.a.: False means to try another converter. True + ErrorTerm means we know that this is the right converter, but the user messed up.
        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
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
                    }
                }
                else
                {
                    to = new ErrorTerm("Unable to resolve function or variable: " + qid.Id);
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
                                    //af.Child.Annotations.Add(...);
                                    to = af.Child;
                                    return true;
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
                                        scopeCx.Scope.AddVariableBinding(id, sort, SmtScope.BindingType.Existential);
                                    }
                                    if (_converter.TryConvert(ef.Child, out SmtTerm? child))
                                    {
                                        to = new SmtExistsBinder(child, scopeCx.Scope);
                                        return true;
                                    }
                                }
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
                                        scopeCx.Scope.AddVariableBinding(id, sort, SmtScope.BindingType.Universal);
                                    }
                                    if (_converter.TryConvert(ff.Child, out SmtTerm? child))
                                    {
                                        to = new SmtExistsBinder(child, scopeCx.Scope);
                                        return true;
                                    }
                                }
                                to = new ErrorTerm("Invalid forall form.");
                                return true;
                            }

                        case "match":
                            throw new InvalidOperationException("Match not yet supported.");

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
                                    return new ErrorTerm("Unable to construct term: " + a);
                                }
                            }).ToList();

                            if (defn.TryResolveRank(out var rank, af.Id.Sort, args.Select(a => a.Sort).ToArray()))
                            {
                                to = new SmtFunctionApplication(defn, rank, args);
                            }
                            else
                            {
                                to = new ErrorTerm("Unable to resolve rank for: " + qid);
                            }
                        }
                        else
                        {
                            to = new ErrorTerm("Cannot find matching definition for: " + af.Id.Id);
                        }
                    }
                    else
                    {
                        to = new ErrorTerm("Expected a function identifier, but got: " + form.First());
                    }
                }
                return true;
            }
            else
            // Finally, literals.
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
        }

        // Note: child terms for binders are SemgusTokens, since we need to update the scope before parsing them.
        private record AnnotationForm([Exactly("!")] SmtIdentifier _, SmtTerm Child, [Rest] IList<SemgusToken> Attributes);
        private record LetForm([Exactly("let")] SmtIdentifier _, IList<(SmtIdentifier, SemgusToken)> Bindings, SemgusToken Child);
        private record ForallForm([Exactly("forall")] SmtIdentifier _, IList<(SmtIdentifier, SmtSort)> Bindings, SemgusToken Child);
        private record ExistsForm([Exactly("exists")] SmtIdentifier _, IList<(SmtIdentifier, SmtSort)> Bindings, SemgusToken Child);
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
