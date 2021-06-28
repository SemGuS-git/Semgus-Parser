using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Root node of the Semgus syntax tree.
    /// </summary>
    public class SemgusProblem : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public SynthFun SynthFun { get; }
        public IReadOnlyList<Constraint> Constraints { get; }
        public VariableClosure GlobalClosure { get; }
        public LanguageEnvironment GlobalEnvironment { get; }

        public SemgusProblem(SynthFun synthFun,
                             VariableClosure globalClosure,
                             LanguageEnvironment globalEnvironment,
                             IReadOnlyList<Constraint> constraints) {
            SynthFun = synthFun;
            GlobalClosure = globalClosure;
            GlobalEnvironment = globalEnvironment;
            Constraints = constraints;
        }

        /// <summary>
        /// Adds a constraint to a copy of this Semgus problem
        /// </summary>
        /// <param name="constraint">The constraint to add</param>
        /// <returns>The new Semgus problem with the added constraint</returns>
        public SemgusProblem AddConstraint(Constraint constraint)
        {
            var constraints = new List<Constraint>(Constraints);
            constraints.Add(constraint);
            return new SemgusProblem(SynthFun, GlobalClosure, GlobalEnvironment, constraints);
        }

        /// <summary>
        /// Adds a synth fun to a copy of this Semgus problem.
        /// Note: multiple synth funs are not yet supported, so this replaces any previous synth fun
        /// </summary>
        /// <param name="synthFun">The synth fun to add</param>
        /// <returns>The new Semgus problem with the added synth fun</returns>
        public SemgusProblem AddSynthFun(SynthFun synthFun)
        {
            return new SemgusProblem(synthFun, GlobalClosure, GlobalEnvironment, Constraints);
        }

        /// <summary>
        /// Updates the language environment on a copy of this Semgus problem
        /// </summary>
        /// <param name="env">The new language environment to use</param>
        /// <returns>The new Semgus problem with the specified language environment</returns>
        public SemgusProblem UpdateEnvironment(LanguageEnvironment env)
        {
            return new SemgusProblem(SynthFun, GlobalClosure, env, Constraints);
        }

        /// <summary>
        /// Updates the global closure on a copy of this Semgus problem
        /// </summary>
        /// <param name="closure">The new global closure to use</param>
        /// <returns>The new Semgus problem with the specified global closure</returns>
        public SemgusProblem UpdateClosure(VariableClosure closure)
        {
            return new SemgusProblem(SynthFun, closure, GlobalEnvironment, Constraints);
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}