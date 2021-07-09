using System;

namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// The role each constituent plays in atoms. See § 2.1.4.2 of the CLHS for details.
    /// Our implementation is more restrictive than in CL, but it adheres to the SMT-LIB2 spec.
    /// </summary>
    [Flags]
    public enum ConstituentTrait
    {
        Invalid = 0,
        Alphabetic = 1,
        Digit = 2,
        DecimalPoint = 4,
        Zero = 8,
        PackageMarker = 16
    }
}
