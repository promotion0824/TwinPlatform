import { baseTheme } from '@willowinc/theme'

export type ThemeSpacing = keyof typeof baseTheme.spacing

// Just replicate how Mantine typing it
// which will have spacing token prompt in vscode code IntelliSense
// eslint-disable-next-line @typescript-eslint/ban-types
export type SpacingValue = ThemeSpacing | (string & {}) | number

export function isThemeSpacing(value: SpacingValue): value is ThemeSpacing {
  return !!value && Object.keys(baseTheme.spacing).includes(value.toString())
}

export const getSpacing = (
  value?: SpacingValue
): string | number | undefined => {
  if (value === undefined) return undefined
  return isThemeSpacing(value) ? baseTheme.spacing[value] : value
}
