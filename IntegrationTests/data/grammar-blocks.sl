;;;; -*- mode: lisp -*-
;;;;
;;;;
;;;; Tests for grammar blocks
;;;;

(declare-term-types  ((E 0)) ((($a) ($b) ($c) ($d) ($e))))

(synth-fun f () E
  ((nt_E E))
  ((nt_E E ($a $b $c $d $e))))

(check-synth)
