;;;;
;;;; Non-terminal to non-terminal production test
;;;;

(declare-term-types ((E 0)) (()))

(synth-fun fn () E
  ((X E) (Q E))
  ((X E (Q))))

(check-synth)