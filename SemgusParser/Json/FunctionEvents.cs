using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;

namespace Semgus.Parser.Json
{
    internal class FunctionDeclarationEvent : ParseEvent
    {
        public SmtIdentifier Name { get; }
        public SmtFunctionRank Rank { get; }
        public FunctionDeclarationEvent(SmtFunction function, SmtFunctionRank rank) : base("declare-function", "smt")
        {
            Name = function.Name;
            Rank = rank;
        }
    }

    internal class FunctionDefinitionEvent : ParseEvent
    {
        public SmtSortIdentifier Name { get; }
        public IEnumerable<ConstructorModel> Constructors { get; }
         
        public FunctionDefinitionEvent(SemgusTermType tt) : base("define-function", "smt")
        {
            Name = tt.Name;
            Constructors = tt.Constructors.Select(c => new ConstructorModel(c.Operator, c.Children.Select(x => x.Name)));
        }
        public record ConstructorModel(SmtIdentifier Name, IEnumerable<SmtSortIdentifier> Children);
    }
}
