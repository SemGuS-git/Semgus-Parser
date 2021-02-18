#!/bin/sh
#
# Quick-n-dirty helper script for running the ANTLR GUI test rig
# Only works from the same directory as the Gradle scripts
#

filename="$1"
./gradlew checkGuiTree -Pfilename="$filename"
