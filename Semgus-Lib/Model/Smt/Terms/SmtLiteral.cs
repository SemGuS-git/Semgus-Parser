﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    /// <summary>
    /// Top-level class of literal SMT terms
    /// </summary>
    public abstract class SmtLiteral : SmtTerm
    {
        /// <summary>
        /// Loosely-typed accessor for this literal's value
        /// </summary>
        /// <value></value>
        public abstract object BoxedValue { get; }
        
        /// <summary>
        /// Resolves the given sort for a literal
        /// </summary>
        /// <param name="ctx">SmtContext</param>
        /// <param name="name">Literal sort identifier</param>
        /// <returns>The resolved sort</returns>
        /// <exception cref="InvalidOperationException">Thrown when the literal sort cannot be found</exception>
        protected static SmtSort GetSortOrDie(SmtContext ctx, SmtSortIdentifier name)
        {
            if (!ctx.TryGetSortDeclaration(name, out var sort, out var error))
            {
                throw new InvalidOperationException("Failed to get sort for literal: " + name + " with error: " + error);
            }
            return sort;
        }

        /// <summary>
        /// Base constructor for literals of the given sort 
        /// </summary>
        /// <param name="sort">Literal sort</param>
        public SmtLiteral(SmtSort sort) : base(sort) { }
    }

    /// <summary>
    /// Literals for numerals (integers)
    /// </summary>
    public class SmtNumeralLiteral : SmtLiteral
    {
        /// <summary>
        /// The literal value
        /// </summary>
        public long Value { get; }
    
        public override object BoxedValue => Value;

        /// <summary>
        /// Constructs a new numeral literal with the given value
        /// </summary>
        /// <param name="ctx">Current SMT context</param>
        /// <param name="value">Literal value</param>
        public SmtNumeralLiteral(SmtContext ctx, long value) : base(GetSortOrDie(ctx, SmtCommonIdentifiers.IntSortId))
        {
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Accepts a visitor for this term
        /// </summary>
        /// <typeparam name="TOutput">Visitation output type</typeparam>
        /// <param name="visitor">The visitor</param>
        /// <returns>Output of the visitor</returns>
        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitNumeralLiteral(this);
        }
    }

    /// <summary>
    /// Literals for decimals (reals)
    /// </summary>
    public class SmtDecimalLiteral : SmtLiteral
    {
        public override object BoxedValue => Value;
        
        /// <summary>
        /// The literal value
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Constructs a new decimal literal with the given value
        /// </summary>
        /// <param name="ctx">Current SMT context</param>
        /// <param name="value">Literal value</param>
        public SmtDecimalLiteral(SmtContext ctx, double value) : base(GetSortOrDie(ctx, SmtCommonIdentifiers.RealSortId))
        {
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Accepts a visitor for this term
        /// </summary>
        /// <typeparam name="TOutput">Visitation output type</typeparam>
        /// <param name="visitor">The visitor</param>
        /// <returns>Output of the visitor</returns>
        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitDecimalLiteral(this);
        }
    }

    /// <summary>
    /// Literals for strings
    /// </summary>
    public class SmtStringLiteral : SmtLiteral
    {
        public override object BoxedValue => Value;
        
        /// <summary>
        /// The literal value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Constructs a new string literal with the given value
        /// </summary>
        /// <param name="ctx">Current SMT context</param>
        /// <param name="value">Literal value</param>
        public SmtStringLiteral(SmtContext ctx, string value) : base(GetSortOrDie(ctx, SmtCommonIdentifiers.StringSortId))
        {
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"\"{Value}\"";
        }

        /// <summary>
        /// Accepts a visitor for this term
        /// </summary>
        /// <typeparam name="TOutput">Visitation output type</typeparam>
        /// <param name="visitor">The visitor</param>
        /// <returns>Output of the visitor</returns>
        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitStringLiteral(this);
        }
    }

    /// <summary>
    /// Literals for bit vectors
    /// </summary>
    public class SmtBitVectorLiteral : SmtLiteral
    {
        public override object BoxedValue => Value;

        /// <summary>
        /// The literal value
        /// </summary>
        public BitArray Value { get; }

        /// <summary>
        /// Constructs a new string literal with the given value
        /// </summary>
        /// <param name="ctx">Current SMT context</param>
        /// <param name="value">Literal value</param>
        public SmtBitVectorLiteral(SmtContext ctx, BitArray value) : base(GetSortOrDie(ctx, SmtCommonIdentifiers.BitVectorSortId[value.Length]))
        {
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"#b{string.Join("", Value.Cast<bool>().Select(b => b?"1":"0"))}";
        }

        /// <summary>
        /// Accepts a visitor for this term
        /// </summary>
        /// <typeparam name="TOutput">Visitation output type</typeparam>
        /// <param name="visitor">The visitor</param>
        /// <returns>Output of the visitor</returns>
        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitBitVectorLiteral(this);
        }
    }
}
