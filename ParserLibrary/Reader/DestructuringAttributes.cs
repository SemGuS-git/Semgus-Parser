using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RestAttribute : Attribute { }

    public class NotTypeAttribute : Attribute
    { 
        public NotTypeAttribute(params Type[] types)
        {
            Types = types.ToList();
        }

        public IReadOnlyList<Type> Types { get; }
    }

    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ExactlyAttribute : Attribute
    {
        public ExactlyAttribute(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }
    }
}
