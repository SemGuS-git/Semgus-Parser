# C# Semgus Parser Library
This directory contains source for the Semgus C# parser library. It is divided up into three sub-projects:
* ParserLibrary - the actual library code. This is what clients will consume.
* ParserExample - an example executable that links against the parser library, for easier testing
* ParserTests - unit tests for the parser code

## MSBuild Configuration
The MSBuild configuration is set up to automatically call Gradle to re-generate the ANTLR files when either build.gradle or Semgus.g4 (in the repo root) changes,
or the generated C# files are not up-to-date.

## Namespaces
The following namespaces are being used:
* Semgus.Parser - for the parser library
* Semgus.Parser.Internal - for the generated ANTLR code. There isn't an option to make the generated members internal, so we do this instead
* Semgus.Parser.Example - for the example executable project
* Semgus.Parser.Tests - for the unit tests

## Other Notes
Note that the Semgus.g4 in this tree is overwritten whenever Gradle rebuilds the grammar - it is just an artifact of the build process.
