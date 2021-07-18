using Semgus.Parser.Commands;
using Semgus.Parser.Forms;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Syntax
{
    /// <summary>
    /// Visitor that collects user-defined identifiers as part of the name analysis process.
    /// </summary>
    // TODO: Do we still need this class?
    public static class LanguageEnvironmentCollector
    {
        public static LanguageEnvironment ProcessGrammar(GrammarForm gf, LanguageEnvironment env)
        {
            foreach (var varDecl in gf.VariableDeclarations)
            {
                ProcessVariableDeclaration(varDecl, env);
            }
            foreach (var ntDecl in gf.NonterminalDeclarations)
            {
                if (!env.TryResolveTermType(ntDecl.Type.Name, out var termType))
                {
                    throw new InvalidOperationException($"Invalid term type: {ntDecl.Type.Name}. Either not declared or not a term type.");
                }

                var signature = ntDecl.RelationDefinition.Types.Select(s => env.IncludeType(s.Name)).ToList();

                if (!termType.HasAssociatedSignature())
                {
                    termType.SetSignature(signature, ntDecl.RelationDefinition.Name.Position);
                }
                else if (!termType.Signature.SequenceEqual(signature))
                {
                    throw new InvalidOperationException("Signature mismatch between term type and non-terminal declaration. Signature first inferred here: " + termType.SignatureContext.ToString());
                }
                
                env.AddNonterminal(ntDecl.Name.Name, termType, new SemgusParserContext(ntDecl.Name));
                env.AddNewSemanticRelation(ntDecl.RelationDefinition.Name.Name,
                                           new SemgusParserContext(ntDecl.Name),
                                           signature);
            }
            return env;
        }

        public static LanguageEnvironment ProcessVariableDeclarationList(IEnumerable<VariableDeclarationForm> forms, LanguageEnvironment env)
        {
            foreach (var decl in forms)
            {
                ProcessVariableDeclaration(decl, env);
            }
            return env;
        }

        public static LanguageEnvironment ProcessVariableDeclaration(VariableDeclarationForm form, LanguageEnvironment env)
        {
            env.IncludeType(form.Type.Name);
            return env;
        }

        public static LanguageEnvironment ProcessVariableDeclaration(DeclareVarForm form, LanguageEnvironment env)
        {
            env.IncludeType(form.Type.Name);
            return env;
        }
    }
}