import { LicenseInfo } from '@mui/x-license-pro'

LicenseInfo.setLicenseKey(
  'a914bcf589677e9d1a41977085571d48Tz04NjI2MixFPTE3NDE4NDc4MjIwMDAsUz1wcm8sTE09c3Vic2NyaXB0aW9uLEtWPTI='
)

export { default as baseTokens } from './tokens/base.json'
export { default as darkThemeTokens } from './tokens/darkThemeTokens.json'
export { default as lightThemeTokens } from './tokens/lightThemeTokens.json'

export { default as GlobalStyle } from './lib/GlobalStyles'
export { default as ThemeProvider, useTheme } from './lib/ThemeProvider'
export { default as getElementNormalizingStyle } from './lib/normalizingStyles'

export {
  type Theme,
  type ThemeName,
  baseTheme,
  darkTheme,
  lightTheme,
} from './theme'
