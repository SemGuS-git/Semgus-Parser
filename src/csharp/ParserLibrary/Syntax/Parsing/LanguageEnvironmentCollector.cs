using Semgus.Parser.Commands;
using Semgus.Parser.Forms;
using Semgus.Parser.Reader;
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
        public static LanguageEnvironment ProcessSynthFun(IEnumerable<VariableDeclarationForm> varDecls, IEnumerable<ProductionForm> productions, LanguageEnvironment env)
        {
            ProcessVariableDeclarationList(varDecls, env);
            foreach (var prod in productions)
            {
                ProcessVariableDeclarationList(prod.VariableDeclarations, env);
                foreach (var prem in prod.Premises)
                {
                    ProcessVariableDeclarationList(prem.VariableDeclarations, env);
                }
                env.IncludeNonterminal(prod.Name.Name, new SemgusParserContext(prod.Name));
                env.AddNewSemanticRelation(prod.RelationDefinition.Name.Name,
                                           new SemgusParserContext(prod.Name),
                                           prod.RelationDefinition.Types.Select(s => env.IncludeType(s.Name)).ToList());
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
    }
}