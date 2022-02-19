using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json
{
    internal class SetInfoEvent : ParseEvent
    {
        public string Keyword { get; }
        public SmtAttributeValue Value { get; }
        public SetInfoEvent(SmtAttribute attr) : base("set-info", "meta")
        {
            Keyword = attr.Keyword.Name;
            Value = attr.Value;
        }
    }
}
