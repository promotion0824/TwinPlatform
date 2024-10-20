Feature: Test DateTime and DateTimeOffset Expressions

Scenario Outline: Test DateTime Expressions
	Given the current time is '2024-04-18 14:24:00Z'
	When I evaluate the expression '<Expression>'
	Then the result should be '<Result>'

Examples:
	| Expression     | Result              |
	| NOW            | 2024-04-18 14:24:00 |
	| NOW + 1d       | 2024-04-19 14:24:00 |
	| NOW + 1day     | 2024-04-19 14:24:00 |
	| NOW + 1days    | 2024-04-19 14:24:00 |
	| NOW - 1d       | 2024-04-17 14:24:00 |
	| NOW - 1day     | 2024-04-17 14:24:00 |
	| NOW - 1days    | 2024-04-17 14:24:00 |
	| NOW + 1h       | 2024-04-18 15:24:00 |
	| NOW + 1hour    | 2024-04-18 15:24:00 |
	| NOW + 1hours   | 2024-04-18 15:24:00 |
	| NOW - 1h       | 2024-04-18 13:24:00 |
	| NOW - 1hour    | 2024-04-18 13:24:00 |
	| NOW - 1hours   | 2024-04-18 13:24:00 |
	| NOW + 1m       | 2024-04-18 14:25:00 |
	| NOW + 1min     | 2024-04-18 14:25:00 |
	| NOW + 1mins    | 2024-04-18 14:25:00 |
	| NOW - 1m       | 2024-04-18 14:23:00 |
	| NOW - 1min     | 2024-04-18 14:23:00 |
	| NOW - 1mins    | 2024-04-18 14:23:00 |
	| NOW + 1s       | 2024-04-18 14:24:01 |
	| NOW + 1sec     | 2024-04-18 14:24:01 |
	| NOW + 1secs    | 2024-04-18 14:24:01 |
	| NOW - 1s       | 2024-04-18 14:23:59 |
	| NOW - 1sec     | 2024-04-18 14:23:59 |
	| NOW - 1secs    | 2024-04-18 14:23:59 |
	| TODAY          | 2024-04-18 00:00:00 |
	| TODAY + 1d     | 2024-04-19 00:00:00 |
	| TODAY + 1day   | 2024-04-19 00:00:00 |
	| TODAY + 1days  | 2024-04-19 00:00:00 |
	| TODAY - 1d     | 2024-04-17 00:00:00 |
	| TODAY - 1day   | 2024-04-17 00:00:00 |
	| TODAY - 1days  | 2024-04-17 00:00:00 |
	| TODAY + 1h     | 2024-04-18 01:00:00 |
	| TODAY + 1hour  | 2024-04-18 01:00:00 |
	| TODAY + 1hours | 2024-04-18 01:00:00 |
	| TODAY - 1h     | 2024-04-17 23:00:00 |
	| TODAY - 1hour  | 2024-04-17 23:00:00 |
	| TODAY - 1hours | 2024-04-17 23:00:00 |
	| TODAY + 1m     | 2024-04-18 00:01:00 |
	| TODAY + 1min   | 2024-04-18 00:01:00 |
	| TODAY + 1mins  | 2024-04-18 00:01:00 |
	| TODAY - 1m     | 2024-04-17 23:59:00 |
	| TODAY - 1min   | 2024-04-17 23:59:00 |
	| TODAY - 1mins  | 2024-04-17 23:59:00 |
	| TODAY + 1s     | 2024-04-18 00:00:01 |
	| TODAY + 1sec   | 2024-04-18 00:00:01 |
	| TODAY + 1secs  | 2024-04-18 00:00:01 |
	| TODAY - 1s     | 2024-04-17 23:59:59 |
	| TODAY - 1sec   | 2024-04-17 23:59:59 |
	| TODAY - 1secs  | 2024-04-17 23:59:59 |
	|                |                     |

