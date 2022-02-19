using Microsoft.Extensions.DependencyInjection;

using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader
{
    internal class DestructuringHelper
    {
        private readonly SmtConverter _conv;

        public DestructuringHelper(SmtConverter converter)
        {
            _conv = converter;
        }

        public bool TryDestructureAndInvoke(MethodInfo method, IConsOrNil form, object? instance)
        {
            var paramInfo = method.GetParameters();
            if (!TryDestructure(paramInfo, form, out var parameters))
            {
                return false;
            }

            method.Invoke(instance, parameters);
            return true;
        }

        public bool TryDestructure(ParameterInfo[] paramInfo, IConsOrNil form, out object?[]? parameters)
        {
            parameters = new object?[paramInfo.Length];

            // form is a list of tokens that we want to match up to our arguments
            for (int paramIx = 0; paramIx < paramInfo.Length; paramIx++)
            {
                var param = paramInfo[paramIx];
                var type = param.ParameterType;

                // Check if we're out of parameters to match against
                if (form.IsNil())// !(param.IsOptional || param.GetCustomAttribute<RestAttribute>() != null))
                {
                    if (param.IsOptional)
                    {
                        form = Advance(form);
                        continue;
                    }
                    else if (param.GetCustomAttribute<RestAttribute>() is null)
                    {
                        // Cannot destructure here - out of form elements and no optional ending
                        return false;
                    }   
                }

                // Do the stuff here
                if (param.GetCustomAttribute<RestAttribute>() != null)
                {
                    if (paramIx != paramInfo.Length - 1)
                    {
                        throw new Exception("Rest parameter must be at end of method signature.");
                    }

                    if (type == typeof(ConsToken) || type == typeof(IConsOrNil))
                    {
                        parameters[paramIx] = form;
                    }
                    else if (type == typeof(IList<SemgusToken>))
                    {
                        parameters[paramIx] = Listify(form);
                    }
                    else if (_conv.TryConvert(form.GetType(), type, form, out object? value))
                    {
                        parameters[paramIx] = value;
                    }
                    else
                    {
                        throw new Exception("Rest parameters must be either a ConsToken or some sort of IList<> (a.k.a. failed to convert)");
                    }

                    // The above should have processed all remaining arguments
                    while (!form.IsNil())
                    {
                        form = Advance(form);
                    }
                    continue;
                }

                var item = form.First();

                var exactly = param.GetCustomAttribute<ExactlyAttribute>();
                if (exactly is not null)
                {
                    if (type == typeof(SmtIdentifier) && _conv.TryConvert(item, out SmtIdentifier? id))
                    {
                        if (id.Symbol == exactly.Identifier)
                        {
                            parameters[paramIx] = id;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (type == typeof(SymbolToken) && _conv.TryConvert(item, out SymbolToken? token))
                    {
                        if (token.Name == exactly.Identifier)
                        {
                            parameters[paramIx] = token;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid use of 'Exactly' attribute. Only valid on SmtIdentifier and SymbolToken parameters");
                    }

                    form = Advance(form);
                    continue;
                }

                var not = param.GetCustomAttribute<NotTypeAttribute>();
                if (not is not null && not.Types.Any(t => type.IsAssignableFrom(t)))
                {
                    return false;
                }
                else if (type.IsAssignableFrom(item.GetType()))
                {
                    parameters[paramIx] = item;
                }
                else if (_conv.TryConvert(item.GetType(), type, item, out var val)) {
                    parameters[paramIx] = val;
                }
                else
                {
                    // Unable to convert.
                    return false;
                }

                form = Advance(form);
            }

            if (!form.IsNil())
            {
                // Not enough form to satisfy the lambda list
                return false;
            }

            return true;
        }

        internal static IConsOrNil Advance(IConsOrNil cons)
        {
            IConsOrNil rest = cons.Rest();
            if (rest is null)
            {
                throw new Exception("Expected a proper list at " + ((SemgusToken)cons).Position);
            }
            else
            {
                return rest;
            }
        }

        internal static IList<SemgusToken> Listify(IConsOrNil cons)
        {
            List<SemgusToken> list = new();
            while (!cons.IsNil())
            {
                list.Add(cons.First());
                cons = Advance(cons);
            }
            return list;
        }
    }
}
