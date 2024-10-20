Willow Expressions
=====

This package contains the Willow Expression language and Parser plus various Visitors.

You can think of them like formulae in an Excel sheet except that (a) they can specify a class of entity not just an individual cell; and (b) each formula can only build on the formulae before it and therefore circular references are not allowed. The syntax for expressions is defined below:

Variables
===

Variables - Local references
---
Variables are references to an earlier cell in the rule, to a named global variable or to a specific capability value (aka pointEntityId aka trendId) on the selected equipment item for the rule. Matching can happen using model Ids, haystack tags, partial BACNET names or specific twin Ids.

If the variable matches multiple capabilities the rule will currently fail (aggregations are coming soon). e.g. if an AHU has three zones and each has a return air temperature sensor.

Examples:

Variable format

Explanation

temp_sp

A variable defined higher up in the rule. Square parentheses are optional when the name is a single identifier with no spaces.

[air temperature]

Matches a capability with the tags ‘air’ and ‘temperature’ found on the Equipment item.

[dtmi:com:willowinc:SupplyFan;1]

 Matches a capability with the given model Id found on the Equipment item.

[-AHU-DAT]

Matches a suffix of the BACNET name (twin ID) for a capability/trend found on the Equipment item.

[MS-CR-TU-ElecEnergySensor]

Matches a specific capability twin anywhere in the twin graph

Variables - Absolute References
---

Variables can also refer directly to any twin or capability anywhere in the graph.

Variable format

Explanation

[MS-CR-TU-ElecEnergySensor]

Matches a specific twin anywhere in the twin graph. If this twin is a capability you can use it directly, if it’s an equipment item you can use dot notation to access its capabilities to use in an expression. An equipment twin alone is not allowed as a variable as it has no numeric value.

Properties (dot notation)
---
Using [A].[B] you can refer to a static property of a twin, e.g. maxAirFlow or a nested static property, e.g. returnFan.maxAirFlow.

You can also refer to a capability off an equipment item.

Variable format

Explanation

[DDK-Floor1].[TotalEnergySensor]

Matches the energy sensor off an absolute twin reference.

[AirHandlingUnit;1].[ZoneAirTemperatureSensor;1]

Matches the zone air temperature sensor off the air handling unit found in the system graph around the current node.

[AirHandlingUnit;1].ratedCFM

Takes on the static numeric value from the property of the equipment.

[AirHandlingUnit;1].returnFan.ratedCFM

Take on the static numeric value from the complex property of the equipment.

Note that all property references are computed a binding time not run time. If you change the property value in the twin you will need to rebuild the rules.

Functions
---

OPTION function
---

The special OPTION(arg1, arg2, …) function matches the first argument inside it that can be bound to the twin. This allows a rule to bind to, for example, either a dtmi model Id OR a haystack tag. But it’s even more powerful than that because the arg itself can be an expression and it will recursively explore each argument to find the one where all the parts can be satisfied. For example you can write:   OPTION( [on], [speed] > 0.1)  and it will bind to either a capability ‘on’ or a capability ‘speed’ and if it’s a speed it will use the expression speed > 0.1 to determine that the equipment is on.

Another example would be OPTION([AHU].ratedCFM, 900) to lookup a property of a twin with a fallback to a constant value if it is not found.

A third use of the OPTION function is to bind by the most specific model type first and then to fall back to a more general model type if it’s not found. This is useful for dealing with older Twins where the more precise model types were not initially available. e.g. OPTION[ZoneAirHeatingTemperatureSetpoint;1], [ZoneAirTemperatureSetpoint;1], [AirTemperatureSetpoint;1])

IF (condition, value_true, value_false) : the ternary operator
---

Not to be confused with the OPTION function which works at binding time, the IF function operates at run time. Everything inside it must be a valid time series or constant value.

    IF([AHU].[Occupied], 2, 5)

If the AHU occupied sensor is set, use the value 2, if not, use the value 5.

    IF([AHU].[heating on], [AHU].[HeatingSetpoint;1], [AHU].[CoolingSetpoint;1])

