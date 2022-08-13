using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

namespace Semgus.Parser.Json
{
    /// <summary>
    /// Event for declaring a function. Contains only the name and rank.
    /// </summary>
    internal class FunctionDeclarationEvent : ParseEvent
    {
        /// <summary>
        /// Function name
        /// </summary>
        public SmtIdentifier Name { get; }

        /// <summary>
        /// Function rank
        /// </summary>
        public SmtFunctionRank Rank { get; }

        /// <summary>
        /// Creates a new declaration event for the given function and rank
        /// </summary>
        /// <param name="function">Function to declare</param>
        /// <param name="rank">Rank to declare</param>
        public FunctionDeclarationEvent(SmtFunction function, SmtFunctionRank rank) : base("declare-function", "smt")
        {
            Name = function.Name;
            Rank = rank;
        }
    }

    /// <summary>
    /// Event for defining a function. Contains the name, rank, and a lambda term for the definition.
    /// </summary>
    internal class FunctionDefinitionEvent : ParseEvent
    {
        /// <summary>
        /// Function name
        /// </summary>
        public SmtIdentifier Name { get; }

        /// <summary>
        /// Function rank
        /// </summary>
        public SmtFunctionRank Rank { get; }

        /// <summary>
        /// Function definition
        /// </summary>
        public SmtLambdaBinder Definition { get; }

        /// <summary>
        /// Creates a new definition event for the given function, rank, and binder
        /// </summary>
        /// <param name="function">Function to define</param>
        /// <param name="rank">Rank to define</param>
        /// <param name="lambda">Function definition</param>
        public FunctionDefinitionEvent(SmtFunction function, SmtFunctionRank rank, SmtLambdaBinder lambda) : base("define-function", "smt")
        {
            Name = function.Name;
            Rank = rank;
            Definition = lambda;
        }
    }
}
