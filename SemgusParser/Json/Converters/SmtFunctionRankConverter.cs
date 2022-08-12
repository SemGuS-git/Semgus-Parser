using Newtonsoft.Json;

using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json.Converters
{
    internal class SmtFunctionRankConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SmtFunctionRank);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not SmtFunctionRank rank)
            {
                throw new InvalidOperationException("Attepted to serialize the wrong thing.");
            }

            writer.WriteStartObject();
            writer.WritePropertyName("argumentSorts");
            serializer.Serialize(writer, rank.ArgumentSorts.Select(s => s.Name));
            writer.WritePropertyName("returnSort");
            serializer.Serialize(writer, rank.ReturnSort.Name);
            writer.WriteEndObject();
        }
    }
}
