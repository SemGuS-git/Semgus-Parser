using Semgus.Model.Smt.Terms;
using Semgus.Model.Smt.Theories;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Semgus.Model.Smt.Extensions
{
    /// <summary>
    /// Extended function for bit vectors
    /// </summary>
    public class SmtBitVectorsExtension : ISmtExtension
    {
        /// <summary>
        /// Static instance of SmtBitVectorsExtension
        /// </summary>
        public static SmtBitVectorsExtension Instance { get; } = new(SmtCoreTheory.Instance, SmtBitVectorsTheory.Instance);

        /// <summary>
        /// Empty collection; no sorts declared
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; } = new HashSet<SmtIdentifier>();

        /// <summary>
        /// Function symbols defined by this extension
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Dictionary of functions defined by this extension
        /// </summary>
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }

        /// <summary>
        /// Name of this extension
        /// </summary>
        public SmtIdentifier Name { get; } = SmtCommonIdentifiers.BitVectorsExtensionId;

        /// <summary>
        /// Try to get a function from the given identifier
        /// </summary>
        /// <param name="functionId">Function identifier</param>
        /// <param name="resolvedFunction">Resolved function</param>
        /// <returns>True if successfully resolved function</returns>
        public bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out IApplicable? resolvedFunction)
        {
            return Functions.TryGetValue(functionId, out resolvedFunction);
        }

        /// <summary>
        /// Always returns false. No sorts declared in this extension
        /// </summary>
        public bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort)
        {
            resolvedSort = default;
            return false;
        }

        /// <summary>
        /// Creates a new SmtBitVectorsExtension
        /// </summary>
        /// <param name="core">The Core theory</param>
        /// <param name="bv">The Bit Vectors theory</param>
        internal SmtBitVectorsExtension(SmtCoreTheory core, SmtBitVectorsTheory bv)
        {
            var bv0 = new SmtSort.WildcardSort(new(new SmtIdentifier("BitVec", "*")));
            SmtSourceBuilder sb = new(this);
            sb.AddFn(name: "bvxor",
                     val: SmtSourceBuilder.CheckArgumentSortsEqual,
                     valCmt: "Argument sorts must be of the same size",
                     retCalc: SmtSourceBuilder.UseFirstArgumentSort,
                     bv0,
                     bv0,
                     bv0)
                .DefinitionMissing((ctx, fn, rank) =>
                {
                    SmtIdentifier bvor_id = new("bvor");
                    SmtIdentifier bvand_id = new("bvand");
                    SmtIdentifier bvnot_id = new("bvnot");

                    var a1_id = new SmtIdentifier("a1");
                    var a2_id = new SmtIdentifier("a2");

                    SmtScope scope = new(default);
                    scope.TryAddVariableBinding(a1_id, rank.ArgumentSorts[0], SmtVariableBindingType.Lambda, ctx, out var a1_binding, out _);
                    scope.TryAddVariableBinding(a2_id, rank.ArgumentSorts[1], SmtVariableBindingType.Lambda, ctx, out var a2_binding, out _);

                    var a1 = new SmtVariable(a1_id, a1_binding!);
                    var a2 = new SmtVariable(a2_id, a2_binding!);

                    var b = new SmtTermBuilder(ctx);
                    return b.Lambda(scope,
                        b.Apply(bvor_id, b.Apply(bvand_id, a1, b.Apply(bvnot_id, a2)),
                                         b.Apply(bvand_id, b.Apply(bvnot_id, a1), a2)));

                });

            sb.AddFn(name: "bvugt",
                     val: SmtSourceBuilder.CheckArgumentSortsEqual,
                     valCmt: "Argument sorts must be of the same size",
                     retCalc: SmtSourceBuilder.UseFirstArgumentSort,
                     bv0,
                     bv0,
                     bv0)
                .DefinitionMissing((ctx, fn, rank) =>
                {
                    SmtIdentifier bvult_id = new("bvult");

                    var a1_id = new SmtIdentifier("a1");
                    var a2_id = new SmtIdentifier("a2");

                    SmtScope scope = new(default);
                    scope.TryAddVariableBinding(a1_id, rank.ArgumentSorts[0], SmtVariableBindingType.Lambda, ctx, out var a1_binding, out _);
                    scope.TryAddVariableBinding(a2_id, rank.ArgumentSorts[1], SmtVariableBindingType.Lambda, ctx, out var a2_binding, out _);

                    var a1 = new SmtVariable(a1_id, a1_binding!);
                    var a2 = new SmtVariable(a2_id, a2_binding!);

                    var b = new SmtTermBuilder(ctx);
                    return b.Lambda(scope,
                        b.Apply(bvult_id, a2, a1));

                });

            sb.AddFn(name: "bvsub",
                     val: SmtSourceBuilder.CheckArgumentSortsEqual,
                     valCmt: "Argument sorts must be of the same size",
                     retCalc: SmtSourceBuilder.UseFirstArgumentSort,
                     bv0,
                     bv0,
                     bv0)
                .DefinitionMissing((ctx, fn, rank) =>
                {
                    SmtIdentifier bvadd_id = new("bvadd");
                    SmtIdentifier bvnot_id = new("bvnot");

                    var a1_id = new SmtIdentifier("a1");
                    var a2_id = new SmtIdentifier("a2");
                    var c1_id = new SmtIdentifier("c1");

                    SmtScope scope = new(default);
                    scope.TryAddVariableBinding(a1_id, rank.ArgumentSorts[0], SmtVariableBindingType.Lambda, ctx, out var a1_binding, out _);
                    scope.TryAddVariableBinding(a2_id, rank.ArgumentSorts[1], SmtVariableBindingType.Lambda, ctx, out var a2_binding, out _);

                    var a1 = new SmtVariable(a1_id, a1_binding!);
                    var a2 = new SmtVariable(a2_id, a2_binding!);
                    var c1 = new SmtBitVectorLiteral(ctx, new BitArray(new int[] { 1 }));

                    var b = new SmtTermBuilder(ctx);
                    // 2's complement
                    return b.Lambda(scope,
                        b.Apply(bvadd_id, b.Apply(bvadd_id, a1, b.Apply(bvnot_id, a2)),
                                          c1));

                });

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
        }
    }
}
