using Newtonsoft.Json;

using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json.Converters
{
    internal class SmtTermConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableTo(typeof(SmtTerm));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            SmtTermConverterVisitor visitor = new(writer, serializer);
            if (value is not SmtTerm term)
            {
                throw new InvalidOperationException("Attempt to serialize not an SmtTerm.");
            }
            term.Accept(visitor);
        }

        private class SmtTermConverterVisitor : ISmtTermVisitor<object>
        {
            private readonly JsonWriter _writer;
            private readonly JsonSerializer _serializer;
            public SmtTermConverterVisitor(JsonWriter writer, JsonSerializer serializer)
            {
                _writer = writer;
                _serializer = serializer;
            }

            private abstract record TermModel
            {
                [JsonProperty("$termType")]
                string TermType { get; }

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
                                                    SmtIdentifier ReturnSort,
                                                    IEnumerable<SmtIdentifier> ArgumentSorts,
                                                    IEnumerable<SmtTerm> Arguments) : TermModel("application");
            public object VisitFunctionApplication(SmtFunctionApplication functionApplication)
            {
                _serializer.Serialize(_writer, new FunctionApplicationModel(
                    Name: functionApplication.Definition.Name,
                    ReturnSort: functionApplication.Rank.ReturnSort.Name,
                    ArgumentSorts: functionApplication.Rank.ArgumentSorts.Select(x => x.Name),
                    Arguments: functionApplication.Arguments
                ));
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

            private record VariableModel(SmtIdentifier Name, SmtIdentifier Sort) : TermModel("variable");
            public object VisitVariable(SmtVariable variable)
            {
                _serializer.Serialize(_writer, new VariableModel(Name: variable.Name, Sort: variable.Sort.Name));
                return this;
            }

            private record ExistsBinderModel(IEnumerable<VariableModel> Bindings, SmtTerm Child) : TermModel("exists");
            public object VisitExistsBinder(SmtExistsBinder existsBinder)
            {
                _serializer.Serialize(_writer,
                    new ExistsBinderModel(existsBinder.NewScope.LocalBindings.Select(b => new VariableModel(b.Id, b.Sort.Name)),
                                          existsBinder.Child));
                return this;
            }

            private record ForallBinderModel(IEnumerable<VariableModel> Bindings, SmtTerm Child) : TermModel("forall");
            public object VisitForallBinder(SmtForallBinder forallBinder)
            {
                _serializer.Serialize(_writer,
                    new ForallBinderModel(forallBinder.NewScope.LocalBindings.Select(b => new VariableModel(b.Id, b.Sort.Name)),
                                          forallBinder.Child));
                return this;
            }
        }
    }
}
