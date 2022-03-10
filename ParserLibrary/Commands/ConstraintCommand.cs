﻿using System;
using System.Collections.Generic;
using System.IO;

using Semgus.Parser.Reader;

using Semgus.Model.Smt.Terms;
using Semgus.Model.Smt.Theories;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for adding a new constraint into the SemGuS problem
    /// Syntax: (constraint [predicate])
    /// </summary>
    public class ConstraintCommand
    {
        private readonly ISemgusProblemHandler _problemHandler;
        private readonly ISmtContextProvider _smtProvider;
        private readonly ISemgusContextProvider _semgusProvider;

        public ConstraintCommand(ISemgusProblemHandler handler, ISmtContextProvider smtProvider, ISemgusContextProvider semgusProvider)
        {
            _problemHandler = handler;
            _smtProvider = smtProvider;
            _semgusProvider = semgusProvider;
        }

        [Command("constraint")]
        public void Constraint(SmtTerm predicate)
        {
            // Only Boolean constraints are valid
            var boolSort = _smtProvider.Context.GetSortDeclaration(SmtCommonIdentifiers.SORT_BOOL);
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
