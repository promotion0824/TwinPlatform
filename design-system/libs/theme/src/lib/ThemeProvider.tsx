import { ReactNode } from 'react'
import {
  ThemeProvider as StyledThemeProvider,
  useTheme,
} from 'styled-components'
import { Theme, ThemeName, darkTheme, lightTheme } from '../theme'

import GlobalStyles from './GlobalStyles'

declare module 'styled-components' {
  interface DefaultTheme extends Theme {}
}

/**
 * `ThemeProvider` offers the Willow theme along with some global styles
 * for basic elements and scrollbars.
 *
 * If you're using `@willowinc/ui`, use the theme provider exported from that library.
 */
export default function ThemeProvider({
  children,
  includeGlobalStyles = true,
  name = 'dark',
}: {
  children: ReactNode
  includeGlobalStyles?: boolean
  name?: ThemeName
}) {
  return (
    <StyledThemeProvider theme={name === 'dark' ? darkTheme : lightTheme}>
      {includeGlobalStyles && <GlobalStyles />}
      {children}
    </StyledThemeProvider>
  )
}

export { useTheme }
