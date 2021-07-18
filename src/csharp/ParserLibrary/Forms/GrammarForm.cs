using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A grammar declaration form. This consists of two parts: declarations and productions.
    /// Declarations consist of either declare-var or declare-nt forms.
    /// </summary>
    public record GrammarForm(IReadOnlyList<DeclareVarForm> VariableDeclarations,
                              IReadOnlyList<DeclareNonterminalForm> NonterminalDeclarations,
                              IReadOnlyList<ProductionForm> Productions)
    {
        /// <summary>
        /// Attempts to parse a grammar declaration
        /// </summary>
        /// <param name="cons">Grammar form to parse. Should not include any 'define-grammar' or other leading parameters</param>
        /// <param name="grammar">The parsed grammar form</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed successfully, false if not</returns>
        public static bool TryParse(ConsToken cons, out GrammarForm grammar, out string err, out SexprPosition errPos)
        {
            grammar = default;
            err = default;
            errPos = default;
            List<DeclareVarForm> varDecls = new();
            List<DeclareNonterminalForm> ntDecls = new();
            List<ProductionForm> productions = new();

            //
            // Grab declarations at the top of the grammar
            //
            while (cons is not null)
            {
                if (!cons.TryPop(out ConsToken subCons, out cons, out err, out errPos))
                {
                    return false;
                }
                
                //
                // Declarations start with a symbol, while noterminals are a list (<Name> <Var>)
                //
                if (subCons.Head is SymbolToken declSymbol)
                {
                    if (subCons.Tail is not ConsToken declTail)
                    {
                        err = "Expected a proper list for declaration, but got: " + subCons.Tail;
                        errPos = subCons.Tail.Position;
                        return false;
                    }

                    switch (declSymbol.Name)
                    {
                        case "declare-var":
                            if (!DeclareVarForm.TryParse(declTail, out var declVarForm, out err, out errPos))
                            {
                                return false;
                            }
                            varDecls.Add(declVarForm);
                            break;

                        case "declare-nt":
                            if (!DeclareNonterminalForm.TryParse(declTail, out var declNtForm, out err, out errPos))
                            {
                                return false;
                            }
                            ntDecls.Add(declNtForm);
                            break;

                        default:
                            err = "Unknown grammar declaration: " + declSymbol.Name;
                            errPos = declSymbol.Position;
                            return false;
                    }
                }
                else if (subCons.Head is ConsToken)
                {
                    if (!ProductionForm.TryParse(subCons, out var prodForm, out err, out errPos))
                    {
                        return false;
                    }
                    productions.Add(prodForm);
                }
                else
                {
                    err = "Unknown form in grammar declaration: " + subCons;
                    errPos = subCons.Position;
                    return false;
                }
            }

            grammar = new GrammarForm(varDecls, ntDecls, productions);
            return true;
        }
    }
}
