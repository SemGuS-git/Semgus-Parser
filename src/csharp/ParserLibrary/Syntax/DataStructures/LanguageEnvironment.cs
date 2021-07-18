using System;
using System.Collections.Generic;
using System.Linq;

using Semgus.Parser.Reader;
using Semgus.Util;

namespace Semgus.Syntax {
    /// <summary>
    /// Contains declarations for identifiers that are "global" within the language of a synthesis problem.
    /// </summary>
    public class LanguageEnvironment {
        private readonly Dictionary<string, SemgusType> _types = new();
        public IReadOnlyCollection <SemgusType> Types => _types.Values;
        
        private readonly Dictionary<string, Nonterminal> _nonterminals = new();
        public IReadOnlyCollection <Nonterminal> Nonterminals => _nonterminals.Values;
        
        private readonly Dictionary<string, SemanticRelationDeclaration> _relations = new();
        public IReadOnlyCollection <SemanticRelationDeclaration> Relations => _relations.Values;
        
        private readonly Dictionary<string, LibraryFunction> _libraryFunctions = new();
        public IReadOnlyCollection <LibraryFunction> LibraryFunctions => _libraryFunctions.Values;

        private readonly Dictionary<string, SemgusTermType> _termTypes = new();
        public IReadOnlyCollection<SemgusTermType> TermTypes => _termTypes.Values;

        public LanguageEnvironment Clone()
        {
            LanguageEnvironment clone = new();

            static void ShallowCopyDictionary<TKey, TValue>(IDictionary<TKey, TValue> from, IDictionary<TKey, TValue> to)
            {
                foreach (var (k, v) in from)
                {
                    to.Add(k, v);
                }
            }

            ShallowCopyDictionary(_types, clone._types);
            ShallowCopyDictionary(_nonterminals, clone._nonterminals);
            ShallowCopyDictionary(_relations, clone._relations);
            ShallowCopyDictionary(_libraryFunctions, clone._libraryFunctions);
            ShallowCopyDictionary(_termTypes, clone._termTypes);

            return clone;
        }

        public bool IsNameDeclared(string name)
        {
            return _types.ContainsKey(name)                // Term Types are a subset of all types
                || _nonterminals.ContainsKey(name)
                || _relations.ContainsKey(name)
                || _libraryFunctions.ContainsKey(name);
        }

        public SemanticRelationDeclaration AddNewSemanticRelation(string name, SemgusParserContext context, IReadOnlyList<SemgusType> elementTypes) {
            if (_relations.ContainsKey(name)) throw new Exception();

            if (IsNameDeclared(name))
            {
                throw new InvalidOperationException($"Error declaring production at: {context.Position}. Name in use: {name}");
            }

            var rel = new SemanticRelationDeclaration(
                name: name,
                elementTypes: elementTypes
            );

            _relations.Add(name, rel);
            return rel;
        }

        public SemgusTermType AddTermType(string name, SemgusParserContext declContext)
        {
            if (IsNameDeclared(name))
            {
                throw new InvalidOperationException("Name already declared: " + name);
            }

            SemgusTermType stt = new(name, declContext);
            _types.Add(name, stt);
            _termTypes.Add(name, stt);
            return stt;
        }

        public bool TryResolveTermType(string typename, out SemgusTermType type)
            => _termTypes.TryGetValue(typename, out type);

        public SemgusType IncludeType(string name) {
            if (_types.TryGetValue(name, out var value)) return value;

            value = new SemgusType(name: name);
            _types.Add(name, value);
            return value;
        }
        
        public LibraryFunction IncludeLibraryFunction(string name) {
            if (_libraryFunctions.TryGetValue(name, out var value)) return value;

            value = new LibraryFunction(name: name);
            _libraryFunctions.Add(name, value);
            return value;
        }

        public Nonterminal AddNonterminal(string name, SemgusTermType type, SemgusParserContext context) {
            
            if (IsNameDeclared(name))
            {
                throw new InvalidOperationException($"Error declaring nonterminal at: {context.Position}. Name in use: {name}");
            }

            Nonterminal value = new(name: name, type: type);
            _nonterminals.Add(name, value);
            return value;
        }
        
        public bool TryResolveRelation(string name, out SemanticRelationDeclaration value) => _relations.TryGetValue(name, out value);
        
        // TODO: hide these methods, since they can't throw proper syntax errors.
        // Currently need ResolveType(string) to handle the special case of the "Type" type.
        
        public SemgusType ResolveType(string name) => _types[name];
        public Nonterminal ResolveNonterminal(string name) => _nonterminals[name];
        public SemanticRelationDeclaration ResolveRelation(string name) => _relations[name];
        
        public string PrettyPrint() {
            var builder = new CodeTextBuilder();
            using(builder.InBraces())
            using(builder.InLineBreaks()) {
                builder.Write("types: ");
                using(builder.InBrackets()) {
                    builder.WriteEach(Types.Select(t=>t.Name), ", ");
                }
                builder.Write(",");
                builder.LineBreak();
                
                builder.Write("nonterminals: ");
                using(builder.InBrackets()) {
                    builder.WriteEach(Nonterminals.Select(t=>t.Name), ", ");
                }
                builder.Write(",");
                builder.LineBreak();
                
                builder.Write("relations: ");
                using(builder.InBrackets()) {
                    var first = true;
                    foreach(var rel in Relations) {
                        if(first) {
                            first = false;
                        } else {
                            builder.Write(",");
                        }
                        builder.LineBreak();
                        using(builder.InParens()) {
                            builder.Write(rel.Name);
                            builder.Write(" ");
                            builder.WriteEach(rel.ElementTypes.Select(t=>t.Name));
                        }
                    }
                    builder.LineBreak();
                }
                builder.Write(",");
                builder.LineBreak();
                
                builder.Write("libraryFunctions: ");
                using(builder.InBrackets()) {
                    builder.WriteEach(LibraryFunctions.Select(t=>t.Name), ", ");
                }
            }
            return builder.ToString();
        }
    }
}