import { PropsWithChildren } from 'react'
import { createGlobalStyle } from 'styled-components'
import { ThemeProvider as WillowThemeProvider, ThemeName } from '@willowinc/ui'
import flattenObject from './flattenObject'

export const GlobalCSSVariables = createGlobalStyle(({ theme }) => ({
  ':root': flattenObject(theme, '-', '--theme-'),
}))

/**
 * Wrapper of ThemeProvider which provides css variables to support
 * legacy css stylesheets.
 */
export default function ThemeProvider({
  children,
  name = 'dark',
}: PropsWithChildren<{ name?: ThemeName }>) {
  return (
    <WillowThemeProvider name={name}>
      <GlobalCSSVariables />
      {children}
    </WillowThemeProvider>
  )
}
