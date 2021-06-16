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
production_lhs : nt_name ':' '(' nt_relation_def ')' ':' nt_term '[' nt_relation ']';

nt_relation_def : symbol '(' symbol* ')';

nt_relation : var_decl_list '(' symbol symbol* ')';

//
// Non-terminals for the right-hand-side of productions
//
production_rhs : rhs_expression '[' predicate ']';

rhs_expression : '(' op rhs_atom* ')'
               | rhs_atom;

op : symbol;

rhs_atom : nt_name ':' nt_term
         | symbol    // Leaf
         | literal ; // We allow literals as leaf names, too

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
        | literal;

literal : intConst
        | boolConst
        | bVConst
        | enumConst
        | realConst
        | quotedLit;

//
// Constants and literal definitions
//
intConst : INTEGER;
boolConst : 'true' | 'false';
bVConst : BVCONST;
enumConst : SYMBOL '::' SYMBOL;
realConst : REALCONST;
quotedLit : SINGLEQUOTEDLIT | DOUBLEQUOTEDLIT;

//
// Lexer rules
//
SYMBOL : ([a-z]|[A-Z]|'_'|'+'|'-'|'*'|'&'|'|'|'!'|'~'|'<'|'>'|'='|'/'|'%'|'?'|'.'|'$'|'^')(([a-z]|[A-Z]|'_'|'+'|'-'|'*'|'&'|'|'|'!'|'~'|'<'|'>'|'='|'/'|'%'|'?'|'.'|'$'|'^') | ([0-9]))*;

INTEGER : ('-'?([0-9])+);

BVCONST : '#x'([0-9] | [a-f] | [A-F])+ | '#b'('0' | '1')+;

REALCONST : ('-'?([0-9])+'.'([0-9])+);

//
// We allow both single- and double-quoted literals
// no explicit support for escape sequences
//
SINGLEQUOTEDLIT : '\'' (~['])* '\'';
DOUBLEQUOTEDLIT : '"' (~["])* '"';

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
