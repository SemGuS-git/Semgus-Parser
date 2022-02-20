using Semgus.Parser.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser
{
    public static class Program
    {
        public const string Ex = @"
;;;;
;;;; max2-exp.sl - The max2 example problem encoded in SemGuS
;;;;

;;; Metadata
(set-info :format-version ""2.0.0"")
(set-info :author(""Jinwoo Kim"" ""Keith Johnson""))
(set-info :realizable true)

;;;
;;; Term types
;;;
(declare-term-types
 ;; Nonterminals
 ((E 0) (B 0))

 ;; Productions
 ((($x); E productions
  ($y)
   ($0)
   ($1)
   ($+ ($+_1 E) ($+_2 E))
   ($ite($ite_1 B) ($ite_2 E) ($ite_3 E)))

  (($t) ; B productions
   ($f)
   ($! ($!_1 B))
   ($and($and_1 B) ($and_2 B))
   ($or($or_1 B) ($or_2 B))
   ($< ($<_1 E) ($<_2 E)))))

;;;
;;; Semantics
;;;
(define-funs-rec
    ;; CHC heads
    ((E.Sem ((et E) (x Int) (y Int) (r Int)) Bool)
     (B.Sem ((bt B) (x Int) (y Int) (r Bool)) Bool))

  ;; Bodies
  ((! (match et ; E.Sem definitions
       (($x (= r x))
        ($y (= r y))
        ($0 (= r 0))
        ($1 (= r 1))
        (($+ et1 et2)
         (exists ((r1 Int) (r2 Int))
             (and
              (E.Sem et1 x y r1)
              (E.Sem et2 x y r1)
              (= r (+ r1 r2)))))
        (($ite bt etc eta)
         (exists ((rb Bool) (rc Int) (ra Int))
             (and
              (B.Sem bt x y rb)
              (E.Sem etc x y rc)
              (E.Sem eta x y ra)
              (= r(ite rb rc ra)))))))

    :input (x y) :output (r))

   (! (match bt ; B.Sem definitions
        (($t (= r true))
         ($f(= r false))
         (($! bt)
          (exists ((rb Bool))
              (and
               (B.Sem bt x y rb)
               (= r(not rb)))))
         (($and bt1 bt2)
          (exists ((rb1 Bool) (rb2 Bool))
              (and
               (B.Sem bt1 x y rb1)
               (B.Sem bt2 x y rb2)
               (= r(and rb1 rb2)))))
         (($or bt1 bt2)
          (exists ((rb1 Bool) (rb2 Bool))
              (and
               (B.Sem bt1 x y rb1)
               (B.Sem bt2 x y rb2)
               (= r(or rb1 rb2)))))
         (($< et1 et2)
          (exists ((r1 Int) (r2 Int))
              (and
               (E.Sem et1 x y r1)
               (E.Sem et2 x y r2)
               (= r(< r1 r2)))))))
    :input (x y) :output (r))))

;;;
;;; Function to synthesize - a term rooted at E
;;;
(synth-fun max2() E) ; Using the default universe of terms rooted at E

;;;
;;; Constraints - examples
;;;
(constraint (E.Sem max2 4 2 4))
(constraint (E.Sem max2 2 5 5))

;;;
;;; Constraints - logical specification
;;;
(constraint (forall ((x Int) (y Int) (r Int))
                (= (E.Sem max2 x y r)
                   (and (or (= x r)
                            (= y r))
                        (>= r x)
                        (>= r y)))))

;;;
;;; Instruct the solver to find max2
;;;
(check-synth)

";

        public enum OutputFormat
        {
            None,
            Json
        }

        public enum ProcessingMode
        {
            Stream,
            Batch
        }

        public static int Main(string[] args)
        {
            OutputFormat format = OutputFormat.None;
            ProcessingMode mode = ProcessingMode.Stream;
            bool test = false;
            string? output = default;
            int argIx = 0;
            while (argIx < args.Length)
            {
                switch (args[argIx].ToLowerInvariant())
                {
                    case "--format":
                        if (args[++argIx].ToLowerInvariant() == "json")
                        {
                            format = OutputFormat.Json;
                        }
                        else
                        {
                            Console.Error.WriteLine($"error: unsupported format: {args[argIx]}. Only json is supported.");
                            return 1;
                        }
                        argIx += 1;
                        break;
                    case "--mode":
                        if (args[++argIx].ToLowerInvariant() == "batch")
                        {
                            mode = ProcessingMode.Batch;
                        }
                        else if (args[argIx].ToLowerInvariant() == "stream")
                        {
                            mode = ProcessingMode.Stream;
                        }
                        else
                        {
                            Console.Error.WriteLine($"error: unsupported mode: {args[argIx]}. Only batch and stream are supported.");
                        }
                        argIx += 1;
                        break;

                    case "--output":
                        output = args[++argIx];
                        break;

                    case "--test":
                        test = true;
                        argIx += 1;
                        break;

                    case "--":
                        argIx += 1;
                        goto ExitArgParsing;

                    default:
                        goto ExitArgParsing;
                }
            }
            
        // Remaining arguments are filenames
        ExitArgParsing:
            List<string> inputs = new();
            while (argIx < args.Length)
            {
                inputs.Add(args[argIx++]);
            }
            if (inputs.Count == 0)
            {
                inputs.Add("-");
            }

            if (test)
            {
                Test(mode);
                return 0;
            }

            using TextWriter outputWriter = output is not null ? File.CreateText(output) : Console.Out;
            using var handler = new JsonHandler(outputWriter, mode);
            foreach (var input in inputs)
            {
                using SemgusParser parser = (input == "-") ? new(Console.In, "stdin") : new(input);
                if (!parser.TryParse(handler))
                {
                    Console.Error.WriteLine("error: fatal error reported while parsing " + (input == "-" ? "standard input" : input));
                    return 2;
                }
            }

            return 0;
        }

        private static void Test(ProcessingMode mode)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(Ex);
            writer.Flush();
            stream.Position = 0;
            SemgusParser parser = new(stream, "string");
            using var handler = new JsonHandler(Console.Out, mode);
            parser.TryParse(handler);
        }
    }
}
