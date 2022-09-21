using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Sorts
{
    public class SmtDatatypeConstructor : ISmtConstructor, IApplicable
    {
        public SmtDatatypeConstructor(SmtIdentifier name, SmtDatatype parent, IEnumerable<SmtSort> children, ISmtSource source)
        {
            Name = name;
            Parent = parent;
            Children = children.ToList();
            Source = source;
        }

        public SmtIdentifier Name { get; }
        
        public SmtDatatype Parent { get; }

        public IReadOnlyList<SmtSort> Children { get; }

        public ISmtSource Source { get; }

        public string GetRankHelp()
        {
            return $"({string.Join(' ', Children.Select(c => c.Name))}) -> {Parent.Name} [Constructor]";
        }

        public bool IsArityPossible(int arity)
        {
            return arity == Children.Count;
        }

        public bool TryResolveRank([NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            // TODO: handle parameterized datatypes
            if (returnSort is not null && returnSort != Parent)
            {
                rank = default;
                return false;
            }
            returnSort = Parent;

            if (Children.Count != argumentSorts.Length)
            {
                rank = default;
                return false;
            }

            for (int i = 0; i < argumentSorts.Length; ++i)
            {
                if (argumentSorts[i] != Children[i])
                {
                    rank = default;
                    return false;
                }
            }

            rank = new(returnSort, argumentSorts);
            return true;
        }
    }
}
