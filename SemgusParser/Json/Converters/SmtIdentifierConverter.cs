using Newtonsoft.Json;

using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json.Converters
{
    internal class SmtIdentifierConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SmtIdentifier);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not SmtIdentifier id)
            {
                throw new InvalidOperationException("Attepted to serialize the wrong thing.");
            }

            if (id.Indices.Length > 0)
            {
                writer.WriteStartArray();
                writer.WriteValue(id.Symbol);
                foreach (var index in id.Indices)
                {
                    if (index.NumeralValue is not null)
                    {
                        writer.WriteValue(index.NumeralValue.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException("Indexes cannot be strings; they must be integers.");
                        // writer.WriteValue(index.StringValue);
                    }
                }
                writer.WriteEndArray();
            }
            else
            {
                serializer.Serialize(writer, id.Symbol);
            }
        }
    }
}
