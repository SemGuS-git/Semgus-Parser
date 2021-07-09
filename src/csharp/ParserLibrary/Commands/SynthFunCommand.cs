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
    /// Command for defining a function to be synthesized, along with its grammar and syntax
    /// Syntax: (synth-fun [name] [inputs] [outputs] [productions]*)
    /// </summary>
    public class SynthFunCommand : ISemgusCommand
    {
        /// <summary>
        /// Name of this command, as appearing in the source files
        /// </summary>
        public string CommandName => "synth-fun";

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

            // Function name
            if (!form.TryPop(out SymbolToken name, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            // Input parameters
            if (!form.TryPop(out SemgusToken inputList, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            if (!VariableDeclarationForm.TryParseList(inputList, out var inputDecls, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
            }

            // Output parameters
            if (!form.TryPop(out SemgusToken outputList, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }
            if (!VariableDeclarationForm.TryParseList(outputList, out var outputDecls, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
            }

            List<ProductionForm> productions = new();
            while (default != form)
            {
                if (!form.TryPop(out ConsToken productionForm, out form, out err, out errPos))
                {
                    errorStream.WriteParseError(err, errPos);
                    errCount += 1;
                }
                if (!ProductionForm.TryParse(productionForm, out ProductionForm production, out err, out errPos))
                {
                    errorStream.WriteParseError(err, errPos);
                    errCount += 1;
                }
                productions.Add(production);
            }

            var env = LanguageEnvironmentCollector.ProcessSynthFun(inputDecls.Concat(outputDecls), productions, previous.GlobalEnvironment.Clone());

            var closure = new VariableClosure(parent: previous.GlobalClosure,
                                              FormsToDeclarations(inputDecls, env, VariableDeclaration.Context.SF_Input)
                                              .Concat(FormsToDeclarations(outputDecls, env, VariableDeclaration.Context.SF_Output)));

            SynthFun sf = new(name.Name, closure, productions.Select(p => ProcessProduction(p, env, closure)).ToList());

            return previous.AddSynthFun(sf).UpdateEnvironment(env);
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

            // TODO *wjc: relable aux variables as outputs when implied by their position in the relation
            var closure = new VariableClosure(parent: parentClosure,
                FormsToDeclarations(prod.VariableDeclarations, env, VariableDeclaration.Context.NT_Auxiliary)
                  .Prepend(new NonterminalTermDeclaration(
                      name: prod.Term.Name,
                      type: env.ResolveType(NonterminalTermDeclaration.TYPE_NAME),
                      nonterminal: env.ResolveNonterminal(nonterminal.Name),
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
                semanticRules: prod.Premises.Select(p => ProcessProductionRule(p, env, closure)).ToList()
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
                .Concat(FormsToDeclarations(rule.VariableDeclarations, env, VariableDeclaration.Context.PR_Auxiliary))
            );

            var node = new SemanticRule(
                rewriteExpression: choiceExpression,
                closure: closure,
                predicate: new FormulaConverter(env, closure).ConvertFormula(rule.Predicate)
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
        private IEnumerable<VariableDeclaration> FormsToDeclarations(IReadOnlyList<VariableDeclarationForm> decls,
                                                                       LanguageEnvironment env,
                                                                       VariableDeclaration.Context context)
        {
            return decls.Select(decl => new VariableDeclaration(decl.Name.Name, env.ResolveType(decl.Type.Name), context));
        }
    }
}
