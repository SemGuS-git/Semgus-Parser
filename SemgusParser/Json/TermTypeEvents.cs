using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;

namespace Semgus.Parser.Json
{
    internal class TermTypeDeclarationEvent : ParseEvent
    {
        public SmtIdentifier Name { get; }
        public TermTypeDeclarationEvent(SemgusTermType tt) : base("declare-term-type", "semgus")
        {
            Name = tt.Name;
        }
    }

    internal class TermTypeDefinitionEvent : ParseEvent
    {
        public SmtIdentifier Name { get; }
        public IEnumerable<ConstructorModel> Constructors { get; }
         
        public TermTypeDefinitionEvent(SemgusTermType tt) : base("define-term-type", "semgus")
        {
            Name = tt.Name;
            Constructors = tt.Constructors.Select(c => new ConstructorModel(c.Operator, c.Children.Select(x => x.Name)));
        }
        public record ConstructorModel(SmtIdentifier Name, IEnumerable<SmtIdentifier> Children);
    }
}
