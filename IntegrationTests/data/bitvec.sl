;;;; -*- mode: lisp -*-
;;;;
;;;; Bit vector theory tests
;;;;
;;;; Not intended to be a real and solvable benchmark

(declare-term-types ((BV 0)) ((($bv))))

(define-funs-rec
  ((BV.Sem ((term BV) (x1 (_ BitVec 8))) Bool))
  ((match term
	  (($bv true)))))

(synth-fun fn () BV)

;; Concat
(constraint (BV.Sem fn (concat #b0000 #b1111)))
(constraint (BV.Sem fn (concat #b1 #b0011000)))
(constraint (BV.Sem fn (concat #xF #xA)))

;; Extract
(constraint (BV.Sem fn ((_ extract 7 0) #xFF)))
(constraint (BV.Sem fn ((_ extract 15 8) #xFFFFAAAA)))

;; Unary operators
(constraint (BV.Sem fn (bvnot #xFF)))
(constraint (BV.Sem fn (bvneg #xFF)))

;; Binary operators
(constraint (BV.Sem fn (bvand  #xAA #xBB)))
(constraint (BV.Sem fn (bvor   #xAA #xBB)))
(constraint (BV.Sem fn (bvadd  #xAA #xBB)))
(constraint (BV.Sem fn (bvmul  #xAA #xBB)))
(constraint (BV.Sem fn (bvudiv #xAA #xBB)))
(constraint (BV.Sem fn (bvurem #xAA #xBB)))
(constraint (BV.Sem fn (bvshl  #xAA #xBB)))
(constraint (BV.Sem fn (bvlshr  #xAA #xBB)))

;; Comparison
(constraint (BV.Sem fn (ite (bvult #xABCD #xDCBA) #xFF #x00)))

;; Extensions
(constraint (BV.Sem fn (bvxor #xAA #xBB)))
(constraint (BV.Sem fn (bvxor (bvxor #xAA #xCC) #xBB)))

(check-synth)
