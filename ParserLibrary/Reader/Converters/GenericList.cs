using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader.Converters
{
    internal class GenericList
    {
        public Type Type { get; }
        public object List { get; }
        private readonly MethodInfo _addMethod;

        public GenericList(Type type)
        {
            Type = type;
            List = Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
            _addMethod = List.GetType().GetMethod("Add")!;
        }

        public void Add(object item)
        {
            if (!Type.IsAssignableFrom(item.GetType()))
            {
                throw new InvalidOperationException($"Unable to add item of type {item.GetType().FullName} to generic list with type {Type.FullName}");
            }
            _addMethod.Invoke(List, new[] { item });
        }
    }
}
