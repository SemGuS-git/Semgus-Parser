using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Forms;
using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;
using Semgus.Syntax;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for declaring a variable globally
    /// </summary>
    public class DeclareVarCommand : ISemgusCommand
    {
        /// <summary>
        /// The name of this command
        /// </summary>
        public string CommandName => "declare-var";

        /// <summary>
        /// Processes an invocation of this command
        /// </summary>
        /// <param name="previous">The SemgusProblem state prior to this command</param>
        /// <param name="commandForm">The S-expression form of this command</param>
        /// <param name="errorStream">Stream to write errors to</param>
        /// <param name="errCount">Number of errors encountered</param>
        /// <returns>The SemgusProblem after this command, or default if parsing failed</returns>
        public SemgusProblem Process(SemgusProblem previous, ConsToken commandForm, TextWriter errorStream, ref int errCount)
        {
            // Syntax: [0] = "declare-var", [1] = <name>, [2] = <type>
            string err;
            SexprPosition errPos;

            if (!commandForm.TryPop(out SymbolToken _, out commandForm, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            if (!DeclareVarForm.TryParse(commandForm, out var declForm, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            var env = LanguageEnvironmentCollector.ProcessVariableDeclaration(declForm, previous.GlobalEnvironment.Clone());

            var newDecls = declForm.Symbols.Select(d =>
                new VariableDeclaration(d.Name, env.ResolveType(declForm.Type.Name), VariableDeclaration.Context.CT_Auxiliary));

            VariableClosure closure = new(parent: previous.GlobalClosure, newDecls);

            return previous.UpdateEnvironment(env).UpdateClosure(closure);
        }
    }
}
