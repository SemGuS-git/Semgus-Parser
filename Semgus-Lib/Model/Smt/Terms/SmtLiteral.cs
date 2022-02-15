using Semgus.Model.Smt.Theories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public abstract class SmtLiteral : SmtTerm
    {
        public SmtLiteral(SmtSort sort) : base(sort) { }
    }

    public class SmtNumeralLiteral : SmtLiteral
    {
        public long Value { get; }

        public SmtNumeralLiteral(SmtContext ctx, long value) : base(ctx.GetSortDeclaration(new SmtIdentifier("Int")))
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class SmtDecimalLiteral : SmtLiteral
    {
        public double Value { get; }

        public SmtDecimalLiteral(SmtContext ctx, double value) : base(ctx.GetSortDeclaration(new SmtIdentifier("Real")))
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class SmtStringLiteral : SmtLiteral
    {
        public string Value { get; }

        public SmtStringLiteral(SmtContext ctx, string value) : base(ctx.GetSortDeclaration(new SmtIdentifier("String")))
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
    // TODO: Bitvectors
}
