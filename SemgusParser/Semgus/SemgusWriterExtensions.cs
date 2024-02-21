using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Parser.Sexpr;
using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Semgus
{
    internal static class SemgusWriterExtensions
    {
        public static void WriteList<T>(this ISexprWriter writer, IEnumerable<T> list, Action<T> action, bool insertNewlines = false)
        {
            writer.WriteList(() =>
            {
                bool first = true;
                foreach (var item in list)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteSpace();
                        if (insertNewlines)
                        {
                            writer.AddConditionalNewline();
                        }
                    }
                    action(item);
                }
            });
        }

        public static void WriteForm(this ISexprWriter writer, string formName, Action? body = null)
        {
            writer.WriteList(() =>
            {
                writer.WriteSymbol(formName);
                if (body is not null)
                {
                    writer.WriteSpace();
                    body();
                }
            });
        }

        public static void Write<T>(this ISexprWriter writer, IEnumerable<T> elements, Action<T> action)
        {
            writer.WithinLogicalBlock("", "", false, () =>
            {
                bool first = true;
                foreach (var item in elements)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteSpace();
                        writer.AddConditionalNewline();
                    }
                    action(item);
                }
            });
        }

        public static void Write(this ISexprWriter writer, SmtSortIdentifier sortId)
        {
            if (sortId.Parameters.Length > 0)
            {
                writer.WriteList(() =>
                {
                    writer.Write(sortId.Name);
                    foreach (var sParam in sortId.Parameters)
                    {
                        writer.WriteSpace();
                        writer.Write(sParam);
                    }
                });
            }
            else
            {
                writer.Write(sortId.Name);
            }
        }

        public static void Write(this ISexprWriter writer, SmtIdentifier id)
        {
            if (id.Indices.Length > 0)
            {
                writer.WriteList(() =>
                {
                    writer.WriteSymbol("_");
                    writer.WriteSpace();
                    writer.WriteSymbol(id.Symbol);
                    foreach (var index in id.Indices)
                    {
                        writer.WriteSpace();
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
            else
            {
                writer.WriteSymbol(id.Symbol);
            }
        }

        public static void Write(this ISexprWriter writer, SemgusChc.SemanticRelation rel)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

        public static void Write(this ISexprWriter writer, SmtTerm term)
        {
            term.Accept(new SemgusTermWriter(writer));
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

        public static void Write(this ISexprWriter writer, SmtKeyword keyword)
        {
            writer.WriteKeyword(keyword.Name);
        }

        public static void Write(this ISexprWriter writer, SmtAttributeValue attrVal)
        {
            switch (attrVal.Type)
            {
                case SmtAttributeValue.AttributeType.List:
                    writer.WriteList(attrVal.ListValue!, writer.Write);
                    break;

                case SmtAttributeValue.AttributeType.Identifier:
                    writer.Write(attrVal.IdentifierValue!);
                    break;

                case SmtAttributeValue.AttributeType.Keyword:
                    writer.Write(attrVal.KeywordValue!);
                    break;

                case SmtAttributeValue.AttributeType.Literal:
                    writer.Write(attrVal.LiteralValue!);
                    break;

                case SmtAttributeValue.AttributeType.None:
                    break;
            }
        }

        public static void Write(this ISexprWriter writer, int integer)
        {
            if (integer >= 0)
            {
                writer.WriteNumeral(integer);
            }
            else
            {
                writer.WriteList(() =>
                {
                    writer.WriteSymbol("-");
                    writer.WriteNumeral(-integer);
                });
            }
        }

        public static void WriteSpace(this ISexprWriter writer)
        {
            writer.WriteSymbol(" ");
        }
    }
}
