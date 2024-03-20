using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Common identifiers for well-used objects
    /// </summary>
    public static class SmtCommonIdentifiers
    {
        public static SmtIdentifier CoreTheoryId { get; } = new("Core");
        public static SmtIdentifier IntsTheoryId { get; } = new("Ints");
        public static SmtIdentifier StringsTheoryId { get; } = new("Strings");
        public static SmtIdentifier BitVectorsTheoryId { get; } = new("BitVectors");
        public static SmtIdentifier BitVectorsExtensionId { get; } = new("BitVectors", "extension");
        public static SmtIdentifier ArraysExTheoryId { get; } = new("ArraysEx");
        public static SmtIdentifier SequencesTheoryId { get; } = new("Sequences");

        public static SmtSortIdentifier BoolSortId { get; } = new("Bool");
        public static SmtSortIdentifier IntSortId { get; } = new("Int");
        public static SmtSortIdentifier StringSortId { get; } = new("String");
        public static SmtSortIdentifier RegLanSortId { get; } = new("RegLan");
        public static SmtSortIdentifier RealSortId { get; } = new("Real");
        public static SmtIdentifier BitVectorSortPrimaryId { get; } = new("BitVec");
        public static BitVectorSortIndexer BitVectorSortId { get; } = new BitVectorSortIndexer();
        public class BitVectorSortIndexer
        {
            public SmtSortIdentifier this[int size]
            {
                get => new(new SmtIdentifier(BitVectorSortPrimaryId.Symbol,
                                             new SmtIdentifier.Index(size)));
            }
        }
        public static SmtIdentifier ArraySortPrimaryId { get; } = new("Array");
        public static SmtIdentifier SeqSortPrimaryId { get; } = new("Seq");

        public static SmtIdentifier AndFunctionId { get; } = new("and");
        public static SmtIdentifier OrFunctionId { get; } = new("or");
        public static SmtIdentifier EqFunctionId { get; } = new("=");
        public static SmtIdentifier TrueConstantId { get; } = new("true");
        public static SmtIdentifier FalseConstantId { get; } = new("false");
        public static SmtIdentifier MinusFunctionId { get; } = new("-");
    }
}
