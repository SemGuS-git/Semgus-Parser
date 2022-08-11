using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Sexpr
{
    internal static class SexprWriterExtensions
    {
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
                writer.Write(binder.Constructor.Operator);
                writer.WriteKeyword("arguments");
                writer.WriteList(binder.Bindings.OrderBy(b => b.Index), b => writer.Write(b.Binding.Id));
                writer.WriteKeyword("argument-sorts");
                writer.WriteList(binder.Constructor.Children, c => writer.Write(c.Name));
                writer.WriteKeyword("return-sort");
                writer.Write(binder.ParentType.Name);
            });
        }

        public static void Write(this ISexprWriter writer, SmtTerm term)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol("term");
                term.Accept(new SmtTermWriter(writer));
            });
        }

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
    }
}
