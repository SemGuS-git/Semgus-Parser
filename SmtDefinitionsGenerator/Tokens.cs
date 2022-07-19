using System;
using System.Collections.Generic;
using System.Text;

namespace Semgus.SmtDefinitionsGenerator
{
    internal abstract class StringishToken
    {
        public string Name { get; }
        public StringishToken(string name)
        {
            Name = name;
        }
    }

    internal class SymbolToken : StringishToken
    {
        public SymbolToken(string name) : base(name) { }
    }

    internal class KeywordToken : StringishToken
    {
        public KeywordToken(string name) : base(name) { }
    }

    internal class SentinelToken : StringishToken
    {
        public SentinelToken(string name) : base(name) { }
    }
}
