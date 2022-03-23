﻿using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Commands
{
    internal class SynthFunCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _smtContext;
        private readonly ISemgusContextProvider _semgusContext;
        private readonly SmtConverter _converter;
        private readonly ILogger<SynthFunCommand> _logger;

        public SynthFunCommand(ISemgusProblemHandler handler, ISmtContextProvider smtContext, ISemgusContextProvider semgusContext, SmtConverter converter, ILogger<SynthFunCommand> logger)
        {
            _handler = handler;
            _smtContext = smtContext;
            _semgusContext = semgusContext;
            _converter = converter;
            _logger = logger;
        }

        [Command("synth-fun")]
        public void SynthFun(SmtIdentifier name, IList<SmtConstant> args, SmtSort ret, GrammarForm? grammarForm = default)
        {
            // Currently, only Semgus-style synth-funs are supported
            if (args.Count > 0 || ret is not SemgusTermType tt)
            {
                _logger.LogParseError("Only Semgus-style `synth-fun`s are supported, with no arguments and a term type as the return.", default);
                throw new InvalidOperationException("Only Semgus-style `synth-fun`s are supported, with no arguments and a term type as the return.");
            }

            var rank = new SmtFunctionRank(tt);
            var decl = new SmtFunction(name, rank);

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
            List<SemgusGrammar.NonTerminal> nonTerminals = new();
            foreach (var (id, sort) in grammarForm.ntDecls)
            {
                if (sort is not SemgusTermType stt)
                {
                    _logger.LogParseError($"Not a term type in synth-fun non-terminal decl: ({id} {sort.Name})", default);
                    throw new InvalidOperationException($"Not a term type in synth-fun non-terminal decl: ({id} {sort.Name})");
                }
                nonTerminals.Add(new(id, stt));
            }

            List<SemgusGrammar.Production> productions = new();
            foreach (var (id, sort, terms) in grammarForm.Productions)
            {
                if (sort is not SemgusTermType stt)
                {
                    _logger.LogParseError($"Not a term type in synth-fun production: ({id} {sort.Name} ...)", default);
                    throw new InvalidOperationException($"Not a term type in synth-fun production: ({id} {sort.Name} ...)");
                }

                var instance = nonTerminals.Find(m => m.Name == id && m.Sort == stt);
                if (instance is null)
                {
                    _logger.LogParseError($"Nonterminal not declared in synth-fun: ({id} {sort.Name} ...)", default);
                    throw new InvalidOperationException($"Nonterminal not declared in synth-fun: ({id} {sort.Name} ...)");
                }

                foreach (var term in terms)
                {
                    if (_converter.TryConvert(term, out SmtIdentifier? termId))
                    {
                        var c = stt.Constructors.FirstOrDefault(p => p.Operator == termId);
                        if (c == null)
                        {
                            _logger.LogParseError($"Not a valid constructor for production: ({id} {sort.Name} ... {termId})", term.Position);
                            throw new InvalidOperationException($"Not a valid constructor for production: ({id} {sort.Name} ... {termId})");
                        }
                        else if (c.Children.Length > 0)
                        {
                            _logger.LogParseError($"Not a valid nullary constructor for production: ({id} {sort.Name} ... {termId})", term.Position);
                            throw new InvalidOperationException($"Not a valid nullary constructor for production: ({id} {sort.Name} ... {termId})");
                        }
                        else
                        {
                            productions.Add(new(instance, c, new List<SemgusGrammar.NonTerminal>()));
                        }
                    }
                    else if (_converter.TryConvert(term, out IList<SmtIdentifier>? applTerm))
                    {
                        var c = stt.Constructors.FirstOrDefault(p => p.Operator == applTerm.First());
                        if (c == null)
                        {
                            _logger.LogParseError($"Not a valid constructor for production: ({id} {sort.Name} ... {termId})", term.Position);
                            throw new InvalidOperationException($"Not a valid constructor for production: ({id} {sort.Name} ... {termId})");
                        }
                        else if (c.Children.Length != applTerm.Count - 1)
                        {
                            _logger.LogParseError($"Not a valid number of children for constructor for production: ({id} {sort.Name} ... {termId})", term.Position);
                            throw new InvalidOperationException($"Not a valid number of children for constructor for production: ({id} {sort.Name} ... {termId})");
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
                                        _logger.LogParseError($"Nonterminal not declared in synth-fun: ({id} {sort.Name} ...)", default);
                                        throw new InvalidOperationException($"Nonterminal not declared in synth-fun: ({id} {sort.Name} ...)");
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
                }
            }
            return new SemgusGrammar(nonTerminals, productions);
        }

        public record GrammarForm(IList<(SmtIdentifier Name, SmtSort Sort)> ntDecls, IList<(SmtIdentifier Name, SmtSort Sort, IList<SemgusToken> Productions)> Productions);
    }
}
