# SemGuS-Parser
Grammar and examples for parsing SemGuS specifications.

## The Grammar
The SemGuS grammar described in [Semgus-Lang](https://github.com/SemGuS-git/Semgus-Lang) is implemented in the [Semgus.g4](Semgus.g4) grammar file.
If you find any issues with this implementation, please submit an issue to this repository. A minimally-reproducible SemGuS file that exhibits the issue would be appreciated.

## Testing
This repository contains a crude automated parsing test. Any files in the [tests/](tests/) directory are loaded into Gradle tasks, which attempt to parse them after
the grammar is built. There is no reporting of failures at this time; the output of the tests have to be checked for failure messages. Still, better than nothing!

For checking your own SemGuS files, a convenience task to run the ANTLR GUI test rig is included. Either:
* run: `./check-tree.sh <your-file.sem>` from the repository root, or 
* run the Gradle task directly: `./gradlew checkGuiTree -Pfilename="<your-file.sem>`. 

Installation and setup of the ANTLR libraries happens automatically through Gradle when running these tasks.
