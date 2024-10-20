Feature: Test DateTime Expressions Value Retriever

Scenario Outline: Test DateTime Expressions Value Retriever
	Given the current time is '2024-04-18 14:24:00Z'
	And I have test objects
		| CurrentDateTime |
		| NOW             |
		| NOW + 1d        |
		| NOW + 1day      |
		| NOW + 1days     |
		| NOW - 1d        |
		| NOW - 1day      |
		| NOW - 1days     |
		| NOW + 1h        |
		| NOW + 1hour     |
		| NOW + 1hours    |
		| NOW - 1h        |
		| NOW - 1hour     |
		| NOW - 1hours    |
		| NOW + 1m        |
		| NOW + 1min      |
		| NOW + 1mins     |
		| NOW - 1m        |
		| NOW - 1min      |
		| NOW - 1mins     |
		| NOW + 1s        |
		| NOW + 1sec      |
		| NOW + 1secs     |
		| NOW - 1s        |
		| NOW - 1sec      |
		| NOW - 1secs     |
		| TODAY           |
		| TODAY + 1d      |
		| TODAY + 1day    |
		| TODAY + 1days   |
		| TODAY - 1d      |
		| TODAY - 1day    |
		| TODAY - 1days   |
		| TODAY + 1h      |
		| TODAY + 1hour   |
		| TODAY + 1hours  |
		| TODAY - 1h      |
		| TODAY - 1hour   |
		| TODAY - 1hours  |
		| TODAY + 1m      |
		| TODAY + 1min    |
		| TODAY + 1mins   |
		| TODAY - 1m      |
		| TODAY - 1min    |
		| TODAY - 1mins   |
		| TODAY + 1s      |
		| TODAY + 1sec    |
		| TODAY + 1secs   |
		| TODAY - 1s      |
		| TODAY - 1sec    |
		| TODAY - 1secs   |
		|                 |
	Then the objects' CurrentDateTime should be
		| CurrentDateTime     |
		| 2024-04-18 14:24:00 |
		| 2024-04-19 14:24:00 |
		| 2024-04-19 14:24:00 |
		| 2024-04-19 14:24:00 |
		| 2024-04-17 14:24:00 |
		| 2024-04-17 14:24:00 |
		| 2024-04-17 14:24:00 |
		| 2024-04-18 15:24:00 |
		| 2024-04-18 15:24:00 |
		| 2024-04-18 15:24:00 |
		| 2024-04-18 13:24:00 |
		| 2024-04-18 13:24:00 |
		| 2024-04-18 13:24:00 |
		| 2024-04-18 14:25:00 |
		| 2024-04-18 14:25:00 |
		| 2024-04-18 14:25:00 |
		| 2024-04-18 14:23:00 |
		| 2024-04-18 14:23:00 |
		| 2024-04-18 14:23:00 |
		| 2024-04-18 14:24:01 |
		| 2024-04-18 14:24:01 |
		| 2024-04-18 14:24:01 |
		| 2024-04-18 14:23:59 |
		| 2024-04-18 14:23:59 |
		| 2024-04-18 14:23:59 |
		| 2024-04-18 00:00:00 |
		| 2024-04-19 00:00:00 |
		| 2024-04-19 00:00:00 |
		| 2024-04-19 00:00:00 |
		| 2024-04-17 00:00:00 |
		| 2024-04-17 00:00:00 |
		| 2024-04-17 00:00:00 |
		| 2024-04-18 01:00:00 |
		| 2024-04-18 01:00:00 |
		| 2024-04-18 01:00:00 |
		| 2024-04-17 23:00:00 |
		| 2024-04-17 23:00:00 |
		| 2024-04-17 23:00:00 |
		| 2024-04-18 00:01:00 |
		| 2024-04-18 00:01:00 |
		| 2024-04-18 00:01:00 |
		| 2024-04-17 23:59:00 |
		| 2024-04-17 23:59:00 |
		| 2024-04-17 23:59:00 |
		| 2024-04-18 00:00:01 |
		| 2024-04-18 00:00:01 |
		| 2024-04-18 00:00:01 |
		| 2024-04-17 23:59:59 |
		| 2024-04-17 23:59:59 |
		| 2024-04-17 23:59:59 |
		|                     |

