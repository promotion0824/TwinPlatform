using System;
using System.Collections.Generic;
using System.Linq;

namespace Willow.Rules.Model
{
    public class TagSet
    {
        internal static IEnumerable<string> cleanTag(string tag)
        {
            if (replacements.TryGetValue(tag, out string? replacement))
            {
                foreach (var t in replacement!.Split(' '))
                {
                    yield return t;
                }
            }
            else
            {
                yield return tag;
            }
        }

        internal static int getGroup(string tag)
        {
            int c = 0;
            foreach (var tagSet in tagSets)
            {
                if (tagSet.Contains(tag, StringComparer.OrdinalIgnoreCase)) return c;
                c++;
            }
            return c;
        }

        private static readonly string positionTags = @"
condensate
discharge
entering
leaving
exhaust
inlet
input
intake
mixed
out
outlet
output
outside
overflow
return
source
supply
waterside";

        private static readonly string describesSubstance = @"
AC
DC
dirty
diesel
emergency
heat
chilled
cool
coolant
cooling
gasoline
heating
hot
warm
humidifying
preheat
reheat
";

        private static readonly string substance = @"
electricity
fuel
air
gas
hydrogen
oil
smoke
steam
urea
water
co
";

        private static readonly string describesEquipment = @"
interlock
purge
reheat
warmup
manual
reject
locked
out
";
        // locked out = "locked" "out"


        private static readonly string equipment = @"
absorber
coil
compressor
condenser
damper
door
drive
ejector
elevator
exchanger
fan
filter
filtration
fire
freeze
fuse
gear
generator
greaseInterceptor
heatExchanger
inverter
manifold
modbus
panel
panelboard
plenum
plumbing
pump
sewage
solenoid
strainer
suction
switch
tank
turbine
valve
vent
vfd
waterHeater
";

        private static readonly string describesMeasure = @"
acceptable
instantaneous
static
thermal
total
wetbulb
thermal
elec
multiplier
";

        private static readonly string measure = @"
co2
energy
humidity
malfunction
methane
MovingDirection
LastError
phase
position
power
pressure
pressurization
pulse
run
runhours
speed
tamper
temp
time
toggle
torque
tvoc
velocity
vibration
voc
voltage
current
";

        private static readonly string units = @"
celsius
percentage
";

        private static readonly string value = @"
count
counter
connectState
day
empty
enabled
fail
failed
failure
fault
halted
hour
LastError
locked
mode
off
offline
on
online
open
OperationMode
overload
reliability
stopped
stopping
tripped
unoccupied
unreliable
year
";

        // db = deadband

        private static readonly string describesCapability = @"
occ
offset
db
reset
setback
flow
enable
acceptable
";

        private static readonly string capabilityType = @"
sensor
sp
cmd
";

        // optional match if present, OK if missing
        private static readonly string qualifier = @"
master
1
2
3
4
5
6
7
8
9
A
B
C
zone
night
morning
signal
db
primaryLoop
secondaryLoop
tertiaryLoop
basement
3-1
1-2
2-3
ab
bc
";

        /// <summary>
        /// Typos and synonyms
        /// </summary>
        public static Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            ["afms"] = "air flow afms",
            ["fan1"] = "fan 1",
            ["fan2"] = "fan 2",
            ["fan3"] = "fan 3",
            ["fan4"] = "fan 4",
            ["aparent"] = "apparent",
            ["coolan"] = "coolant",
            ["damer"] = "damper",
            ["dicharge"] = "discharge",
            ["disharge"] = "discharge",
            ["heat"] = "heating",
            ["exhcanger"] = "exchanger",
            ["economizer"] = "economy",
            ["frequency"] = "freq",
            ["fanSpeed"] = "fan speed",
            ["occupancy"] = "occ",
            ["occupied"] = "occ",
            ["PassangerAlarm"] = "passenger alarm",
            ["per"] = "pecentage",
            ["PowerKW"] = "power KW",
            ["pressurize"] = "pressurization",
            ["rejection"] = "reject",
            ["retunr"] = "return",
            ["runHours"] = "runhours",
            ["seconadryLoop"] = "secondaryLoop",
            ["soruce"] = "source",
            ["Speed"] = "speed",
            ["temperature"] = "temp",
            ["unocc"] = "unoccupied",
            ["volt"] = "voltage",
            ["waring"] = "warning",
            ["outsie"] = "outside",
            ["outsie"] = "outside",
            // Elevators appear to have a lot of unique values
            ["PostionCM"] = "position cm",
            ["BuildingPosition"] = "building position",
        };

        // elec total sensor reactive
        private static string bugFixes(string canonicaltags)
        {
            if (canonicaltags == "elec total sensor reactive") return "elec total power sensor reactive";
            if (canonicaltags == "elec total power sensor") return "elec total power sensor active";
            return canonicaltags;
        }


        public static string ignore = @"
ac1-2
ac-2-1
acb1-1
acb1-2
and
BoardingCallAllocationsSide1
BoardingCallAllocationsSide2
c
c1
c2
Car1
CommandDestinationCall
CommandLockBoardingsSide1
CommandLockBoardingsSide2
CommandLockDestinationesSide1
CommandLockDestinationesSide2
CommandOperationMode
DestinationCallsSide1
DestinationCallsSide2
EF0406A
EF0406B
EF0407
EF05
HV4-1A
HV68-1
HV68-2
HV68-3
identification
import
janitor
kitchen
L12
L15
L18
L21
L24
L27
L28
L31
L34
L37
L40
L43
L46
L49
L52
L53
L56
L59
L6
L62
L65
L9
lobby
LockedBoardingsSide1
LockedBoardingsSide2
LockedDestnationesSide1
LockedDestnationesSide2
mamoth
mens
miscelleneous
MovingDirection
now
of
office
or
paragon
r
RSF05
ServedBuildingFloorsSide1
ServedBuildingFloorsSide2
ss1
ss2
ss3
ss4
ss5
ss6
to
toilet
vlan01
west
ALL-GENDER RESTROOM WITH SHOWER
ANTE SPACE/COFFEE POINT
ALL-GENDER RESTROOM
ABLUTION RM
ADA PHONE
ACCESSIBILITY CENTER
CORRIDOR
COPY RM

";

        public static string[] Ignores = split(ignore);

        private static char[] splits = new[] { '\r', '\n' };

        private static string[] split(string s) => s.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        private static string[][] tagSets = new[]
        {
            split(substance),
            split(describesSubstance),
            split(positionTags),
            split(describesEquipment),
            split(equipment),
            split(describesMeasure),
            split(measure),
            split(units),
            split(value),
            split(describesCapability),
            split(capabilityType),
            split(qualifier)
        };
    }
}