using Newtonsoft.Json;

using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json.Converters
{
    internal class SmtSortIdentifierConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SmtSortIdentifier);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not SmtSortIdentifier id)
            {
                throw new InvalidOperationException("Attepted to serialize the wrong thing.");
            }

            if (id.Parameters.Length > 0)
            {
                throw new InvalidOperationException("Parameterized sorts not yet supported by the JSON serializer.");
            }
            else
            {
                serializer.Serialize(writer, id.Name);
            }            
        }
    }
}
