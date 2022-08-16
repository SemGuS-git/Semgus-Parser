#!/bin/sh

for file in tests/*.out; do
   mv "$file" "$(dirname "$file")/$(basename $file .out).txt"
done