Scenario Outline: Test DateTimeOffset Expressions
	Given the current time is '2024-04-18 14:24:00Z'
	When I parse the expression '<Expression>' to a DateTimeOffset
	Then the result should be '<Result>'

Examples:
	| Expression     | Result              |
	| NOW            | 2024-04-18 14:24:00Z |
	| NOW + 1d       | 2024-04-19 14:24:00Z |
	| NOW + 1day     | 2024-04-19 14:24:00Z |
	| NOW + 1days    | 2024-04-19 14:24:00Z |
	| NOW - 1d       | 2024-04-17 14:24:00Z |
	| NOW - 1day     | 2024-04-17 14:24:00Z |
	| NOW - 1days    | 2024-04-17 14:24:00Z |
	| NOW + 1h       | 2024-04-18 15:24:00Z |
	| NOW + 1hour    | 2024-04-18 15:24:00Z |
	| NOW + 1hours   | 2024-04-18 15:24:00Z |
	| NOW - 1h       | 2024-04-18 13:24:00Z |
	| NOW - 1hour    | 2024-04-18 13:24:00Z |
	| NOW - 1hours   | 2024-04-18 13:24:00Z |
	| NOW + 1m       | 2024-04-18 14:25:00Z |
	| NOW + 1min     | 2024-04-18 14:25:00Z |
	| NOW + 1mins    | 2024-04-18 14:25:00Z |
	| NOW - 1m       | 2024-04-18 14:23:00Z |
	| NOW - 1min     | 2024-04-18 14:23:00Z |
	| NOW - 1mins    | 2024-04-18 14:23:00Z |
	| NOW + 1s       | 2024-04-18 14:24:01Z |
	| NOW + 1sec     | 2024-04-18 14:24:01Z |
	| NOW + 1secs    | 2024-04-18 14:24:01Z |
	| NOW - 1s       | 2024-04-18 14:23:59Z |
	| NOW - 1sec     | 2024-04-18 14:23:59Z |
	| NOW - 1secs    | 2024-04-18 14:23:59Z |
	| TODAY          | 2024-04-18 00:00:00Z |
	| TODAY + 1d     | 2024-04-19 00:00:00Z |
	| TODAY + 1day   | 2024-04-19 00:00:00Z |
	| TODAY + 1days  | 2024-04-19 00:00:00Z |
	| TODAY - 1d     | 2024-04-17 00:00:00Z |
	| TODAY - 1day   | 2024-04-17 00:00:00Z |
	| TODAY - 1days  | 2024-04-17 00:00:00Z |
	| TODAY + 1h     | 2024-04-18 01:00:00Z |
	| TODAY + 1hour  | 2024-04-18 01:00:00Z |
	| TODAY + 1hours | 2024-04-18 01:00:00Z |
	| TODAY - 1h     | 2024-04-17 23:00:00Z |
	| TODAY - 1hour  | 2024-04-17 23:00:00Z |
	| TODAY - 1hours | 2024-04-17 23:00:00Z |
	| TODAY + 1m     | 2024-04-18 00:01:00Z |
	| TODAY + 1min   | 2024-04-18 00:01:00Z |
	| TODAY + 1mins  | 2024-04-18 00:01:00Z |
	| TODAY - 1m     | 2024-04-17 23:59:00Z |
	| TODAY - 1min   | 2024-04-17 23:59:00Z |
	| TODAY - 1mins  | 2024-04-17 23:59:00Z |
	| TODAY + 1s     | 2024-04-18 00:00:01Z |
	| TODAY + 1sec   | 2024-04-18 00:00:01Z |
	| TODAY + 1secs  | 2024-04-18 00:00:01Z |
	| TODAY - 1s     | 2024-04-17 23:59:59Z |
	| TODAY - 1sec   | 2024-04-17 23:59:59Z |
	| TODAY - 1secs  | 2024-04-17 23:59:59Z |
	|                |                     |
