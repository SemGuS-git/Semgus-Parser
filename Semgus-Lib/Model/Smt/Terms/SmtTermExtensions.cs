using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    /// <summary>
    /// Extension methods for terms
    /// </summary>
    public static class SmtTermExtensions
    {
        /// <summary>
        /// Copies attributes from an old term to a new term. Modifies the new term.
        /// </summary>
        /// <typeparam name="TTerm">The type of the new term</typeparam>
        /// <param name="newTerm">The new term (destination)</param>
        /// <param name="oldTerm">The old term (source)</param>
        /// <returns>The new term</returns>
        public static TTerm CopyAnnotations<TTerm>(TTerm newTerm, SmtTerm oldTerm) where TTerm : SmtTerm
        {
            if (oldTerm.Annotations is not null)
            {
                foreach (var attr in oldTerm.Annotations)
                {
                    newTerm.AddAttribute(attr);
                }
            }
            return newTerm;
        }

        /// <summary>
        /// Copies annotations into the current term from a source term
        /// </summary>
        /// <typeparam name="TTerm">Type of term to copy into</typeparam>
        /// <param name="destTerm">This SmtTerm to copy into</param>
        /// <param name="srcTerm">Source SmtTerm to copy annotations from</param>
        /// <returns>This SmtTerm</returns>
        public static TTerm CopyAnnotationsFrom<TTerm>(this TTerm destTerm, SmtTerm srcTerm) where TTerm : SmtTerm
        {
            return CopyAnnotations(destTerm, srcTerm);
        }
    }
}
