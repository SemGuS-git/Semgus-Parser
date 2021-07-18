using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Semgus.Parser.Forms;
using Semgus.Parser.Reader;
using Semgus.Parser.Util;
using Semgus.Sexpr.Reader;
using Semgus.Syntax;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for defining a term to be synthesized, along with its type and grammar.
    /// If the grammar is a symbol, it will use a previously-defined grammar [NOT SUPPORTED YET]
    /// Otherwise, it can be a list containing an in-line grammar declaration.
    /// Syntax: (synth-term [name] [type] [grammar])
    /// </summary>
    public class SynthTermCommand : ISemgusCommand
    {
        /// <summary>
        /// Name of this command, as appearing in the source files
        /// </summary>
        public string CommandName => "synth-term";

        /// <summary>
        /// Tries to parse a synth fun command from the given form
        /// </summary>
        /// <param name="form">Form to parse</param>
        /// <param name="sfc">The resultant synth fun command</param>
        /// <param name="errorStream">Stream for writing errors to</param>
        /// <param name="errCount">Number of errors encountered while parsing</param>
        /// <returns>True if successfully parsed, false if not</returns>
        public SemgusProblem Process(SemgusProblem previous, ConsToken form, TextWriter errorStream, ref int errCount)
        {
            string err;
            SexprPosition errPos;
            // Arguments:
            // [0] = "synth-fun" (symbol)
            // [1] = function name (symbol)
            // [2] = input parameters (list)
            // [3] = output parameters (list)
            // &rest = non-terminals
            if (!form.TryPop(out SymbolToken _, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            // Objective name
            if (!form.TryPop(out SymbolToken name, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            // Objective type
            if (!form.TryPop(out SymbolToken type, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            if (!previous.GlobalEnvironment.TryResolveTermType(type.Name, out var objectiveType))
            {
                if (previous.GlobalEnvironment.IsNameDeclared(type.Name))
                {
                    errorStream.WriteParseError("Expected a term type, but got: " + type.Name, type.Position);
                }
                else
                {
                    errorStream.WriteParseError("Undeclared term type: " + type.Name, type.Position);
                }
                errCount += 1;
                return default;
            }

            VariableClosure nextClosure = new(parent: previous.GlobalClosure, new[]
            {
                new VariableDeclaration(name.Name, objectiveType, VariableDeclaration.Context.CT_Term)
            });

            // Grammar, possibly inline
            if (!form.TryPop(out SemgusToken grammar, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            if (grammar is SymbolToken)
            {
                errorStream.WriteParseError("Non-inline grammar definitions not supported", grammar.Position);
                errCount += 1;
                return default;
            }
            else if (grammar is ConsToken inlineGrammar)
            {
                if (!GrammarForm.TryParse(inlineGrammar, out var grammarForm, out err, out errPos))
                {
                    errorStream.WriteParseError(err, errPos);
                    errCount += 1;
                    return default;
                }

                var env = LanguageEnvironmentCollector.ProcessGrammar(grammarForm, previous.GlobalEnvironment.Clone());
                var closure = new VariableClosure(parent: nextClosure,
                                                FormsToDeclarations(grammarForm.VariableDeclarations, env, VariableDeclaration.Context.NT_Auxiliary));

                SynthFun sf = new(name.Name, closure, grammarForm.Productions.Select(p => ProcessProduction(p, env, closure)).ToList());
                return previous.AddSynthFun(sf).UpdateEnvironment(env).UpdateClosure(nextClosure);
            }
            else
            {
                errorStream.WriteParseError("Unknown expression. Expected grammar definition: " + grammar, grammar.Position);
                errCount += 1;
                return default;
            }
        }

        /// <summary>
        /// Processes a production form
        /// </summary>
        /// <param name="prod">The production form</param>
        /// <param name="env">The environment for this synth fun</param>
        /// <param name="parentClosure">The enclosing variable closure</param>
        /// <returns>The processed production group object</returns>
        private ProductionGroup ProcessProduction(ProductionForm prod, LanguageEnvironment env, VariableClosure parentClosure)
        {
            var nonterminal = env.ResolveNonterminal(prod.Name.Name);
            if (!parentClosure.TryResolve(prod.Term.Name, out var termVar))
            {
                throw new InvalidOperationException("Term variable not declared: " + prod.Term.Name);
            }
            if (termVar.Type != nonterminal.Type)
            {
                throw new InvalidOperationException($"Type of term variable {termVar} does not match nonterminal type: expected: {nonterminal.Type}, but got: {termVar.Type}.");
            }

            // TODO *wjc: relable aux variables as outputs when implied by their position in the relation
            var closure = new VariableClosure(parent: parentClosure,
                Enumerable.Empty<VariableDeclaration>()
                  .Prepend(new NonterminalTermDeclaration(
                      name: prod.Term.Name,
                      type: nonterminal.Type,
                      nonterminal: nonterminal,
                      declarationContext: VariableDeclaration.Context.NT_Term
                      ))
            );

            if (prod.Relation.List is null)
            {
                throw new InvalidOperationException("Semantic relation must be a list: for nonterminal " + nonterminal.Name);
            }

            var relationSymbols = prod.Relation.List.Select(f =>
            {
                if (f.Atom is null)
                {
                    throw new InvalidOperationException("Only atoms allowed in semantic relation: " + f.ToString());
                }
                else if (f.Atom is not SymbolToken symb)
                {
                    throw new InvalidOperationException("Only symbols allowed in semantic relation: " + f.Atom.ToString());
                }
                else
                {
                    return symb;
                }
            });

            relationSymbols = relationSymbols.Pop(out var relName);

            var relationInstance = new SemanticRelationInstance(env.ResolveRelation(relName.Name),
                                                                relationSymbols.Select(s => {
                                                                    if (closure.TryResolve(s.Name, out var decl))
                                                                    {
                                                                        return decl;
                                                                    }
                                                                    else
                                                                    {
                                                                        throw new InvalidOperationException("Unknown variable in semantic relation: " + s.Name);
                                                                    }
                                                                }).ToList());

            var node = new ProductionGroup(
                nonterminal: nonterminal,
                closure: closure,
                relationInstance: relationInstance,
                semanticRules: prod.Productions.Select(p => ProcessProductionRule(p, env, closure)).ToList()
            );

            node.AssertCorrectness();

            return node;
        }

        /// <summary>
        /// Processes an individual production rule, a.k.a. right-hand-side of a production
        /// </summary>
        /// <param name="rule">The production rule form</param>
        /// <param name="env">The current language environment</param>
        /// <param name="parentClosure">The enclosing variable closure</param>
        /// <returns>The processed semantic rule object</returns>
        private SemanticRule ProcessProductionRule(ProductionRuleForm rule, LanguageEnvironment env, VariableClosure parentClosure)
        {
            var choiceExpressionConverter = new ChoiceExpressionConverter(env);
            var choiceExpression = choiceExpressionConverter.ProcessChoiceExpression(rule);

            var closure = new VariableClosure(
                parent: parentClosure,
                variables:
                choiceExpressionConverter.DeclaredTerms
            );

            FormulaConverter fConv = new(env, closure);

            var node = new SemanticRule(
                rewriteExpression: choiceExpression,
                closure: closure,
                predicates: rule.Predicates.Select(p => fConv.ConvertFormula(p)).ToList()
            );
            return node;
        }

        /// <summary>
        /// Converts a list of variable declaration forms into a list of variable declaration objects
        /// </summary>
        /// <param name="decls">The list of declaration forms</param>
        /// <param name="env">The current environment</param>
        /// <param name="context">The usage context for the variables</param>
        /// <returns>The processed declarations</returns>
        private IEnumerable<VariableDeclaration> FormsToDeclarations(IReadOnlyList<DeclareVarForm> decls,
                                                                       LanguageEnvironment env,
                                                                       VariableDeclaration.Context context)
        {
            return decls.SelectMany(decl => decl.Symbols.Select(d => new VariableDeclaration(d.Name, env.ResolveType(decl.Type.Name), context)));
        }
    }
}
