import { writeFileSync } from 'node:fs'
import StyleDictionary from 'style-dictionary'
import config from './config/palette.json'
import { generatePalette, PaletteTokens } from './lib/palette'

function savePalette(themeName: string) {
  StyleDictionary.extend({
    source: [`${__dirname}/tokens/${themeName}Tokens.json`],
    platforms: {
      json: {
        files: [
          {
            destination: `${__dirname}/palettes/${themeName}Palette.json`,
            format: 'json/nested',
          },
        ],
      },
    },
  }).buildAllPlatforms()
}

function saveTokens(paletteTokens: PaletteTokens, themeName: string) {
  writeFileSync(
    `${__dirname}/tokens/${themeName}Tokens.json`,
    JSON.stringify(paletteTokens, null, 2)
  )
}

const darkThemePalette = generatePalette(config.lightness.dark)
const lightThemePalette = generatePalette(config.lightness.light)

saveTokens(darkThemePalette, 'darkTheme')
saveTokens(lightThemePalette, 'lightTheme')

savePalette('darkTheme')
savePalette('lightTheme')
