using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser.Reader;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for declaring a new term type.
    /// Syntax: (declare-term-type [typename])
    /// </summary>
    public class DeclareTermTypeCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _context;
        private readonly ISmtConverter _converter;

        public DeclareTermTypeCommand(ISemgusProblemHandler handler, ISmtContextProvider context, ISmtConverter converter)
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

            List<SemgusTermType> termTypes = new();
            foreach (var decl in sortDecls)
            {
                if (decl.Arity != 0)
                {
                    throw new InvalidOperationException("Only arity 0 term types allowed.");
                }
                SemgusTermType tt = new(decl.Identifier);
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
    }
}
