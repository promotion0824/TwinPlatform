import darkThemeJSON from './dist/darkTheme.json'
import lightThemeJSON from './dist/lightTheme.json'
import { StyledObject } from 'styled-components'

const { color, shadow, ...baseTheme } = darkThemeJSON

type GenericTheme = typeof darkThemeJSON
export interface Theme extends GenericTheme {
  // TextTransform properties are really specific, and their types needs to match Style Dictionary.
  font: GenericTheme['font'] & {
    heading: {
      group: {
        textTransform: StyledObject<object>['textTransform']
      }
    }
  }
}

const isTheme = (theme: GenericTheme): theme is Theme => {
  return theme.font.heading.group.textTransform === 'uppercase'
}

const darkThemeIsValid = isTheme(darkThemeJSON)
const lightThemeIsValid = isTheme(lightThemeJSON)

if (!darkThemeIsValid) throw new Error('Dark theme is invalid.')
if (!lightThemeIsValid) throw new Error('Light theme is invalid.')

const darkTheme = darkThemeJSON as Theme
const lightTheme = lightThemeJSON as Theme

export { baseTheme, darkTheme, lightTheme }
export type ThemeName = 'dark' | 'light'
