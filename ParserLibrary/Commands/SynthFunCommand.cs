using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands
{
    internal class SynthFunCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _smtContext;
        private readonly ISemgusContextProvider _semgusContext;
        private readonly ISmtConverter _converter;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<SynthFunCommand> _logger;

        public SynthFunCommand(ISemgusProblemHandler handler, ISmtContextProvider smtContext, ISemgusContextProvider semgusContext, ISmtConverter converter, ISourceMap sourceMap, ILogger<SynthFunCommand> logger)
        {
            _handler = handler;
            _smtContext = smtContext;
            _semgusContext = semgusContext;
            _converter = converter;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        [Command("synth-fun")]
        public void SynthFun(SmtIdentifier name, IList<SmtConstant> args, SmtSortIdentifier retId, GrammarForm? grammarForm = default)
        {
            using var logscope = _logger.BeginScope($"while processing `synth-fun` for {name.Symbol}:");

            var ret = _smtContext.Context.GetSortOrDie(retId, _sourceMap, _logger);

            // Currently, only Semgus-style synth-funs are supported
            if (args.Count > 0 || ret is not SemgusTermType tt)
            {
                _logger.LogParseError("Only Semgus-style `synth-fun`s are supported, with no arguments and a term type as the return.", default);
                throw new InvalidOperationException("Only Semgus-style `synth-fun`s are supported, with no arguments and a term type as the return.");
            }

            var rank = new SmtFunctionRank(tt);
            var decl = new SmtFunction(name, SmtTheory.UserDefined, rank);

            // Handle the grammar declaration
            SemgusGrammar grammar;
            if (grammarForm is null)
            {
                grammar = CreateDefaultGrammar(tt);
            }
            else
            {
                grammar = CreateGrammarFromForm(grammarForm);
            }

            // Add to the places
            _smtContext.Context.AddFunctionDeclaration(decl);
            _handler.OnSynthFun(_smtContext.Context, name, args, ret);
            _semgusContext.Context.AddSynthFun(new(decl, rank, grammar));
        }

        public SemgusGrammar CreateDefaultGrammar(SemgusTermType tt)
        {
            Dictionary<SemgusTermType, SemgusGrammar.NonTerminal> nonTerminals = new();

            void ProcessNonterminal(SemgusTermType tt)
            {
                foreach (var cons in tt.Constructors)
                {
                    foreach (var child in cons.Children)
                    {
                        if (child is SemgusTermType ctt)
                        {
                            if (!nonTerminals.ContainsKey(ctt))
                            {
                                nonTerminals.Add(ctt, new(new SmtIdentifier($"@{ctt.Name}__agtt"), ctt));
                                ProcessNonterminal(ctt);
                            }
                        }
                    }
                }
            }

            nonTerminals.Add(tt, new(new SmtIdentifier($"@{tt.Name}__agtt"), tt));
            ProcessNonterminal(tt);

            List<SemgusGrammar.Production> productions = new();
            foreach (var nt in nonTerminals.Values)
            {
                foreach (var cons in nt.Sort.Constructors)
                {
                    productions.Add(new(nt, cons, cons.Children.Select(s => s is SemgusTermType stt ? nonTerminals[stt] : null).ToList()));
                }
            }

            return new SemgusGrammar(nonTerminals.Values, productions);
        }

        public SemgusGrammar CreateGrammarFromForm(GrammarForm grammarForm)
        {
            using var logScope = _logger.BeginScope("in grammar block:");

            List<SemgusGrammar.NonTerminal> nonTerminals = new();
            foreach (var (id, sortId) in grammarForm.ntDecls)
            {
                var sort = _smtContext.Context.GetSortOrDie(sortId, _sourceMap, _logger);
                if (sort is not SemgusTermType stt)
                {
                    throw _logger.LogParseErrorAndThrow($"Not a term type in synth-fun non-terminal decl: ({id} {sort.Name})", _sourceMap[sort]);
                }
                nonTerminals.Add(new(id, stt));
            }

            List<SemgusGrammar.Production> productions = new();
            foreach (var (id, sortId, terms) in grammarForm.Productions)
            {
                var sort = _smtContext.Context.GetSortOrDie(sortId, _sourceMap, _logger);
                if (sort is not SemgusTermType stt)
                {
                    throw _logger.LogParseErrorAndThrow($"Not a term type in synth-fun production: ({id} {sort.Name} ...)", _sourceMap[sort]);
                }

                var instance = nonTerminals.Find(m => m.Name == id && m.Sort == stt);
                if (instance is null)
                {
                    throw _logger.LogParseErrorAndThrow($"Nonterminal not declared in synth-fun: ({id} {sort.Name} ...)", _sourceMap[id]);
                }

                foreach (var term in terms)
                {
                    if (_converter.TryConvert(term, out SmtIdentifier? termId))
                    {
                        // A loose identifier can either be a non-terminal or a nullary constructor.
                        // Non-terminals take precedence, as nullary constructors can additionally be
                        // specified as a list with no child non-terminals.
                        var matchingNt = nonTerminals.Find(nt => nt.Name == termId);
                        if (matchingNt is not null)
                        {
                            if (instance.Sort != matchingNt.Sort)
                            {
                                throw _logger.LogParseErrorAndThrow($"Term types do not match between non-terminals in production: {instance} --> {matchingNt}", term.Position);
                            }
                            productions.Add(new(instance, matchingNt));
                        }
                        else
                        {
                            var c = stt.Constructors.FirstOrDefault(p => p.Operator == termId);
                            if (c == null)
                            {
                                throw _logger.LogParseErrorAndThrow($"Not a valid constructor or non-terminal for production: ({id} {sort.Name} ... {termId})", term.Position);
                            }
                            else if (c.Children.Length > 0)
                            {
                                throw _logger.LogParseErrorAndThrow($"Not a valid nullary constructor for production: ({id} {sort.Name} ... {termId})", term.Position);
                            }
                            else
                            {
                                productions.Add(new(instance, c, new List<SemgusGrammar.NonTerminal>()));
                            }
                        }
                    }
                    else if (_converter.TryConvert(term, out IList<SmtIdentifier>? applTerm))
                    {
                        var c = stt.Constructors.FirstOrDefault(p => p.Operator == applTerm.First());
                        if (c == null)
                        {
                            throw _logger.LogParseErrorAndThrow($"Not a valid operator for production: {id} --> {applTerm.First()}({string.Join(' ', applTerm.Skip(1).Select(s => s.Symbol))})", term.Position);
                        }
                        else if (c.Children.Length != applTerm.Count - 1)
                        {
                            throw _logger.LogParseErrorAndThrow($"Not a valid number of children for constructor for production: ({id} {sort.Name} ... {termId})", term.Position);
                        }
                        else
                        {
                            List<SemgusGrammar.NonTerminal?> occurrences = new();
                            for (int ix = 0; ix < c.Children.Length; ++ix)
                            {
                                if (c.Children[ix] is not SemgusTermType ctt)
                                {
                                    occurrences.Add(default);
                                }
                                else
                                {
                                    var oNT = nonTerminals.Find(p => p.Name == applTerm[ix + 1]);
                                    if (oNT is null)
                                    {
                                        throw _logger.LogParseErrorAndThrow($"Nonterminal not declared in synth-fun: ({id} {sort.Name} ...)", _sourceMap[applTerm[ix + 1]]);
                                    }
                                    else
                                    {
                                        occurrences.Add(oNT);
                                    }
                                }
                            }

                            productions.Add(new(instance, c, occurrences));
                        }
                    }
                    else
                    {
                        throw _logger.LogParseErrorAndThrow("Malformed production: " + term.ToString(), term.Position);
                    }
                }
            }
            return new SemgusGrammar(nonTerminals, productions);
        }

        public record GrammarForm(IList<(SmtIdentifier Name, SmtSortIdentifier Sort)> ntDecls, IList<(SmtIdentifier Name, SmtSortIdentifier Sort, IList<SemgusToken> Productions)> Productions);
    }
}
