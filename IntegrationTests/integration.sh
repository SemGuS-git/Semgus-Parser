#!/bin/bash
#
# Integration tests for the SemGuS parser
#
ProjectDir=../SemgusParser

# Compile check
echo "Compiling..."
dotnet build "${ProjectDir}"
if [ $? -ne 0 ]; then
    echo "error: compile failed." >&2
    exit 2
fi

# Basic tests with the built-in test string
errors=0

for testrsp in tests/*.rsp; do

    testname="$(basename "$testrsp" .rsp)"
    echo "running: ${testname}"
    dotnet run --project "${ProjectDir}" --no-build -- @"${testrsp}" --output "tests/${testname}.out"
    if [ $? -ne 0 ]; then
	echo "error: parsing failed for test '${testname}'." >&2
	echo " --> failed."
	errors=1
    else
	diff "tests/${testname}.out" "tests/${testname}.txt"
	if [ $? -ne 0 ]; then
	    echo "error: differences found for test '${testname}'." >&2
	    echo " --> failed."
	    errors=1
	else
	    echo " --> passed."
	fi
    fi
    echo " "
done

if [ $errors -ne 0 ]; then
    echo "Integration tests failed."
    exit 1
else
    echo "Integration tests passed."
    exit 0
fi
