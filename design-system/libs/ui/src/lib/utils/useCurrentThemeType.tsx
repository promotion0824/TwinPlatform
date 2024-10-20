import { lightTheme } from '@willowinc/theme'
import { useTheme } from 'styled-components'

/**
 * Check current theme is light or dark.
 * This is a temporary solution until we have a better way to determine the current theme.
 */
export const useCurrentThemeType = () => {
  const theme = useTheme()

  return theme.color.neutral.fg.default === lightTheme.color.neutral.fg.default
    ? 'light'
    : 'dark'
}
