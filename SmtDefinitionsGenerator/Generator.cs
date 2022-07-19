using Microsoft.CodeAnalysis;

using Semgus.Sexpr.Reader;

using System;
using System.Collections.Generic;
using System.Text;

namespace Semgus.SmtDefinitionsGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var files = context.AdditionalFiles;
            string text = "// Generated - additional source files\n\n";
            foreach (var file in files)
            {
                text += $"\n// ==========\n// File: {file.Path}\n// ==========\n";


                SexprReader<object> reader = new SexprReader<object>(new TokenFactory(),
                                                                     ReadtableFactory.CreateReadtable(),
                                                                     file.GetText().ToString());
                object token;
                while ((token = reader.Read(false)) != reader.EndOfFileSentinel)
                {
                    text += "// " + token.ToString() + "\n";
                }
            }

            context.AddSource("otherfiles.generated.cs", text);
        }
    }
}
