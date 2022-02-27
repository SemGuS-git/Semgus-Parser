;;;; -*- mode: lisp -*-
;;;;
;;;; ring-mod.sem - a non-deterministic ring modulator function
;;;;
;;;; For a carrier frequency (fc) and modulating frequency (fx),
;;;; non-deterministically return (fc - fx) or (fc + fx)
;;;;

;;; Metadata
(set-info :format-version "2.0.0")
(set-info :author "Keith Johnson")
(set-info :realizable true)

;;;
;;; Term types
;;;
(declare-term-types
 ;; Nonterminals
 ((Start 0))

 ;; Productions
 ((($fc); Start productions
   ($fx)
   ($0)
   ($1)
   ($+ ($+_1 Start) ($+_2 Start))
   ($- ($-_1 Start) ($-_2 Start))
   ($mux ($mux_1 Start) ($mux_2 Start)))))

;;;
;;; Semantics
;;;
(define-funs-rec
  ;; CHC heads
  ((Sem ((et Start) (fc Int) (fx Int) (r Int)) Bool))

  ;; Bodies
  ((! (match et ; Sem definitions
       (($fc (= r fc))
        ($fx (= r fx))
        ($0 (= r 0))
        ($1 (= r 1))
        (($+ et1 et2)
         (exists ((r1 Int) (r2 Int))
             (and
              (Sem et1 fc fx r1)
              (Sem et2 fc fx r2)
              (= r (+ r1 r2)))))
        (($- et1 et2)
         (exists ((r1 Int) (r2 Int))
             (and
              (Sem et1 fc fx r1)
              (Sem et2 fc fx r2)
              (= r (- r1 r2)))))
        (($mux et1 et2)
         (exists ((r1 Int) (r2 Int))
             (and
              (Sem et1 fc fx r1)
              (Sem et2 fc fx r2)
              (or
               (= r r1)
               (= r r2)))))))

    :input (fc fx) :output (r))))

;;;
;;; Function to synthesize - a term rooted at Start
;;;
(synth-fun ring () Start) ; Using the default universe of terms rooted at Start

;;;
;;; Constraints - logical specification
;;;
(constraint (forall ((fc Int) (fx Int) (r Int))
                (= (Sem ring fc fx r)
                   (or (= (+ fc fx) r)
                       (= (- fc fx) r)))))

;;;
;;; Instruct the solver to find ring
;;;
(check-synth)