If the heating is on use the heating setpoint live value, otherwise use the live cooling value.

FAHRENHEIT() and CELSIUS()
----

These two special functions can be applied to a capability that has a units field with degF or degC (or some of the other variants found in twins). If the capability has the same units as the function name then it is passed through, otherwise it is converted. This means you can write CELSIUS([room temp]) > 22 and it will do the right thing whether the capability is measured in celsius or Fahrenheit.

We may add more conversion operators in the future but first we need to make sure everything has that units field filled in!



CONTAINS(arg, filter_value), STARTSWITH(arg, filter_value), ENDSWITH(arg, filter_value) : Filter Expressions
---

These expressions operate on twin properties and can perform the usual string matching. The can be used in filters [TODO: link] to select which rule instances to exclude.

````
    CONTAINS(this.name, “V1”)
````

Include if the referenced name includes “V1”

!CONTAINS(this.Id, “-N12-”)

Exclude if the referenced iD includes “-N12-”

STARTSWITH(this.name, “CHW”)

Include if the referenced name starts with “CHW”



Distributive Expressions
----
Often rules may apply to models that may reference multiple instances. For example, on the Willow Rules Engine: AHU Simultaneous Heating and Cooling rule [dtmi:com:willowinc:ChilledWaterValuePositionSensor;1] is referenced in a rule, however some AHU instances have relationships to multiple sensors. Distributive expressions allow rules to seamlessly handle unexpected or expected arrays. Continuing the example above, the expression ([ChwValvePosition] > 0) now distributives across each ChwValuePosition found linked to the rule bound AHU instance. This becomes an array of expressions ex. {([DFWAA-DFWA-AHU-2-3-ChilledWaterValveACmd-110AO3004165] > 0),([DFWAA-DFWA-AHU-2-3-ChilledWaterValveBCmd-110AO3004166] > 0)}. This array can be wrapped with any of the aggregation functions below.

`ChillersOperational = ANY([ChwValvePosition] > 0)`

