using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json
{
    internal class ParseEvent
    {
        [JsonProperty("$event")]
        public string Event { get; }
        [JsonProperty("$type")]
        public string Type { get; }
        public ParseEvent(string eventName, string type)
        {
            Event = eventName;
            Type = type;
        }
    }
}