Scenario Outline: Test DateTimeOffset Expressions Value Retriever
	Given the current time is '2024-04-18 14:24:00Z'
	And I have test offset objects
		| CurrentDateTime |
		| NOW             |
		| NOW + 1d        |
		| NOW + 1day      |
		| NOW + 1days     |
		| NOW - 1d        |
		| NOW - 1day      |
		| NOW - 1days     |
		| NOW + 1h        |
		| NOW + 1hour     |
		| NOW + 1hours    |
		| NOW - 1h        |
		| NOW - 1hour     |
		| NOW - 1hours    |
		| NOW + 1m        |
		| NOW + 1min      |
		| NOW + 1mins     |
		| NOW - 1m        |
		| NOW - 1min      |
		| NOW - 1mins     |
		| NOW + 1s        |
		| NOW + 1sec      |
		| NOW + 1secs     |
		| NOW - 1s        |
		| NOW - 1sec      |
		| NOW - 1secs     |
		| TODAY           |
		| TODAY + 1d      |
		| TODAY + 1day    |
		| TODAY + 1days   |
		| TODAY - 1d      |
		| TODAY - 1day    |
		| TODAY - 1days   |
		| TODAY + 1h      |
		| TODAY + 1hour   |
		| TODAY + 1hours  |
		| TODAY - 1h      |
		| TODAY - 1hour   |
		| TODAY - 1hours  |
		| TODAY + 1m      |
		| TODAY + 1min    |
		| TODAY + 1mins   |
		| TODAY - 1m      |
		| TODAY - 1min    |
		| TODAY - 1mins   |
		| TODAY + 1s      |
		| TODAY + 1sec    |
		| TODAY + 1secs   |
		| TODAY - 1s      |
		| TODAY - 1sec    |
		| TODAY - 1secs   |
		|                 |
	Then the offset objects' CurrentDateTime should be
		| CurrentDateTime      |
		| 2024-04-18 14:24:00Z |
		| 2024-04-19 14:24:00Z |
		| 2024-04-19 14:24:00Z |
		| 2024-04-19 14:24:00Z |
		| 2024-04-17 14:24:00Z |
		| 2024-04-17 14:24:00Z |
		| 2024-04-17 14:24:00Z |
		| 2024-04-18 15:24:00Z |
		| 2024-04-18 15:24:00Z |
		| 2024-04-18 15:24:00Z |
		| 2024-04-18 13:24:00Z |
		| 2024-04-18 13:24:00Z |
		| 2024-04-18 13:24:00Z |
		| 2024-04-18 14:25:00Z |
		| 2024-04-18 14:25:00Z |
		| 2024-04-18 14:25:00Z |
		| 2024-04-18 14:23:00Z |
		| 2024-04-18 14:23:00Z |
		| 2024-04-18 14:23:00Z |
		| 2024-04-18 14:24:01Z |
		| 2024-04-18 14:24:01Z |
		| 2024-04-18 14:24:01Z |
		| 2024-04-18 14:23:59Z |
		| 2024-04-18 14:23:59Z |
		| 2024-04-18 14:23:59Z |
		| 2024-04-18 00:00:00Z |
		| 2024-04-19 00:00:00Z |
		| 2024-04-19 00:00:00Z |
		| 2024-04-19 00:00:00Z |
		| 2024-04-17 00:00:00Z |
		| 2024-04-17 00:00:00Z |
		| 2024-04-17 00:00:00Z |
		| 2024-04-18 01:00:00Z |
		| 2024-04-18 01:00:00Z |
		| 2024-04-18 01:00:00Z |
		| 2024-04-17 23:00:00Z |
		| 2024-04-17 23:00:00Z |
		| 2024-04-17 23:00:00Z |
		| 2024-04-18 00:01:00Z |
		| 2024-04-18 00:01:00Z |
		| 2024-04-18 00:01:00Z |
		| 2024-04-17 23:59:00Z |
		| 2024-04-17 23:59:00Z |
		| 2024-04-17 23:59:00Z |
		| 2024-04-18 00:00:01Z |
		| 2024-04-18 00:00:01Z |
		| 2024-04-18 00:00:01Z |
		| 2024-04-17 23:59:59Z |
		| 2024-04-17 23:59:59Z |
		| 2024-04-17 23:59:59Z |
		|                      |
