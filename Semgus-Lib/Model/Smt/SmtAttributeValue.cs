using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Terms;

namespace Semgus.Model.Smt
{
    public class SmtAttributeValue
    {
        public SmtAttributeValue()
        {
            Type = AttributeType.None;
        }

        public SmtAttributeValue(SmtLiteral literal)
        {
            LiteralValue = literal;
            Type = AttributeType.Literal;
        }

        public SmtAttributeValue(SmtIdentifier id)
        {
            IdentifierValue = id;
            Type = AttributeType.Identifier;
        }

        public SmtAttributeValue(SmtKeyword keyword)
        {
            KeywordValue = keyword;
            Type = AttributeType.Keyword;
        }

        public SmtAttributeValue(IEnumerable<SmtAttributeValue> values)
        {
            ListValue = values.ToList();
            Type = AttributeType.List;
        }

        // Four types: literals, identifiers, keywords, and lists of attribute values...plus a 'none' for a keyword attribute alone
        public enum AttributeType
        {
            None,
            Literal,
            Identifier,
            Keyword,
            List
        }

        public AttributeType Type { get; }

        public SmtLiteral? LiteralValue { get; }
        public SmtIdentifier? IdentifierValue { get; }
        public SmtKeyword? KeywordValue { get; }
        public IReadOnlyList<SmtAttributeValue>? ListValue { get; }

        public override string ToString()
        {
            switch (Type)
            {
                case AttributeType.Literal: return LiteralValue!.ToString() ?? "";
                case AttributeType.Identifier: return IdentifierValue!.ToString();
                case AttributeType.Keyword: return KeywordValue!.ToString();
                case AttributeType.List: return $"({string.Join(' ', ListValue!)})";
                default: throw new InvalidOperationException("Not a valid attribute type: " + Type);
            }
        }
    }
}
