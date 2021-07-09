using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// Represents arbitrary annotations attached to a form.
    /// Specified as key-value pairs, e.g., :key value, with an optional
    /// single keyword as the last element
    /// </summary>
    public record Annotation(IReadOnlyDictionary<string, SemgusToken> Annotations)
    {
        /// <summary>
        /// Name of the symbol used to indicate annotations
        /// </summary>
        private static readonly string AnnotationMarker = "!";

        /// <summary>
        /// Checks if the given form has an annotation attached to it
        /// </summary>
        /// <param name="form">The form to check</param>
        /// <returns>True if annotated, false if not</returns>
        public static bool HasAnnotation(SemgusToken form)
        {
            return form is ConsToken cons && cons.Head is SymbolToken symb && AnnotationMarker == symb.Name;
        }

        /// <summary>
        /// Parses an annotation form. This form must have passed the HasAnnotation check already
        /// </summary>
        /// <param name="form">Annotated form to parse</param>
        /// <param name="target">The s-expression that was annotated</param>
        /// <param name="annotation">The parsed annotation</param>
        /// <param name="err">Error string</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if successfully parsed, false if an error was encountered</returns>
        public static bool TryParse(SemgusToken form, out SemgusToken target, out Annotation annotation, out string err, out SexprPosition errPos)
        {
            annotation = default;

            if (form is not ConsToken cons
             || !cons.TryPop(out SymbolToken annMarker, out cons, out err, out errPos)
             || AnnotationMarker != annMarker.Name)
            {
                throw new InvalidOperationException("Attempting to parse annotation, but not a valid annotation form start.");
            }

            // Grab the target; the thing being annotated
            if (!cons.TryPop(out target, out cons, out err, out errPos))
            {
                return false;
            }
            // TODO: Handle nested annotations

            // Pull out the actual annotations now
            var annotations = new Dictionary<string, SemgusToken>();
            while (default != cons)
            {
                if (!cons.TryPop(out KeywordToken key, out cons, out err, out errPos))
                {
                    return false;
                }

                // Single trailing keyword
                if (default == cons)
                {
                    annotations.Add(key.Name, new NilToken(SexprPosition.Default));
                    break;
                }

                if (!cons.TryPop(out SemgusToken value, out cons, out err, out errPos))
                {
                    return false;
                }
                annotations.Add(key.Name, value);
            }

            annotation = new Annotation(annotations);
            return true;
        }
    }
}
