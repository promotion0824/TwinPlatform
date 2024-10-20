const constants = require('./constants')
const _ = require('lodash')
const i18CountryTranslator = require('i18n-iso-countries')
const xlsx = require('xlsx')
const fs = require('fs')
const { parse: parseCsv } = require('csv-parse/sync')
const { checkCol } = constants
const decode = xlsx.utils.decode_range
const encode = xlsx.utils.encode_cell

/* script variables of column numbers */
let categoryCol // Category/Source
let keyCol // P&E key
let engCol // English Labels
let frCol // French Labels

const main = () => {
  const filePath = process.argv.at(-1)
  if (!filePath.toLowerCase().endsWith('.xlsx')) {
    console.error(
      'Usage: node generate-translation-jsons.js <path-to-spreadsheet.xlsx>'
    )
  }

  /* workbook && worksheet variables */
  const wsName = constants.wsName
  const wb = xlsx.readFile(filePath)
  const ws = wb.Sheets[wsName]
  const {
    e: { c: maxCol, r: maxRow },
  } = decode(ws['!ref'])

  /* verify column headers are all found */
  for (let i = 0; i <= maxCol; i++) {
    const { v: cellValue } = ws[encode({ r: 0, c: i })]
    switch (cellValue) {
      case constants.categoryColName:
        categoryCol = i
        break
      case constants.keyColName:
        keyCol = i
        break
      case constants.engColName:
        engCol = i
        break
      case constants.frColName:
        frCol = i
        break
    }
  }
  checkCol({ colName: constants.categoryColName, colValue: categoryCol })
  checkCol({ colName: constants.keyColName, colValue: keyCol })
  checkCol({ colName: constants.engColName, colValue: engCol })
  checkCol({ colName: constants.frColName, colValue: frCol })

  const languages = [
    {
      code: 'en',
      col: engCol,
      modelTranslationsColumnIndex: 1,
    },
    {
      code: 'fr',
      col: frCol,
      modelTranslationsColumnIndex: 2,
    },
  ]

  const assetsRows = parseCsv(
    fs.readFileSync('model-translations.csv'),
    // Strip the byte order mark from the content, so we don't get a weird
    // character before the first model ID in the output.
    { bom: true }
  )

  function getCellValue(rowIndex, colIndex) {
    return ws[encode({ r: rowIndex, c: colIndex })]?.v
  }

  for (const lang of languages) {
    const baseTranslations = {}
    for (let row = 1; row < maxRow + 1; row++) {
      const category = getCellValue(row, categoryCol)
      const key = getCellValue(row, keyCol)
      const value = getCellValue(row, lang.col) ?? getCellValue(row, engCol)

      if (category != null && key != null && value != null) {
        if (baseTranslations[category] == null) {
          baseTranslations[category] = {}
        }
        baseTranslations[category][key] = value
      }
    }

    const countries = Object.fromEntries(
      constants.countries.map((countryName) => {
        let translatedCountryName
        if (lang.code === 'en') {
          // In principle we do not need this special case, but for now it is kept
          // to maintain consistency with the existing behaviour. If we removed it,
          // most of the changes are very minor, eg.
          // "Saint Vincent and The Grenadines" -> "Saint Vincent and the Grenadines"
          // (note uncapitalised "The"), some are more noticeable ("United States" ->
          // "United States of America").
          translatedCountryName = countryName
        } else {
          const countryCode = i18CountryTranslator.getAlpha2Code(
            countryName,
            'en'
          )
          translatedCountryName = i18CountryTranslator.getName(
            countryCode,
            lang.code,
            {
              select: 'official',
            }
          )
        }

        return [_.camelCase(countryName), translatedCountryName]
      })
    )

    for (const category of [...Object.keys(baseTranslations), 'countries']) {
      if (category !== 'interpolation') {
        baseTranslations.interpolation[category] = `$t(${category}.{{key}})`
      }
    }

    for (const category of Object.keys(baseTranslations)) {
      baseTranslations[category] = sortKeys(baseTranslations[category])
    }

    const modelIds = Object.fromEntries(
      assetsRows.map((row) => [row[0], row[lang.modelTranslationsColumnIndex]])
    )

    const fileContents = {
      translation: sortKeys({
        ...baseTranslations,
        countries,
        modelIds,
      }),
    }

    fs.writeFileSync(
      `../../packages/platform/src/public/translations/${lang.code}.json`,
      JSON.stringify(fileContents, null, 2)
    )
  }

  console.log(
    'Please `git add` the translation files in ' +
      'packages/platform/src/public/translations and commit them'
  )
}

function sortKeys(dict) {
  return Object.fromEntries(
    Object.entries(dict).sort((a, b) => a[0].localeCompare(b[0]))
  )
}

main()
