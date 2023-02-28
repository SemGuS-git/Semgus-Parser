using System;
using System.Collections.Generic;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Sexpr.Writer;

namespace Semgus.Parser.Sexpr
{
    /// <summary>
    /// A symbol table for a CHC
    /// </summary>
    internal class ChcSymbolTable
    {
        /// <summary>
        /// The CHC this symbol table is for
        /// </summary>
        private readonly SemgusChc _chc;

        /// <summary>
        /// Creates a new symbol table
        /// </summary>
        /// <param name="chc">Underlying CHC</param>
        public ChcSymbolTable(SemgusChc chc)
        {
            _chc = chc;
        }

        /// <summary>
        /// Finds the index of an identifier in the CHC head's signature
        /// </summary>
        /// <param name="name">The identifier to find</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">Thrown if the identifier isn't found</exception>
        private int FindIndex(SmtIdentifier name)
        {
            for (int i = 0; i < _chc.Head.Arguments.Count; ++i)
            {
                if (_chc.Head.Arguments[i].Name == name)
                {
                    return i;
                }
            }
            throw new InvalidOperationException("Trying to find variable in CHC head that doesn't exist");
        }

        /// <summary>
        /// Writes a variable as a symbol entry
        /// </summary>
        /// <param name="sw">ISexprWriter to write to</param>
        /// <param name="v">Variable to write</param>
        private void WriteVariable(ISexprWriter sw, SmtVariable v)
        {
            SymbolEntry se = new(v.Name, v.Sort.Name, FindIndex(v.Name));
            se.Write(sw);
        }

        /// <summary>
        /// Writes a variable list, if not null
        /// </summary>
        /// <param name="sw">ISexprWriter to write to</param>
        /// <param name="name">Keyword to name this list</param>
        /// <param name="vars">Variables to write</param>
        private void MaybeWriteVariableList(ISexprWriter sw, string name, IEnumerable<SmtVariable>? vars)
        {
            if (vars is not null)
            {
                sw.WriteKeyword(name);
                sw.WriteList(vars, v => WriteVariable(sw, v));
            }
        }

        /// <summary>
        /// Writes a binding list
        /// </summary>
        /// <param name="sw">ISexprWriter to write to</param>
        /// <param name="name">Keyword to name this list</param>
        /// <param name="vars">Variable bindings to write</param>
        private void MaybeWriteBindingList(ISexprWriter sw, string name, IEnumerable<SmtVariableBinding> vars)
        {
            sw.WriteKeyword(name);
            sw.WriteList(vars, v =>
            {
                SymbolEntry se = new(v.Id, v.Sort.Name);
                se.Write(sw);
            });
        }

        /// <summary>
        /// Writes this symbol table
        /// </summary>
        /// <param name="sw">ISexprWriter to write to</param>
        public void Write(ISexprWriter sw)
        {
            sw.WriteList(() =>
            {
                sw.WriteSymbol("symbol-table");
                sw.WriteKeyword("term");
                WriteVariable(sw, _chc.TermVariable);
                MaybeWriteVariableList(sw, "input", _chc.InputVariables);
                MaybeWriteVariableList(sw, "output", _chc.OutputVariables);
                MaybeWriteBindingList(sw, "auxiliary", _chc.AuxiliaryVariables);
            });
        }

        /// <summary>
        /// A symbol entry for sorted symbols
        /// </summary>
        /// <param name="Id">Identifier of this symbol</param>
        /// <param name="Sort">Sort of this symbol</param>
        /// <param name="Index">Optional index of this symbol in the CHC head</param>
        private record SymbolEntry(SmtIdentifier Id, SmtSortIdentifier Sort, int? Index = default)
        {
            /// <summary>
            /// Writes this symbol entry
            /// </summary>
            /// <param name="sw">ISexprWriter to write to</param>
            public void Write(ISexprWriter sw)
            {
                sw.WriteList(() =>
                {
                    sw.WriteSymbol("symbol-entry");
                    sw.Write(Id);
                    sw.WriteKeyword("sort");
                    sw.Write(Sort);
                    if (Index is not null)
                    {
                        sw.WriteKeyword("index");
                        sw.WriteNumeral(Index.Value);
                    }
                });
            }
        }
    }
}
