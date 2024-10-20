import { registerTransforms } from '@tokens-studio/sd-transforms'
import { copyFileSync, mkdirSync, writeFileSync } from 'fs'
import StyleDictionary from 'style-dictionary'
import { darkThemePalette, lightThemePalette } from '@willowinc/palette'

registerTransforms(StyleDictionary)

StyleDictionary.registerTransform({
  name: 'text-case-to-text-transform',
  type: 'value',
  transitive: true,
  matcher: (token) => token['type'] === 'typography' && token.value.textCase,
  transformer: (token) => {
    const { textCase, ...value } = token.value
    return {
      ...value,
      textTransform: textCase,
    }
  },
})

function copyToDir(dir: string, relativeFilePath: string) {
  copyFileSync(
    `${__dirname}/${relativeFilePath}`,
    `${dir}/${relativeFilePath.split('/').at(-1)}`
  )
}

function buildTheme(themeName: string, themePalette: typeof darkThemePalette) {
  const workingDir = `${__dirname}/tmp/${themeName}`

  mkdirSync(workingDir, { recursive: true })

  copyToDir(workingDir, 'tokens/base.json')
  copyToDir(workingDir, 'tokens/global.json')
  copyToDir(workingDir, `tokens/${themeName}ThemeTokens.json`)

  writeFileSync(
    `${workingDir}/${themeName}ThemePalette.json`,
    JSON.stringify(themePalette, null, 2)
  )

  const themeFilters = [
    'fontFamily',
    'fontSize',
    'fontWeight',
    'leonardo',
    'lineHeight',
    'textCase',
    'textDecoration',
  ]

  StyleDictionary.extend({
    source: [`${workingDir}/*.json`],
    platforms: {
      json: {
        transforms: ['text-case-to-text-transform', 'ts/shadow/css/shorthand'],
        files: [
          {
            destination: `${__dirname}/dist/${themeName}Theme.json`,
            format: 'json/nested',
            filter: (token) => !themeFilters.includes(token.path[0]),
          },
        ],
      },
    },
  }).buildAllPlatforms()
}

buildTheme('dark', darkThemePalette)
buildTheme('light', lightThemePalette)
