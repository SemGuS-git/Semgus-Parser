using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Reader;

namespace Semgus.Syntax
{
    /// <summary>
    /// The type of a term
    /// </summary>
    public class SemgusTermType : SemgusType
    {
        /// <summary>
        /// List of types that a semantic relation for this type must follow
        /// </summary>
        public IReadOnlyList<SemgusType> Signature { get; private set; }

        /// <summary>
        /// Context of where this type was declared
        /// </summary>
        public SemgusParserContext DeclarationContext { get; }

        /// <summary>
        /// Context of where the signature for this type was first inferred
        /// </summary>
        public SemgusParserContext SignatureContext { get; private set; }

        /// <summary>
        /// True if a signature has been inferred for this type
        /// </summary>
        public bool HasAssociatedSignature() => Signature is not null;

        /// <summary>
        /// Updates this type with an inferred signature. It is an error to call this if a signature has already been set
        /// </summary>
        /// <param name="signature">The new signature</param>
        /// <param name="sigContext">Where the signature was inferred from</param>
        public void SetSignature(IReadOnlyList<SemgusType> signature, SemgusParserContext sigContext)
        {
            if (HasAssociatedSignature())
            {
                throw new InvalidOperationException("Attempt to set signature of term type that already has one.");
            }
            Signature = signature;
            SignatureContext = sigContext;
        }

        /// <summary>
        /// Creates a new SemgusTermType with the given name
        /// </summary>
        /// <param name="name">The term type name</param>
        /// <param name="context">Where this type was declared</param>
        public SemgusTermType(string name, SemgusParserContext context) : base(name)
        {
            DeclarationContext = context;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
