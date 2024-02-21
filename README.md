# SemGuS Parser
[![.NET Actions Status](https://github.com/SemGuS-git/Semgus-Parser/workflows/.NET/badge.svg)](https://github.com/SemGuS-git/Semgus-Parser/actions)
[![NuGet Badge](https://buildstats.info/nuget/Semgus.Parser?dWidth=0)](https://www.nuget.org/packages/Semgus.Parser/)

A C# parsing library for SemGuS problems. We also provide a standalone tool for verifying and converting
SemGuS files. Find the latest release and binaries on the [Releases page](https://github.com/SemGuS-git/Semgus-Parser/releases).

### Features
* Parses SemGuS problems from SMT-LIB2 source files
* Displays constrained Horn clauses (CHCs) and other problem data in a human-inspectable format
* Converts SMT-LIB2 SemGuS problems into JSON for consumption by other tools
* Allows programatic access via a C# API

### Examples
To verify the syntax of a problem file and display details in a human-inspectable format, run:
```
semgus-parser <input.sem>
```

To convert a problem file to JSON, run:
```
semgus-parser --format json --mode batch --output <output.json> -- <input.sem>
```

Some examples of SemGuS files can be found in the [IntegrationTests/data](IntegrationTests/data) directory.

### Caveats and Considerations
This project is under active development and does not yet support all SMT-LIB2 features. The following theories are currently supported:
* Core
* Ints
* Strings (with partial support for regular expressions)
* Bit vectors (partial support; only theory functions supported for now)
* Datatypes (non-parametric only)
* Arrays (partial support)

Other features:
* Converting SyGuS problems to CHCs on the fly (beta)
* Declarative S-expression output format (beta). Use `--format sexpr`

New additions:
* Datatypes (non-parametric only)
* Auxiliary functions
* Arbitrary `match` expressions

Unsupported SMT-LIB2 features include:
* Theory functions annotated with `:left-assoc`, `:right-assoc`, `:chainable`, and `:pairwise`. Certain Core functions are implemented, so post an issue if others are needed.
* Some terms, including `let`
* Uninterpreted sorts (`declare-sort`) and sort aliases (`define-sort`)

The roadmap for next-up features includes:
* Arbitrary `let` terms

If there is an unsupported feature that you would like added, drop us a line by submitting an issue (or commenting on an existing one).
This will help us prioritize what to put on our roadmap.

## Installation
To install the stand-alone parsing tool, find the binary for your operating system on the [Releases page](https://github.com/SemGuS-git/Semgus-Parser/releases). Unzip it and put the `semgus-parser` (or `semgus-parser.exe`) in a convenient location. No other dependencies are required. You may have to mark it
executable on Linux and macOS; just run `chmod a+x semgus-parser` in the folder with the binary.

If you have the .NET 6 SDK installed, the parsing tool can also be installed automatically through NuGet:
```
dotnet tool install --global Semgus.Parser.Tool
```
The C# parsing library is available in NuGet. Look for the `Semgus.Parser` package.

## Usage

```
semgus-parser [--format <format>] [--mode <mode>] [--output <filename>] -- <input.sem> ...
```
Passing `-` as the input filename (or not supplying any filenames) makes the tool read from standard input.

# Format Verification
Run the tool with a format of `verify` to perform a syntax verification. In addition to syntax verification,
this format prints out the problem information, which can be checked for correctness. This output
format is not designed to be machine readable; use the JSON output format instead.

# JSON Converter
The `SemgusParser` project contains a utility that reads in a SemGuS file and produces JSON data representing
the problem, usable by other non-.NET tools that cannot directly link with this library.

## Usage

```
semgus-parser --format json [--mode stream|batch] [--output <filename.json>] -- <input.sem> ...
```
Passing `-` as the input filename (or not supplying any filenames) makes the tool read from standard input.

The two modes, `stream` and `batch`, alter the output format of this tool. In `stream` mode (the default),
the output is a stream of newline-delimited JSON objects representing parsing events. This is suitable for
interactive mode, or when the output of this tool is being directly piped to the consuming process. In
`batch` mode, the output is a JSON array, with each item in the array being a parsing event object. This format
is suitable for outputting to a file as a complete JSON datatype. Note that the event object format is identical
between these two modes; only the output structure differs.

In stream mode:
```
{ ...event 1... }
{ ...event 2... }
{ ...event 3... }
.
.
.
```

In batch mode:
```
[
  { ...event 1... },
  { ...event 2... },
  { ...event 3... },
  .
  .
  .
]
```

## Event object format
Each event object has two fields that denote what type of event it represents: `$event` and `$type`. The `$event`
field holds the name of the particular event. The `$type` field denotes the general type of event: currently, either

* `semgus`, for a SemGuS-specific event
* `smt`, for a general SMT-LIB2 event
* `meta`, for metadata about the problem

### `meta` events
#### `set-info`
Holds general metadata about the problem. The `<value>` is either a string, number, or list of values (which might be lists, strings, numbers, etc.).
```
{ "$event": "set-info", "$type": "meta", "keyword": "<key>", "value": <value> }
```
#### `end-of-stream`
Sent at the end of the event stream.
```
{ "$event": "end-of-stream", "$type": "meta" }
```

### `semgus` events
#### `check-synth`
Signals that the synthesizer should perform synthesis with the information provided in previous events.
```
{ "$event": "check-synth", "$type": "semgus" }
```
#### `declare-term-type`
Declares that a particular symbol will be used as a term type. Does not contain the actual term type definition.
```
{ "$event": "declare-term-type", "$type": "semgus", "name": "<term-type-name>" }
```
#### `define-term-type`
Defines a set of constructors for a term type. All term types referenced will have been previously declared by a `declare-term-type` event.
```
{ "$event": "define-term-type", "$type": "semgus", "name": "<term-type-name>", "constructors": [ <constructors> ] }
```
Each constructor (a.k.a. operator) has the form:
```
{ "name": "<constructor-name>", "children": ["<term-type>", ...] }
```
Example:
```
{
  "name": "E",
  "constructors": [
    {
      "name": "$x", "children": []
    },
    {
      "name": "$y", "children": []
    },
    {
      "name": "$0", "children": []
    },
    {
      "name": "$1", "children": []
    },
    {
      "name": "$+", "children": ["E", "E"]
    },
    {
      "name": "$ite", "children": ["B", "E", "E"]
    }
  ],
  "$event": "define-term-type",
  "$type": "semgus"
}
```
#### `chc`
An individual constrained Horn clause generated by the problem.
```
{"head": <head-rel>, "bodyRelations":[<body-rels>...], "symbols":<symbol-table>, "constraint": <term>, "constructor": <constructor>,"$event":"chc","$type":"semgus"}
```
Where each semantic relation has the form:
```
{"name":"E.Sem","signature":["E","Int","Int","Int"],"arguments":["et","x","y","r"]}
```
The constructor has the form:
```
{"name":"<name>","arguments":["<var>"...],"argumentSorts":["<sort>"...],"returnSort":"<sort>"}
```
Terms have the general form:
```
{ "$termType": "<type>", ... }
```
Term types include `application`, `exists`, `forall`, and `variable`. Literals are simply a literal string or number.
An example term:
```
{"name":"=","returnSort":"Bool","argumentSorts":["Int","Int"],"arguments":[{"name":"r","sort":"Int","$termType":"variable"},{"name":"y","sort":"Int","$termType":"variable"}],"$termType":"application"}
```
##### Symbol Table
The symbol table (denoted `<symbol-table>` above) contains information about what symbols appear in the top-level
bindings of the CHC head and body. Each symbol entry contains the symbol identifier, sort, and an optional index:
```
{"id": <id>, "sort": <sort>, "index": <index>}
```
The symbol table object has six properies, all but one (`term`) an array of symbol entries:

* `inputs`: symbols that are annotated as CHC input variables. The index is the zero-based offset into the head signature.
* `outputs`: symbols that are annotated as CHC output variables. The index is the zero-based offset into the head signature.
* `term`: a single symbol entry for the CHC term variable. The index is the zero-based offset into the head signature.
* `children`: symbols that are bound as child terms of the main CHC term. The index is the zero-based offset of the child in the term constructor.
* `auxiliary`: symbols that are bound at the top level of the CHC body. The index is unspecified.
* `unclassified`: symbols that appear in the CHC head but are not the term variable or annotated as an input or output. The index is the zero-based offset into the head signature.

Of note, `inputs`, `outputs`, `term`, and `unclassified` are bound outside of `children`, and `auxiliary` inside `children`.
This means that a variable with the same identifier that appears in, e.g., `auxiliary`, will shadow a same-named variable
in, e.g., `inputs` or `children`.

Note that the CHC event may contain the following deprecated properties for symbol information, which should be ignored:

* `inputVariables`
* `outputVariables`
* `variables`

These properties will be removed in a future release.
##### Example:
```
{
    "head":
    {
        "name":"E.Sem",
        "signature":["E","Int","Int","Int"],
        "arguments":["et","x","y","r"]
    },
    "bodyRelations":
    [
        {
            "name":"E.Sem",
            "signature":["E","Int","Int","Int"],
            "arguments":["et1","x","y","r1"]
        },
        {
            "name":"E.Sem",
            "signature":["E","Int","Int","Int"],
            "arguments":["et2","x","y","r2"]
        }
    ],
    "inputVariables":["x","y"],
    "outputVariables":["r"],
    "variables":["et","x","y","r","r1","r2"],
    "constraint":
    {
        "name":"=",
        "returnSort":"Bool",
        "argumentSorts":["Int","Int"],
        "arguments":
        [
            {
                "name":"r",
                "sort":"Int",
                "$termType":"variable"
            },
            {
                "name":"+",
                "returnSort":"Int",
                "argumentSorts":["Int","Int"],
                "arguments":
                [
                    {"name":"r1","sort":"Int","$termType":"variable"},
                    {"name":"r2","sort":"Int","$termType":"variable"}
                ],
                "$termType":"application"
            }
        ],
        "$termType":"application"
    },
    "constructor":
    {
        "name":"$+",
        "arguments":["et1","et2"],
        "argumentSorts":["E","E"],
        "returnSort":"E"
    },
    "symbols":
    {
        "inputs":
        [
            {"id":"x","sort":"Int","index":1},
            {"id":"y","sort":"Int","index":2}
        ],
        "outputs":
        [
            {"id":"r","sort":"Int","index":3}
        ],
        "term":{"id":"et","sort":"E","index":0},
        "unclassified":[],
        "auxiliary":
        [
            {"id":"r1","sort":"Int","index":null},
            {"id":"r2","sort":"Int","index":null}
        ],
        "children":
        [
            {"id":"et1","sort":"E","index":0},
            {"id":"et2","sort":"E","index":1}
        ]
    },
    "$event":"chc",
    "$type":"semgus"
}
```
#### `constraint`
A Boolean predicate term.
```
{ "$event": "constraint", "$type": "semgus", "constraint": <term> }
```
The term is defined as above.
#### `synth-fun`
Contains the name of the function to synthesize, as well as the concrete grammar being used.
```
{ "$event": "synth-fun", "$type": "semgus", "grammar": <grammar>, "name": "<name>", "termType": "<name>" }
```
The grammar object is defined as:
```
{ "nonTerminals": [ {"name": "<name>", "termType": "<name>"}, ...], "productions": [<production>...] }
```
Each production object is:
```
{ "instance": "<nt-name>", "operator": "<op-name>", "occurrences": ["<nt-name>"] }
```
Note that an occurrence will be `null` if it is not a non-terminal in the operator constructor, i.e. a discovered constant.

### `smt` events
#### `declare-function`
A function declaration, including a name and signature.
The following declares a function `foo` with no arguments that returns a string:
```
{"$event":"declare-function","$type":"smt","name":"foo","rank":{"argumentSorts":[],"returnSort":"String"}}
```

#### `define-function`
A function definition, including a name, signature, and lambda term implementing the function body.
Note that a `declare-function` event will always precede the `define-function` event (but not necessarily immediately).
The following defines the `foo` function to return a constant string `"Foo"`:
```
{"$event":"define-function","$type":"smt","name":"foo","rank":{"argumentSorts":[],"returnSort":"String"},"definition":{"arguments":[],"body":"Foo","$termType":"lambda"}}
```

#### `declare-datatype`
A datatype declaration, including a type name and arity (currently always zero). Constructors are not included.
The following declares a datatype `Pair`:
```
{"$event":"declare-datatype","$type":"smt","name":"Pair","arity":0}
```

#### `define-datatype`
A datatype definition, that is, the constructors associated with the datatype.
The following defines two constructors for `Pair`, a `pair` constructor and a `leaf` constructor:
```
{"$event":"define-datatype","$type":"smt","name":"Pair","constructors":[{"name":"pair","children":["Pair","Pair"]},{"name":"leaf","children":["String"]}]}
```
