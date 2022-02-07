using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;
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
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _context;
        private readonly SmtConverter _converter;

        public DeclareTermTypeCommand(ISemgusProblemHandler handler, ISmtContextProvider context, SmtConverter converter)
        {
            _handler = handler;
            _context = context;
            _converter = converter;
        }

        [Command("declare-term-types")]
        public void DeclareTermType(IList<SortDecl> sortDecls, IList<IList<ConstructorDecl>> constructors)
        {
            if (sortDecls.Count != constructors.Count)
            {
                throw new InvalidOperationException("Term names and constructor lists must be matched.");
            }

            List<TermType> termTypes = new();
            foreach (var decl in sortDecls)
            {
                if (decl.Arity != 0)
                {
                    throw new InvalidOperationException("Only arity 0 term types allowed.");
                }
                TermType tt = new(decl.Identifier);
                _context.Context.AddSortDeclaration(tt);
                termTypes.Add(tt);
            }

            for (int ix = 0; ix < termTypes.Count; ++ix)
            {
                foreach (var constructor in constructors[ix])
                {
                    List<SmtSort> children = new();
                    foreach (var child in constructor.Children)
                    {
                        if (_converter.TryConvert(child.Sort, out SmtSort? sort)) {
                            children.Add(sort);
                        }
                        else
                        {
                            throw new InvalidOperationException("Sort not declared: " + child.Sort);
                        }
                    }
                    termTypes[ix].AddConstructor(new(constructor.Constructor, children.ToArray()));
                }
            }

            _handler.OnTermTypes(termTypes);
        }

        public record SortDecl(SmtIdentifier Identifier, int Arity) { }
        public record ConstructorDecl(SmtIdentifier Constructor, [Rest] IList<(SmtIdentifier Selector, SemgusToken Sort)> Children) { }

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
