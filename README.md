# SemGuS-Parser
Grammar and examples for parsing SemGuS specifications.

## The Grammar
The SemGuS grammar described in [Semgus-Lang](https://github.com/SemGuS-git/Semgus-Lang) is implemented in the [Semgus.g4](Semgus.g4) grammar file.
If you find any issues with this implementation, please submit an issue to this repository. A minimally-reproducible SemGuS file that exhibits the issue would be appreciated.

## Testing
This repository contains some rudimentary automated testing. Any files in the [src/test/resources/examples/](src/test/resources/examples/)\[[valid](src/test/resources/examples/valid/)|[invalid](src/test/resources/examples/invalid/)] directory 
are loaded into JUnit tests, which attempt to parse them to check for syntactic validity or invalidity.

For checking your own SemGuS files, a convenience task to run the ANTLR GUI test rig is included. Either:
* run: `./check-tree.sh <your-file.sem>` from the repository root, or 
* run the Gradle task directly: `./gradlew checkGuiTree -Pfilename="<your-file.sem>`. 

Installation and setup of the ANTLR libraries happens automatically through Gradle when running these tasks.
