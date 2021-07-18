using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;
using Semgus.Syntax;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for declaring a new term type.
    /// Syntax: (declare-term-type [typename])
    /// </summary>
    public class DeclareTermTypeCommand : ISemgusCommand
    {
        /// <summary>
        /// The name of this command
        /// </summary>
        public string CommandName => "declare-term-type";

        /// <summary>
        /// Processes a term type declaration
        /// </summary>
        /// <param name="previous">The state of the Semgus problem prior to this command</param>
        /// <param name="commandForm">Form for the command</param>
        /// <param name="errorStream">Stream to write errors to</param>
        /// <param name="errCount">Number of errors encountered</param>
        /// <returns>The state of the Semgus problem after this command</returns>
        public SemgusProblem Process(SemgusProblem previous, ConsToken commandForm, TextWriter errorStream, ref int errCount)
        {
            string err;
            SexprPosition errPos;
            if (!commandForm.TryPop(out SymbolToken _, out commandForm, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
            }

            if (!commandForm.TryPop(out SymbolToken type, out commandForm, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return previous;
            }

            if (default != commandForm)
            {
                errorStream.WriteParseError("Extra data at end of declare-term-type form: " + commandForm, commandForm.Position);
                errCount += 1;
            }

            var env = previous.GlobalEnvironment.Clone();
            if (env.IsNameDeclared(type.Name))
            {
                errorStream.WriteParseError($"Name already in use: {type.Name}. Unable to re-declare as a term type.", type.Position);
                errCount += 1;
                return previous;
            }
            env.AddTermType(type.Name, type.Position);

            return previous.UpdateEnvironment(env); 
        }
    }
}
