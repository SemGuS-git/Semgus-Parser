# Integration Tests
Super simple integration tests.

## Parsing tests
Each test consists of two files in the `tests/` directory:

* A response file `<testname>.rsp`, which specifies the command line parameters (one on each line) to use for the test
* An expected output file, `<testname>.txt`, which specifies what the parser's output should be

The integration test script runs the parser with the given command line (specifying an output file of `<testname>.out`)
and then diffs `<testname>.txt` with `<testname>.out`. If no differences are reported, then the test passes. Otherwise, it fails.