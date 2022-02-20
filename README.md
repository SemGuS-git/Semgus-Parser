# SemGuS-Parser
A C# parsing library for SemGuS problems.

# JSON Converter
The `SemgusParser` project contains a utility that reads in a SemGuS file and produces JSON data representing
the problem, usable by other non-.NET tools that cannot directly link with this library.

## Usage

```
SemgusParser [--format json] [--mode stream|batch] [--output <filename.json>] -- <input.sl> ...
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
#### `set-info`
Sent at the end of the event stream.
```
{ "$event": "end-of-stream", "$type": "meta" }
```
