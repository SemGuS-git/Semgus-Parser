using Newtonsoft.Json;

using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json.Converters
{
    internal class SmtAttributeValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SmtAttributeValue);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not SmtAttributeValue attr)
            {
                throw new InvalidOperationException("Attempt to serialize not an attribute value.");
            }

            switch (attr.Type)
            {
                case SmtAttributeValue.AttributeType.None:
                    writer.WriteNull();
                    break;
                case SmtAttributeValue.AttributeType.Keyword:
                    serializer.Serialize(writer, attr.KeywordValue!.Name);
                    break;
                case SmtAttributeValue.AttributeType.Identifier:
                    serializer.Serialize(writer, attr.IdentifierValue);
                    break;
                case SmtAttributeValue.AttributeType.Literal:
                    serializer.Serialize(writer, attr.LiteralValue);
                    break;
                case SmtAttributeValue.AttributeType.List:
                    serializer.Serialize(writer, attr.ListValue);
                    break;
            }
        }
    }
}
