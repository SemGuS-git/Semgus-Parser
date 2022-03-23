
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public abstract class SmtLiteral : SmtTerm
    {
        public abstract object BoxedValue { get; }
        public SmtLiteral(SmtSort sort) : base(sort) { }
    }

    public class SmtNumeralLiteral : SmtLiteral
    {
        public long Value { get; }

        public override object BoxedValue => Value;

        public SmtNumeralLiteral(SmtContext ctx, long value) : base(ctx.GetSortDeclaration(SmtCommonIdentifiers.IntSortId))
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitNumeralLiteral(this);
        }
    }

    public class SmtDecimalLiteral : SmtLiteral
    {
        public double Value { get; }
        public override object BoxedValue => Value;
        public SmtDecimalLiteral(SmtContext ctx, double value) : base(ctx.GetSortDeclaration(SmtCommonIdentifiers.RealSortId))
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitDecimalLiteral(this);
        }
    }

    public class SmtStringLiteral : SmtLiteral
    {
        public string Value { get; }
        public override object BoxedValue => Value;
        public SmtStringLiteral(SmtContext ctx, string value) : base(ctx.GetSortDeclaration(SmtCommonIdentifiers.StringSortId))
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitStringLiteral(this);
        }
    }
    
    // TODO: Bitvectors
}
