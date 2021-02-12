grammar Semgus;

start : synth_fun constraint* EOF;

//
// General rules
//
symbol : SYMBOL;

type : SYMBOL;

var_decl_list : '(' var_decl* ')';

var_decl : '(' symbol type ')';

nt_name : symbol;

nt_term : symbol;

//
// Synth-Fun rules
//
synth_fun : '(' 'synth-fun' symbol input_args output_args productions ')';

input_args  : var_decl_list;
            
output_args : var_decl_list;

productions : production+;

production : '(' production_lhs '(' production_rhs+ ')' ')';

//
// Non-terminals for the left-hand-side of productions
//
// TODO: parens around nt_relation_def?
production_lhs : nt_name ':' nt_relation_def ':' nt_term '[' nt_relation ']';

nt_relation_def : symbol '.' 'Sem' '(' symbol* ')';

nt_relation : var_decl_list? '(' symbol '.' 'Sem' symbol* ')'; // TODO: is var_decl_list optional?

//
// Non-terminals for the right-hand-side of productions
//
production_rhs : rhs_expression '[' predicate ']';

rhs_expression : '(' op rhs_atom* ')'
               | rhs_atom;

op : symbol;

rhs_atom : nt_name ':' nt_term
         | symbol; // Leaf

predicate : var_decl_list formula;

//
// Constraint command
//
constraint : '(' 'constraint' formula ')';

//
// General s-expression formulae
//
formula : '(' formula* ')'
        | symbol
        | symbol '.' symbol
        | symbol '.' 'Sem'
        | literal;

literal : intConst
//        | boolConst
        | bVConst
        | enumConst
        | realConst
        | quotedLit;

//
// Constants and literal definitions
//
intConst : INTEGER;
//TODO: This is causing issues with an example. Do we allow true and false as symbols?
//boolConst : 'true' | 'false';
bVConst : BVCONST;
enumConst : SYMBOL '::' SYMBOL;
realConst : REALCONST;
quotedLit : SINGLEQUOTEDLIT | DOUBLEQUOTEDLIT;

// TODO: As it is, 'Sem' is being picked up as a distinct literal. This goes for how we want to treat
// xxx.Sem terms - as a "complete" symbol, or as two distinct symbols connected by '.'.

//
// Lexer rules
//
SYMBOL : ([a-z]|[A-Z]|'_'|'+'|'-'|'*'|'&'|'|'|'!'|'~'|'<'|'>'|'='|'/'|'%'|'?'/*|'.'*/|'$'|'^')(([a-z]|[A-Z]|'_'|'+'|'-'|'*'|'&'|'|'|'!'|'~'|'<'|'>'|'='|'/'|'%'|'?'/*|'.'*/|'$'|'^') | ([0-9]))*;
// TODO: keep '.', or don't split t.sem?
// TODO: Should (just) numbers be allowed to be symbols? Probably not, but maybe?

INTEGER : ('-'?([0-9])+);

BVCONST : '#x'([0-9] | [a-f] | [A-F])+ | '#b'('0' | '1')+;

REALCONST : ('-'?([0-9])+'.'([0-9])+);

//
// We allow both single- and double-quoted literals
// ...but they are restricted to [a-zA-Z0-9.]+.
//
SINGLEQUOTEDLIT : '\''([a-z]|[A-Z]|([0-9])|'.')+'\'';
DOUBLEQUOTEDLIT : '"'([a-z]|[A-Z]|([0-9])|'.')+'"';

//
// Comments are permissive. C, C++, and Lisp-style
//
BLOCK_COMMENT : '/*' .*? '*/' -> skip;
LINE_COMMENT  : '//' ~[\r\n]* -> skip;
SEXPR_COMMENT : ';' ~[\r\n]* -> skip;

//
// Spaces, tabs, and newlines are the only whitespace we recognize
//
WS : [ \t\r\n]+ -> skip;
