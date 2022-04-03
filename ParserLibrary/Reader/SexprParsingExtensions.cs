using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    public static class SexprParsingExtensions
    {
        public static bool TryPop<TSexpr>(this ConsToken cons, [NotNullWhen(true)] out TSexpr? head, out ConsToken? tail, [NotNullWhen(false)] out string? err, out SexprPosition? errPos)
        where TSexpr : SemgusToken
        {
            if (cons is null)
            {
                err = $"End of list encountered when expecting {typeof(TSexpr).Name}";
                errPos = SexprPosition.Default; // This is unfortunate, because we don't have a position to report
                head = default;
                tail = default;
                return false;
            }

            if (cons.Head is not TSexpr headSexpr)
            {
                err = $"Expected expression of type {typeof(TSexpr).Name}, but got {cons.Head.GetType().Name}";
                errPos = cons.Head.Position;
                head = default;
                tail = default;
                return false;
            }

            if (cons.Tail is NilToken)
            {
                head = headSexpr;
                tail = default;
                err = default;
                errPos = default;
                return true;
            }
            else if (cons.Tail is not ConsToken tailSexpr)
            {
                err = $"Expected proper list, but found atom in tail: {cons.Tail}";
                errPos = cons.Tail.Position;
                head = default;
                tail = default;
                return false;
            }
            else
            {
                head = headSexpr;
                tail = tailSexpr;
                err = default;
                errPos = default;
                return true;
            }
        }

        public static void WriteParseError(this TextWriter writer, string err, SexprPosition? errPos)
        {
            writer.Write($"{errPos?.Source}:{errPos?.Line}:{errPos?.Column}: error: ");
            writer.WriteLine(err);
        }
    }
}
