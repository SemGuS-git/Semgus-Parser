using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Parser.Sexpr
{
    /// <summary>
    /// Extensions for writing various types as S-expressions
    /// </summary>
    internal static class SexprWriterExtensions
    {
        /// <summary>
        /// Writes an IEnumerable as a list by calling an action.
        /// (list [`action-result`...])
        /// </summary>
        /// <typeparam name="T">Type to enumerate over</typeparam>
        /// <param name="writer">Writer to write to</param>
        /// <param name="list">List to enumerate over</param>
        /// <param name="action">Action to call on each element</param>
        public static void WriteList<T>(this ISexprWriter writer, IEnumerable<T> list, Action<T> action)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("list");
                
                foreach (var item in list)
                {
                    action(item);
                }
            });
        }

        /// <summary>
        /// Writes an SMT sort identifier
        /// (sort `name` [`parameters`...])
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="sortId">Sort ID to write</param>
        public static void Write(this ISexprWriter writer, SmtSortIdentifier sortId)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("sort");
                writer.Write(sortId.Name);
                foreach (var sParam in sortId.Parameters)
                {
                    writer.Write(sParam);
                }
            });
        }

        /// <summary>
        /// Writes an SMT identifier
        /// (identifier `symbol` [`indices`...])
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="id">Identifier to write</param>
        public static void Write(this ISexprWriter writer, SmtIdentifier id)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("identifier");
                writer.WriteString(id.Symbol);
                foreach (var index in id.Indices)
                {
                    if (index.NumeralValue.HasValue)
                    {
                        writer.WriteNumeral(index.NumeralValue.Value);
                    }
                    else
                    {
                        writer.WriteString(index.StringValue);
                    }
                }
            });
        }

        /// <summary>
        /// Writes a semantic relation
        /// (relation `name` :signature `signature` :arguments `arguments`)
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="rel"></param>
        public static void Write(this ISexprWriter writer, SemgusChc.SemanticRelation rel)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("relation");
                writer.Write(rel.Relation.Name);
                writer.WriteKeyword("signature");
                writer.WriteList(rel.Rank.ArgumentSorts, a => writer.Write(a.Name));
                writer.WriteKeyword("arguments");
                writer.WriteList(rel.Arguments, a => writer.Write(a.Name));
            });
        }

        /// <summary>
        /// Writes a match binder
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="binder">Binder to write</param>
        public static void WriteConstructor(this ISexprWriter writer, SmtMatchBinder binder)
        {
            if (binder.Constructor is null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteList(() =>
            {
                writer.WriteSymbol("constructor");
                writer.Write(binder.Constructor.Name);
                writer.WriteKeyword("arguments");
                writer.WriteList(binder.Bindings.OrderBy(b => b.Index), b => writer.Write(b.Binding.Id));
                writer.WriteKeyword("argument-sorts");
                writer.WriteList(binder.Constructor.Children, c => writer.Write(c.Name));
                writer.WriteKeyword("return-sort");
                writer.Write(binder.ParentType.Name);
            });
        }

        /// <summary>
        /// Writes an SMT term
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="term">Term to write</param>
        public static void Write(this ISexprWriter writer, SmtTerm term)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("term");
                term.Accept(new SmtTermWriter(writer));
            });
        }

        /// <summary>
        /// Writes a synth-fun statement
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="ssf">Synthfun to write</param>
        public static void Write(this ISexprWriter writer, SemgusSynthFun ssf)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("synth-fun");
                writer.Write(ssf.Relation.Name);
                writer.WriteKeyword("term-type");
                writer.Write(ssf.Rank.ReturnSort.Name);
                writer.WriteKeyword("grammar");
                writer.Write(ssf.Grammar);
            });
        }

        /// <summary>
        /// Writes a grammar
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="grammar">Grammar to write</param>
        public static void Write(this ISexprWriter writer, SemgusGrammar grammar)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("grammar");
                writer.WriteKeyword("non-terminals");
                writer.WriteList(grammar.NonTerminals, nt => writer.Write(nt.Name));
                writer.WriteKeyword("non-terminal-types");
                writer.WriteList(grammar.NonTerminals, nt => writer.Write(nt.Sort.Name));
                writer.WriteKeyword("productions");
                writer.WriteList(grammar.Productions, p =>
                {
                    writer.WriteList(() =>
                    {
                        writer.WriteSymbol("production");
                        writer.WriteKeyword("instance");
                        writer.Write(p.Instance.Name);
                        writer.WriteKeyword("occurrences");
                        writer.WriteList(p.Occurrences, o =>
                        {
                            if (o is null)
                            {
                                writer.WriteNil();
                            }
                            else
                            {
                                writer.Write(o.Name);
                            }
                        });
                        writer.WriteKeyword("operator");
                        if (p.Constructor is null)
                        {
                            writer.WriteNil();
                        }
                        else
                        {
                            writer.Write(p.Constructor.Operator);
                        }
                    });
                });
            });
        }

        /// <summary>
        /// Writes a function rank
        /// </summary>
        /// <param name="writer">Writer to write to</param>
        /// <param name="rank">Rank to write</param>
        public static void Write(this ISexprWriter writer, SmtFunctionRank rank)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("rank");
                writer.WriteKeyword("argument-sorts");
                writer.WriteList(rank.ArgumentSorts, r => writer.Write(r.Name));
                writer.WriteKeyword("return-sort");
                writer.Write(rank.ReturnSort.Name);
            });
        }

        /// <summary>
        /// Writes a symbol table entry
        /// </summary>
        /// <param name="sw">ISexprWriter to write to</param>
        /// <param name="se">Symbol entry to write</param>
        public static void Write(this ISexprWriter sw, SemgusChc.SymbolEntry se)
        {
            sw.WriteList(() =>
            {
                sw.WriteSymbol("symbol-entry");
                sw.Write(se.Id);
                sw.WriteKeyword("sort");
                sw.Write(se.Sort);
                if (se.Index is not null)
                {
                    sw.WriteKeyword("index");
                    sw.WriteNumeral(se.Index.Value);
                }
            });
        }

        /// <summary>
        /// Writes a symbol table
        /// </summary>
        /// <param name="sw">ISexprWriter to write to</param>
        /// <param name="st">Symbol table to write</param>
        public static void Write(this ISexprWriter sw, SemgusChc.SymbolTable st)
        {
            sw.WriteList(() =>
            {
                sw.WriteSymbol("symbol-table");
                sw.WriteKeyword("term");
                sw.Write(st.Term);
                if (st.Inputs is not null)
                {
                    sw.WriteKeyword("inputs");
                    sw.WriteList(st.Inputs, se => sw.Write(se));
                }
                if (st.Outputs is not null)
                {
                    sw.WriteKeyword("outputs");
                    sw.WriteList(st.Outputs, se => sw.Write(se));
                }
                sw.WriteKeyword("auxiliary");
                sw.WriteList(st.Auxiliary, se => sw.Write(se));
                sw.WriteKeyword("children");
                sw.WriteList(st.Children, se => sw.Write(se));
                sw.WriteKeyword("unclassified");
                sw.WriteList(st.Unclassified, se => sw.Write(se));
            });
        }
    }
}
