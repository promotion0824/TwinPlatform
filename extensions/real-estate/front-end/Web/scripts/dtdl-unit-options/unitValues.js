const _ = require('lodash')
const path = require('path')
const fs = require('fs')

/**
 * Takes an input like this (tab-delimited):

    Semantic type	Unit type	Unit	Symbol
    Acceleration	AccelerationUnit	centimetrePerSecondSquared	cm/s²
    Acceleration	AccelerationUnit	metrePerSecondSquared	m/s²
    Acceleration	AccelerationUnit	gForce	G
    Angle	AngleUnit	degreeOfArc	°
    Angle	AngleUnit	minuteOfArc	'
    Angle	AngleUnit	secondOfArc	"
    ...

 * and returns an object like this:

    {
      "AccelerationUnit": [
        {
          "name": "centimetrePerSecondSquared",
          "displayName": "cm/s²"
        },
        {
          "name": "metrePerSecondSquared",
          "displayName": "m/s²"
        },
        {
          "name": "gForce",
          "displayName": "G"
        }
      ],
      "AngleUnit": [
        {
          "name": "degreeOfArc",
          "displayName": "°"
        },
        {
          "name": "minuteOfArc",
          "displayName": "'"
        },
        {
          "name": "secondOfArc",
          "displayName": "\""
        },
        // ...
      ]
    }

  * The semantic type is ignored, except that we assert that if the same unit type
  * appears under multiple semantic types, the sets of options are the same.
  */
function getOptions(tsv) {
  const rows = tsv
    .replace(/\r/g, '')
    .split('\n')
    .filter((l) => l)
    .slice(1) // Remove the header
    .map((l) => l.split('\t'))

  // {
  //   'Acceleration-AccelerationUnit': [
  //      ["centimetrePerSecondSquared", "cm/s²"],
  //      ["metrePerSecondSquared", "m/s²"],
  //   ],
  //   'Angle-AngleUnit': [
  //      // ...
  //   ],
  //   // ...
  // }
  const semanticAndUnitGroups = _.mapValues(
    _.groupBy(rows, (r) => `${r[0]}-${r[1]}`),
    (rows) => rows.map((r) => r.slice(2))
  )

  // {
  //   "AccelerationUnit": {
  //     "Acceleration-AccelerationUnit": [
  //        ["centimetrePerSecondSquared", "cm/s²"],
  //        ["metrePerSecondSquared", "m/s²"],
  //      ]
  //   },
  //   "AngleUnit": {
  //     // For each group (like AngleUnit), we want to make sure
  //     // all the sets of options in the groups are the same.
  //
  //     "Angle-AngleUnit": [
  //       // ...
  //     ],
  //     "Latitude-AngleUnit": [
  //       // ...
  //     ],
  //   }
  // }
  const unitGroups = _.mapValues(
    _.groupBy(
      Object.entries(semanticAndUnitGroups),
      ([key]) => key.split('-')[1]
    ),
    Object.fromEntries
  )

  validate(unitGroups)

  return Object.fromEntries(
    Object.entries(unitGroups).map(([group, entries]) => {
      const rows = Object.values(entries)[0]
      return [group, rows.map((r) => ({ name: r[0], displayName: r[1] }))]
    })
  )
}

/**
 * Where we have the same unit type under multiple semantic types,
 * eg. Distance/LengthUnit and Length/LengthUnit, make sure that
 * the sets of options in each semantic type are the same. Otherwise,
 * throw an exception.
 */
function validate(unitGroups) {
  const mismatches = []
  for (const innerGroups of Object.values(unitGroups)) {
    const sets = Object.entries(innerGroups)
    const [firstSetName, firstSetItems] = sets[0]
    for (const [setName, setItems] of sets) {
      if (!_.isEqual(setItems, firstSetItems)) {
        mismatches.push([setName, firstSetName])
      }
    }
  }

  if (mismatches.length > 0) {
    throw new Error(`Mismatches: ${JSON.stringify(mismatches)}`)
  }
}

function main() {
  const tsv = fs.readFileSync('units.tsv', { encoding: 'utf-8' })
  const unitValsPath = '../../packages/common/src/twins/view/unitVals.json'
  fs.writeFileSync(unitValsPath, JSON.stringify(getOptions(tsv), null, 2), {
    encoding: 'utf-8',
  })
  console.log(`Wrote ${path.resolve(unitValsPath)}`)
}

main()
