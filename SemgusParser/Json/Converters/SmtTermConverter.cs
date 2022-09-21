using Newtonsoft.Json;

using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Parser.Json.Converters
{
    /// <summary>
    /// Converts SMT terms into a JSON representation
    /// </summary>
    internal class SmtTermConverter : JsonConverter
    {
        /// <summary>
        /// Can the given object type be converted?
        /// </summary>
        /// <param name="objectType">Type to check</param>
        /// <returns>True if an SmtTerm subclass</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableTo(typeof(SmtTerm));
        }

        /// <summary>
        /// JSON reading is not implemented for this converter
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown</exception>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the given SmtTerm into JSON
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="value">SmtTerm object</param>
        /// <param name="serializer">Serializer to use</param>
        /// <exception cref="InvalidOperationException">Thrown if given something other than an SmtTerm object</exception>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            SmtTermConverterVisitor visitor = new(writer, serializer);
            if (value is not SmtTerm term)
            {
                throw new InvalidOperationException("Attempt to serialize not an SmtTerm.");
            }
            term.Accept(visitor);
        }

        /// <summary>
        /// Specifies conversion methods for each term type
        /// </summary>
        private class SmtTermConverterVisitor : ISmtTermVisitor<object>
        {
            /// <summary>
            /// JSON writer to use
            /// </summary>
            private readonly JsonWriter _writer;

            /// <summary>
            /// JSON serializer to use
            /// </summary>
            private readonly JsonSerializer _serializer;

            /// <summary>
            /// Creates a new SmtTermConverterVisitor for the given writer and serializer
            /// </summary>
            /// <param name="writer">JSON writer to use</param>
            /// <param name="serializer">JSON serializer to use</param>
            public SmtTermConverterVisitor(JsonWriter writer, JsonSerializer serializer)
            {
                _writer = writer;
                _serializer = serializer;
            }

            /// <summary>
            /// Base term model. Specifies the term type and annotation properties.
            /// </summary>
            private abstract record TermModel
            {
                [JsonProperty("$termType")]
                public string TermType { get; }

                [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
                public IEnumerable<SmtAttribute>? Annotations { get; init; }

                public TermModel(string type)
                {
                    TermType = type;
                }
            }

            public object VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral)
            {
                _serializer.Serialize(_writer, decimalLiteral.Value);
                return this;
            }

            private record FunctionApplicationModel(SmtIdentifier Name,
                                                    SmtSortIdentifier ReturnSort,
                                                    IEnumerable<SmtSortIdentifier> ArgumentSorts,
                                                    IEnumerable<SmtTerm> Arguments) : TermModel("application");
            public object VisitFunctionApplication(SmtFunctionApplication functionApplication)
            {
                _serializer.Serialize(_writer, new FunctionApplicationModel(
                    Name: functionApplication.Definition.Name,
                    ReturnSort: functionApplication.Rank.ReturnSort.Name,
                    ArgumentSorts: functionApplication.Rank.ArgumentSorts.Select(x => x.Name),
                    Arguments: functionApplication.Arguments
                )
                {
                    Annotations = functionApplication.Annotations
                });
                return this;
            }

            public object VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral)
            {
                _serializer.Serialize(_writer, numeralLiteral.Value);
                return this;
            }

            public object VisitStringLiteral(SmtStringLiteral stringLiteral)
            {
                _serializer.Serialize(_writer, stringLiteral.Value);
                return this;
            }

            private record VariableModel(SmtIdentifier Name, SmtSortIdentifier Sort) : TermModel("variable");
            public object VisitVariable(SmtVariable variable)
            {
                _serializer.Serialize(_writer,
                    new VariableModel(Name: variable.Name, Sort: variable.Sort.Name)
                    {
                        Annotations = variable.Annotations
                    });
                return this;
            }

            private record ExistsBinderModel(IEnumerable<VariableModel> Bindings, SmtTerm Child) : TermModel("exists");
            public object VisitExistsBinder(SmtExistsBinder existsBinder)
            {
                _serializer.Serialize(_writer,
                    new ExistsBinderModel(existsBinder.NewScope.LocalBindings.Select(b => new VariableModel(b.Id, b.Sort.Name)),
                                          existsBinder.Child)
                    {
                        Annotations = existsBinder.Annotations
                    });
                return this;
            }

            private record ForallBinderModel(IEnumerable<VariableModel> Bindings, SmtTerm Child) : TermModel("forall");
            public object VisitForallBinder(SmtForallBinder forallBinder)
            {
                _serializer.Serialize(_writer,
                    new ForallBinderModel(forallBinder.NewScope.LocalBindings.Select(b => new VariableModel(b.Id, b.Sort.Name)),
                                          forallBinder.Child)
                    {
                        Annotations = forallBinder.Annotations
                    });
                return this;
            }

            private record MatchGrouperModel(SmtTerm Term, IEnumerable<SmtMatchBinder> Binders) : TermModel("match");
            public object VisitMatchGrouper(SmtMatchGrouper matchGrouper)
            {
                _serializer.Serialize(_writer,
                    new MatchGrouperModel(matchGrouper.Term, matchGrouper.Binders)
                    {
                        Annotations = matchGrouper.Annotations
                    });
                return this;
            }

            private record MatchBinderModel(SmtIdentifier? Operator, IEnumerable<SmtIdentifier> Arguments, SmtTerm Child) : TermModel("binder");
            public object VisitMatchBinder(SmtMatchBinder matchBinder)
            {
                _serializer.Serialize(_writer,
                    new MatchBinderModel(matchBinder.Constructor?.Name, matchBinder.Bindings.Select(b => b.Binding.Id), matchBinder.Child)
                    {
                        Annotations = matchBinder.Annotations
                    });
                return this;
            }

            private record LambdaBinderModel(IEnumerable<SmtIdentifier> Arguments, SmtTerm Body) : TermModel("lambda");
            public object VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
            {
                _serializer.Serialize(_writer,
                    new LambdaBinderModel(lambdaBinder.ArgumentNames, lambdaBinder.Child)
                    {
                        Annotations = lambdaBinder.Annotations
                    });
                return this;
            }

            public object VisitLetBinder(SmtLetBinder letBinder)
            {
                throw new NotImplementedException();
            }

            private record BitVectorModel(int Size, string Value) : TermModel("bitvector");
            public object VisitBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral)
            {
                var bv = bitVectorLiteral.Value;
                byte[] bytes = new byte[(int)Math.Ceiling(bv.Length / 8.0)];
                bv.CopyTo(bytes, 0);

                string data = "0x" + string.Join("", bytes.Select(b => $"{b:X}").Reverse());

                _serializer.Serialize(_writer, new BitVectorModel(bv.Length, data));
                return this;
            }
        }
    }
}
