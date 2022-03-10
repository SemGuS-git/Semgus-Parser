﻿using System;
using System.Collections.Generic;
using System.IO;

using Semgus.Parser.Reader;

using Semgus.Model.Smt.Terms;
using Microsoft.Extensions.Logging;
using Semgus.Model.Smt.Theories;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for adding a new constraint into the SemGuS problem
    /// Syntax: (constraint [predicate])
    /// </summary>
    internal class ConstraintCommand
    {
        private readonly ISemgusProblemHandler _problemHandler;
        private readonly ISmtContextProvider _smtProvider;
        private readonly ISemgusContextProvider _semgusProvider;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<ConstraintCommand> _logger;

        public ConstraintCommand(ISemgusProblemHandler handler, ISmtContextProvider smtProvider, ISemgusContextProvider semgusProvider, ISourceMap sourceMap, ILogger<ConstraintCommand> logger)
        {
            _problemHandler = handler;
            _smtProvider = smtProvider;
            _semgusProvider = semgusProvider;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        [Command("constraint")]
        public void Constraint(SmtTerm predicate)
        {
            // Only Boolean constraints are valid
            var boolSort = _smtProvider.Context.GetSortOrDie(SmtCommonIdentifiers.SORT_BOOL, _sourceMap, _logger);
            if (predicate.Sort == boolSort)
            {
                _semgusProvider.Context.AddConstraint(predicate);
                _problemHandler.OnConstraint(_smtProvider.Context, _semgusProvider.Context, predicate);
            }
            else if (predicate.Sort == ErrorSort.Instance)
            {
                throw new InvalidOperationException("Term in constraint is in error state: " + predicate);
            }
            else
            {
                throw new InvalidOperationException("Term in constraint is not of Bool sort: " + predicate);
            }
        }
        
        
    }
}
