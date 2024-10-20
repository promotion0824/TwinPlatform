# Willow Expression Unit Tests

These can be split into the following categories corresponding to the stages of parsing and expression tree manipulation:

1. Scanner testing
The scanner is quite trivial, we haven't bothered writing any tests for it.

2. Parser testing
This is an area we need plenty of tests to ensure that everything continues to work as we update the expression language.
Tests are needed for each part of the language: numbers, identifiers, funtion calls, numeric expressions, comparison expressions,
logical expressions. But it's quite a lot of work to construct a result expression for each and then compare it so we
cheat a bit and simply check that the serialized form of the expression is the same string (modulo spacing and parentheses)
as compared to the input string.

3. Expression visitor testing
We need to create some sample visitors deriving from the base visitor class and exercise them to ensure we can
visit the expression tree effectively to create new trees and other non-tree-like output like the UnboundVariables visitor.

4. Testing any new language feature we add
We want to add an OPTION() function to the language. This takes any number of parameters and matches the first one that
can be bound. An Expression Visitor for binding (to be written) will take an Environment and will attempt to bind an expression to it.

For example: `OPTION("dtmi:com:willowinc:OccupancySensor;1", "occ sensor", "ChargiFi|Occupied") > 0`

Environment: `{ "occ sensor" : "<guid>" }`

Result, a new expression: `<guid> > 0`