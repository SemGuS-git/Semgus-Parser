using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands
{
    internal class GrammarBlockHelper
    {
        public static SynthFunCommand.GrammarForm? CreateGrammarForm<T>(SemgusToken? grammarPredecl, SemgusToken? grammarBlock, ISmtConverter converter, ISourceMap sourceMap, ILogger<T> logger)
        {
            if ((grammarPredecl is null) != (grammarBlock is null))
            {
                // This reports an unknown position for some reason...
                throw logger.LogParseErrorAndThrow("Grammar must consist of a predeclaration list and a production list, but only got one of these.", sourceMap[(grammarPredecl ?? grammarBlock)!]);
            }
            else if (grammarPredecl is not null && grammarBlock is not null)
            {
                IList<(SmtIdentifier Name, SmtSortIdentifier Sort)>? ntDecls;
                using (var grammarPredeclScope = logger.BeginScope($"while processing grammar predeclaration:"))
                {
                    if (!converter.TryConvert(grammarPredecl, out ntDecls))
                    {
                        throw logger.LogParseErrorAndThrow("Malformed grammar predeclaration.", sourceMap[grammarPredecl]);
                    }
                }
                IList<(SmtIdentifier Name, SmtSortIdentifier Sort, IList<SemgusToken> Productions)>? productions;
                using (var grammarParseScope = logger.BeginScope($"while processing grammar block:"))
                {
                    if (!converter.TryConvert(grammarBlock, out productions))
                    {
                        throw logger.LogParseErrorAndThrow("Malformed grammar block.", sourceMap[grammarBlock]);
                    }
                }
                return new SynthFunCommand.GrammarForm(ntDecls, productions);
            }
            else
            {
                return default;
            }
        }

        private record TermTypeData(SmtIdentifier Name,
                                    SmtSort Sort,
                                    SmtSortIdentifier SortId,
                                    SemgusTermType TermType,
                                    SmtSortIdentifier TermTypeId,
                                    SemgusGrammar.NonTerminal NonTerminal);
        private record SemanticRelationData(SmtIdentifier Name,
                                            SmtScope HeadScope,
                                            SmtIdentifier TermVariableId,
                                            SmtIdentifier OutputVariableId,
                                            SmtVariable TermVariable,
                                            IList<SmtVariable> InputVariables,
                                            IList<SmtVariable> OutputVariables,
                                            SemgusChc.SemanticRelation Relation,
                                            SmtFunction Function,
                                            SmtFunctionRank Rank);
        public static (SemgusGrammar, IReadOnlyList<SemgusTermType>, SemgusTermType, SmtFunction, IReadOnlyCollection<SemgusChc>) ConvertSygusGrammar<T>(SynthFunCommand.GrammarForm grammar, IList<(SmtIdentifier, SmtSortIdentifier)> args, SmtContext ctx, ISmtConverter converter, ISourceMap sourceMap, ISourceContextProvider sourceContextProvider, ILogger<T> logger)
        {
            IDictionary<SmtIdentifier, TermTypeData> data = new Dictionary<SmtIdentifier, TermTypeData>();
            IDictionary<SmtIdentifier, SemanticRelationData> semRelInfo = new Dictionary<SmtIdentifier, SemanticRelationData>();

            //
            // Preprocess declarations and create term types
            //
            foreach (var (name, sortId) in grammar.ntDecls)
            {
                SmtSortIdentifier termTypeId = new SmtSortIdentifier(GensymUtils.Gensym("_SyTT", name.Symbol));
                SemgusTermType termType = new(termTypeId);
                ctx.AddSortDeclaration(termType);
                SmtSort sort = ctx.GetSortOrDie(sortId, sourceMap, logger);
                SemgusGrammar.NonTerminal nonTerminal = new(name, termType);
                data.Add(name, new(name, sort, sortId, termType, termTypeId, nonTerminal));
            }

            //
            // Create semantic relation functions for each term type
            //
            foreach (var (name, datum) in data)
            {
                SmtScope headScope = new(default);

                SmtSort[] argSorts = new SmtSort[1 + args.Count + 1];
                SmtVariable[] argVars = new SmtVariable[1 + args.Count + 1];
                IList<SmtVariable> inputVars = new List<SmtVariable>();
                
                argSorts[0] = datum.TermType;
                SmtIdentifier termVarId = GensymUtils.Gensym("_SyTerm", "t");
                argVars[0] = new(termVarId, new SmtVariableBinding(termVarId, datum.TermType, SmtVariableBindingType.Universal, headScope));
                
                for (int argIx = 0; argIx < args.Count; ++argIx)
                {
                    SmtSort argSort = ctx.GetSortOrDie(args[argIx].Item2, sourceMap, logger);
                    argSorts[argIx + 1] = argSort;
                    SmtIdentifier argVarId = args[argIx].Item1;
                    argVars[argIx + 1] = new(argVarId, new SmtVariableBinding(argVarId, argSort, SmtVariableBindingType.Universal, headScope));
                    inputVars.Add(argVars[argIx + 1]);
                }
                
                argSorts[^1] = datum.Sort;
                SmtIdentifier outputVarId = GensymUtils.Gensym("_SyOut", "o");
                argVars[^1] = new(outputVarId, new SmtVariableBinding(outputVarId, datum.Sort, SmtVariableBindingType.Universal, headScope));

                SmtFunctionRank semRank = new(ctx.GetSortOrDie(SmtCommonIdentifiers.BoolSortId, sourceMap, logger), argSorts);
                SmtFunction semFunc = new(GensymUtils.Gensym("_SySem", name.Symbol), sourceContextProvider.CurrentSmtSource, semRank);

                SemgusChc.SemanticRelation semRel = new(semFunc, semRank, argVars);
                semRelInfo.Add(name, new(name, headScope, termVarId, outputVarId,
                                         TermVariable: argVars[0], inputVars, new List<SmtVariable>() { argVars[^1] },
                                         semRel, semFunc, semRank));
            }

            IList<(SmtIdentifier, SmtSortIdentifier, IList<SemgusToken>)> newProdSet = new List<(SmtIdentifier, SmtSortIdentifier, IList<SemgusToken>)>();
            IList<SemgusGrammar.Production> newProds = new List<SemgusGrammar.Production>();
            List<SemgusChc> chcs = new();

            foreach (var (name, sort, productions) in grammar.Productions)
            {
                if (!data.TryGetValue(name, out var ttDatum) || !semRelInfo.TryGetValue(name, out var semDatum))
                {
                    throw logger.LogParseErrorAndThrow($"Undeclared nonterminal: {name}", sourceMap[name]);
                }

                if (ttDatum.SortId != sort)
                {
                    throw logger.LogParseErrorAndThrow($"Mismatched sort for nonterminal {name}: expected {ttDatum.SortId}, but got: {sort}", sourceMap[sort]);
                }

                foreach (var production in productions)
                {
                    SmtScope bodyScope = new(semDatum.HeadScope);
                    SmtScope auxScope = new(semDatum.HeadScope);
                    SmtVariable outputVar = new(semDatum.OutputVariableId, new(semDatum.OutputVariableId, ttDatum.Sort, SmtVariableBindingType.Universal, bodyScope));
                    SemgusTermType.Constructor constructor;
                    SmtTerm constraint;
                    IList<SmtMatchVariableBinding> matchBindings = new List<SmtMatchVariableBinding>();
                    IList<SemgusChc.SemanticRelation> bodyRels = new List<SemgusChc.SemanticRelation>();
                    if (converter.TryConvert(production, out StringToken? strToken))
                    {
                        constructor = new(GensymUtils.Gensym("_SyProd", $"\"{strToken.Value}\""));
                        newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                        constraint = SmtTermBuilder.Apply(ctx,
                                                          SmtCommonIdentifiers.EqFunctionId,
                                                          outputVar,
                                                          new SmtStringLiteral(ctx, strToken.Value));
                        
                    }
                    else if (converter.TryConvert(production, out NumeralToken? numToken))
                    {
                        constructor = new(GensymUtils.Gensym("_SyProd", $"{numToken.Value}"));
                        newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                        constraint = SmtTermBuilder.Apply(ctx,
                                                          SmtCommonIdentifiers.EqFunctionId,
                                                          outputVar,
                                                          new SmtNumeralLiteral(ctx, numToken.Value));

                    }
                    else if (converter.TryConvert(production, out BitVectorToken? bvToken))
                    {
                        string val = "#*";
                        for (int ix = bvToken.Value.Length - 1; ix >= 0; ix--)
                        {
                            val += bvToken.Value[ix] ? "1" : "0";
                        }
                        constructor = new(GensymUtils.Gensym("_SyProd", val));
                        newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                        constraint = SmtTermBuilder.Apply(ctx,
                                                          SmtCommonIdentifiers.EqFunctionId,
                                                          outputVar,
                                                          new SmtBitVectorLiteral(ctx, bvToken.Value));

                    }
                    else if (converter.TryConvert(production, out SmtIdentifier? identifier))
                    {
                        // Options: an input variable, a constant, or another non-terminal
                        if (args.Any(a => a.Item1 == identifier))
                        {
                            constructor = new(GensymUtils.Gensym("_SyProd", identifier.Symbol));
                            newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                            constraint = SmtTermBuilder.Apply(ctx,
                                                              SmtCommonIdentifiers.EqFunctionId,
                                                              outputVar,
                                                              semDatum.InputVariables.Where(v => v.Name == identifier).First());
                        }
                        else if (ctx.TryGetFunctionDeclaration(identifier, out var constant)
                              && constant.TryResolveRank(ctx, out var rank, default))
                        {
                            constructor = new(GensymUtils.Gensym("_SyProd", identifier.Symbol));
                            newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                            constraint = SmtTermBuilder.Apply(ctx,
                                                              SmtCommonIdentifiers.EqFunctionId,
                                                              outputVar,
                                                              SmtTermBuilder.Apply(ctx,
                                                                                   identifier));
                        }
                        else if (data.TryGetValue(identifier, out var cDatum))
                        {
                            var csDatum = semRelInfo[identifier];
                            constructor = new(GensymUtils.Gensym("_SyProd", $"{ttDatum.Name.Symbol}To{identifier.Symbol}"), cDatum.TermType);
                            newProds.Add(new(ttDatum.NonTerminal, constructor, new List<SemgusGrammar.NonTerminal>() { cDatum.NonTerminal }));
                            constraint = SmtTermBuilder.Apply(ctx, SmtCommonIdentifiers.TrueConstantId);
                            List<SmtVariable> cArgs = new();
                            cArgs.Add(csDatum.Relation.Arguments[0]); // Add the fresh term variable
                            cArgs.AddRange(semDatum.InputVariables);  // ...but the same input/outputs as the head
                            cArgs.AddRange(semDatum.OutputVariables);
                            bodyRels.Add(new(csDatum.Function, csDatum.Rank, cArgs));
                            SmtVariableBinding termBinding = new(csDatum.TermVariableId, cDatum.TermType, SmtVariableBindingType.Universal, bodyScope);
                            matchBindings.Add(new(termBinding, 0));
                        }
                        else
                        {
                            throw logger.LogParseErrorAndThrow("Invalid symbol: not a nonterminal, constant, or input: " + identifier, sourceMap[identifier]);
                        }
                    }
                    else if (converter.TryConvert(production, out IList<SmtIdentifier>? applTerm))
                    {
                        if (applTerm.Count == 0)
                        {
                            throw logger.LogParseErrorAndThrow("No operator found.", sourceMap[applTerm]);
                        }

                        SmtIdentifier op = applTerm[0];
                        IList<SemgusTermType> tts = new List<SemgusTermType>();
                        IList<SemgusGrammar.NonTerminal> nts = new List<SemgusGrammar.NonTerminal>();
                        IList<SmtVariable> termVars = new List<SmtVariable>();
                        IList<SmtVariableBinding> termBindings = new List<SmtVariableBinding>();
                        IList<SmtVariable> outVars = new List<SmtVariable>();
                        for (int occIx = 1; occIx < applTerm.Count; occIx++)
                        {
                            TermTypeData cDatum = data[applTerm[occIx]];
                            SemanticRelationData cSem = semRelInfo[applTerm[occIx]];
                            SmtIdentifier termId = GensymUtils.Gensym("_SyTerm", $"{occIx}");
                            SmtVariableBinding termBinding = new(termId, cDatum.TermType, SmtVariableBindingType.Universal, bodyScope);
                            SmtVariable termVar = new(termId, termBinding);
                            SmtIdentifier outId = GensymUtils.Gensym("_SyOut", $"{occIx}");
                            bodyScope.TryAddVariableBinding(outId, cDatum.Sort, SmtVariableBindingType.Universal, ctx, out var outBinding, out _);
                            auxScope.TryAddVariableBinding(outId, cDatum.Sort, SmtVariableBindingType.Universal, ctx, out _, out _);
                            SmtVariable outVar = new(outId, outBinding!);
                            tts.Add(cDatum.TermType);
                            nts.Add(cDatum.NonTerminal);
                            outVars.Add(outVar);

                            List<SmtVariable> cArgs = new();
                            cArgs.Add(termVar);
                            foreach (var a in cSem.InputVariables)
                            {
                                cArgs.Add(a);
                            }
                            cArgs.Add(outVar);

                            bodyRels.Add(new(cSem.Function, cSem.Rank, cArgs));
                            matchBindings.Add(new(termBinding, occIx - 1));
                        }

                        constructor = new(GensymUtils.Gensym("_SyProd", op.Symbol), tts.ToArray());
                        newProds.Add(new(ttDatum.NonTerminal, constructor, nts));
                        constraint = SmtTermBuilder.Apply(ctx,
                                                          SmtCommonIdentifiers.EqFunctionId,
                                                          outputVar,
                                                          SmtTermBuilder.Apply(ctx,
                                                                               op,
                                                                               outVars.ToArray()));
                    }
                    else if (converter.TryConvert(production, out IList<SemgusToken>? tokens)
                            && tokens.Count == 2
                            && converter.TryConvert(tokens[0], out SmtIdentifier? maybeMinus)
                            && maybeMinus == SmtCommonIdentifiers.MinusFunctionId
                            && (tokens[1] is NumeralToken || tokens[1] is DecimalToken))
                    {
                        if (tokens[1] is NumeralToken nt)
                        {
                            constructor = new(GensymUtils.Gensym("_SyProd", $"-{nt.Value}"));
                            newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                            constraint = SmtTermBuilder.Apply(ctx,
                                                              SmtCommonIdentifiers.EqFunctionId,
                                                              outputVar,
                                                              new SmtNumeralLiteral(ctx, -nt.Value));
                        }
                        else if (tokens[1] is DecimalToken dt)
                        {
                            constructor = new(GensymUtils.Gensym("_SyProd", $"-{dt.Value}D"));
                            newProds.Add(new(ttDatum.NonTerminal, constructor, Enumerable.Empty<SemgusGrammar.NonTerminal>()));
                            constraint = SmtTermBuilder.Apply(ctx,
                                                              SmtCommonIdentifiers.EqFunctionId,
                                                              outputVar,
                                                              new SmtDecimalLiteral(ctx, -dt.Value));
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    ttDatum.TermType.AddConstructor(constructor);
                    chcs.Add(GenerateChc(constructor, bodyRels, constraint, matchBindings, bodyScope, auxScope, ttDatum, semDatum));
                }
            }
            return (new SemgusGrammar(data.Values.Select(d => d.NonTerminal), newProds), data.Values.Select(d => d.TermType).ToList(), data[grammar.ntDecls[0].Name].TermType, semRelInfo[grammar.ntDecls[0].Name].Function, chcs);
        }

        private static SemgusChc GenerateChc(SemgusTermType.Constructor constructor, IList<SemgusChc.SemanticRelation> bodyRels, SmtTerm constraint, IList<SmtMatchVariableBinding> matchBindings, SmtScope bodyScope, SmtScope auxScope, TermTypeData ttDatum, SemanticRelationData semDatum)
        {
            SmtIdentifier chcId = GensymUtils.Gensym("_CHC", constructor.Operator.Symbol, indexed: false);
            return new(semDatum.Relation,
                                         bodyRels,
                                         constraint,
                                         new SmtMatchBinder(constraint,
                                                            bodyScope,
                                                            ttDatum.TermType,
                                                            constructor,
                                                            matchBindings),
                                         bodyScope.Bindings,
                                         semDatum.TermVariable,
                                         auxScope.Bindings,
                                         semDatum.InputVariables,
                                         semDatum.OutputVariables)
            {
                Id = chcId
            };
        }
    }
}
