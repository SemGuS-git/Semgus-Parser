using Newtonsoft.Json;

using Semgus.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json.Converters
{
    internal class SemanticRelationConverter : JsonConverter
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(SemgusChc.SemanticRelation);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not SemgusChc.SemanticRelation rel)
            {
                throw new InvalidOperationException("Attepted to serialize the wrong thing.");
            }

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            serializer.Serialize(writer, rel.Relation.Name);
            writer.WritePropertyName("signature");
            serializer.Serialize(writer, rel.Rank.ArgumentSorts.Select(s => s.Name));
            writer.WritePropertyName("arguments");
            serializer.Serialize(writer, rel.Arguments.Select(s => s.Name));
            writer.WriteEndObject();
        }
    }
}
