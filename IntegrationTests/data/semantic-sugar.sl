;;;; -*- mode: lisp -*-
;;;;
;;;;
;;;; Tests for sugaring semantic relation definitions
;;;;

(declare-term-types  ((E 0)) ((($a) ($b) ($c) ($d) ($e))))

(define-funs-rec
  ((E.Sem ((et E) (x Int) (y Int) (r Int)) Bool))
  ((!
    (match et
	   (($a (E.Sem et x y r)) ;; No constraints
	    ($b (> 0 x)) ;; One constraint
	    ($c (and (> 0 x) (= r 0))) ;; Multiple constraint clauses
	    ($d (E.Sem et x 0 12) (> x 3)) ;; Constants
	    ($e (E.Sem et (+ x y) (- 12 r) 89)))) ;; Other FOL formulae
    :input (x y)
    :output (r))))

(synth-fun f () E)
(constraint (E.Sem f 1 2 0))
(check-synth)
