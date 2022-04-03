﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

namespace Semgus.Parser
{
    /// <summary>
    /// Interface that receives messages during reading of a SemGuS problem file
    /// </summary>
    public interface ISemgusProblemHandler
    {
        /// <summary>
        /// Called when new term types are declared
        /// </summary>
        /// <param name="termTypes">The declared term types</param>
        public void OnTermTypes(IReadOnlyList<SemgusTermType> termTypes);

        /// <summary>
        /// Called when a synth-fun command is encountered. NOTE: this doesn't have SemGuS-specific information yet.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="sort"></param>
        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<SmtConstant> args, SmtSort sort);

        /// <summary>
        /// Called when metadata is set via the set-info command
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="attr"></param>
        public void OnSetInfo(SmtContext ctx, SmtAttribute attr);

        /// <summary>
        /// Called when a constraint is encountered
        /// </summary>
        /// <param name="smtCtx"></param>
        /// <param name="semgusCxt"></param>
        /// <param name="constraint"></param>
        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint);

        /// <summary>
        /// Called when a check-synth command is encountered
        /// </summary>
        /// <param name="smtCtx"></param>
        /// <param name="semgusCtx"></param>
        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx);
    }
}