When bound to an AHU with multiple associated ChwValuePosition, the expression will become ANY({([DFWAA-DFWA-AHU-2-3-ChilledWaterValveACmd-110AO3004165] > 0),([DFWAA-DFWA-AHU-2-3-ChilledWaterValveBCmd-110AO3004166] > 0)}

SUM(), COUNT(), AVERAGE(), MIN(), MAX(), ANY(), ALL(): Aggregation Functions
----
Aggregate values from multiple capabilities using SUM, COUNT, AVERAGE, MAX, MIN, ANY and ALL.


SUM([Chiller].[CapacitySensor])
----
If multiple chillers and capacity sensor values are bound to within the rule instance, the sum will return total value.

COUNT([ChwValvePosition] > 0)

Returns the number of open valves.

AVERAGE([AHU].ratedCFM)

Calculate the average rated CFM across a range of air handlers.



Time Functions:
----
HOUR(NOW)

Description: Returns the current hour of the day.

Example: HOUR(NOW) returns the current hour as an integer, such as 15 for 3:00 PM.

MINUTE(NOW)

Description: Returns the current minute of the hour.

Example: MINUTE(NOW) returns the current minute as an integer, such as 30 for 3:30 PM.

DAY(NOW)

Description: Returns the current day of the month.

Example: DAY(NOW) returns the current day as an integer, such as 15 for August 15th.

DAYOFWEEK(NOW)

Description: Returns the day of the week as an integer (1-7, where 1 represents Sunday).

Example: DAYOFWEEK(NOW) returns the current day of the week as an integer, such as 4 for Thursday.

MONTH(NOW)

Description: Returns the current month of the year.

Example: MONTH(NOW) returns the current month as an integer, such as 8 for August.

Additionally, these functions also support a more C#-like syntax, allowing you to access time components directly from the object. For example:

NOW.Hour returns the current hour using the C#-like syntax.

NOW.Minute returns the current minute.

NOW.Day returns the current day.

NOW.DayOfWeek returns the current day of the week.

NOW.Month returns the current month.

These functions can be used for various calculations and decision-making processes. Here are a couple of usage examples:

Seasonal Adjustment:

`seasonal_adjustment = IF(MONTH(NOW) > 4 & MONTH(NOW) < 9, 0.8, 0.2)`

In this example, seasonal_adjustment is calculated based on the current month. If the month is between May and August (inclusive), the adjustment factor is set to 0.8; otherwise, it's set to 0.2.

Dynamic Setting Based on Day of Week:

`setting = IF(DAYOFWEEK(NOW) > 5, weekend_setting, weekday_setting)`
This snippet calculates the setting value based on the current day of the week. If the current day is a weekend day (Saturday or Sunday), the weekend_setting value is used; otherwise, the weekday_setting value is used.

EACH( ) Function: Perform calculations across multiple model instances.
----
EACH Function - Rules Engine - Confluence (atlassian.net)

Mathematical Operators
----
All the usual mathematical operators are supported: + - * /

Comparison Operators
----
All the usual comparison operators are supported: == >= <= < > !=

Logical Operators
----
All the usual logical operators are supported: AND OR NOT,
eg. (expr1 OR expr2)

C# MATH FUNCTIONS
----
All the usual C# Math functions are supported. For example, this is a valid expression to calculate the wet bulb globe temperature (a measure of comfort) for a room:

T * arctan(0.151977 * (rh + 8.313659)^(1/2)) + arctan(T + rh) - arctan(rh - 1.676331) + 0.00391838 *(rh)^(3/2) * arctan(0.023101 * rh) - 4.686035"


Math Functions Supported:
----
ABS(value) -> Double: Calculates the absolute value of a number. Example: ABS(-5) returns 5.

ACOS(value) -> Double: Calculates the arc cosine (in radians) of a number. Example: ACOS(0.5) returns 1.047197551.

ASIN(value) -> Double: Calculates the arc sine (in radians) of a number. Example: ASIN(0.5) returns 0.523598776.

ATAN(value) -> Double: Calculates the arc tangent (in radians) of a number. Example: ATAN(1) returns 0.785398163.

ATAN2(y, x) -> Double: Calculates the angle (in radians) from the X axis to a point represented by the coordinates (x, y). Example: ATAN2(1, 1) returns 0.785398163.

CEILING(value) -> Double: Rounds a number up to the nearest integer. Example: CEILING(4.3) returns 5.

COS(value) -> Double: Calculates the cosine of an angle (in radians). Example: COS(0) returns 1.

FLOOR(value) -> Double: Rounds a number down to the nearest integer. Example: FLOOR(4.7) returns 4.

LOG(value) -> Double: Calculates the natural logarithm (base e) of a number. Example: LOG(2.718281828) returns 1.

LOG10(value) -> Double: Calculates the base-10 logarithm of a number. Example: LOG10(100) returns 2.

ROUND(value) -> Double: Rounds a number to the nearest integer. Example: ROUND(4.3) returns 4.

SIGN(value) -> Double: Returns the sign of a number (1 for positive numbers, -1 for negative numbers, and 0 for zero). Example: SIGN(-5) returns -1.

SIN(value) -> Double: Calculates the sine of an angle (in radians). Example: SIN(0) returns 0.

SQRT(value) -> Double: Calculates the square root of a number. Example: SQRT(16) returns 4.

TAN(value) -> Double: Calculates the tangent of an angle (in radians). Example: TAN(0) returns 0.

POW(base, exponent) -> Double: Calculates the result of raising a number to a specified power. Example: POW(2, 3) returns 8.

Extensibility
----
We can add new functions easily to the parser and evaluation code.

Execution
----
Willow expression can be executed in an interpreted fashion or they can be compiled to C# Expressions. They can also be converted to SQL, MongoDB and English but we aren’t using any of those capabilities.
