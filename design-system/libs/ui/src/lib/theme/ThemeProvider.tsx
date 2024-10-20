import {
  createTheme,
  CSSVariablesResolver,
  MantineProvider,
} from '@mantine/core'
import { StyledEngineProvider } from '@mui/material/styles'

import {
  useTheme,
  ThemeProvider as WillowThemeProvider,
  type ThemeName,
} from '@willowinc/theme'
import { SnackbarProvider } from '../feedback/Snackbars/SnackbarProvider'

import configureEchartsTheme from './echartsTheme'
import GlobalStyles from './GlobalStyles'

const resolver: CSSVariablesResolver = (theme) => {
  // These variables need to be specified for both the light and dark theme
  // in Mantine, however we they will have already been adjusted from our Willow theme,
  // so we can provide the same values in either case.
  const colorDependentVariables = {
    '--mantine-color-body': theme.other['willow'].color.neutral.bg.base.default,
    '--mantine-color-text': theme.other['willow'].color.neutral.fg.muted,
  }

  return {
    // https://mantine.dev/styles/css-variables-list/
    variables: {
      '--mantine-font-family':
        theme.other['willow'].font.body.md.regular.fontFamily,
      '--mantine-font-size-md':
        theme.other['willow'].font.body.md.regular.fontSize,
      '--mantine-line-height': '1.4',
    },
    light: colorDependentVariables,
    dark: colorDependentVariables,
  }
}

function MantineProviderWrapper({ children }: { children: React.ReactNode }) {
  const willowTheme = useTheme()
  const themeOverride = createTheme({
    other: {
      willow: willowTheme,
    },
  })

  return (
    <MantineProvider cssVariablesResolver={resolver} theme={themeOverride}>
      {children}
    </MantineProvider>
  )
}

/** ThemeProvider offers all styles required to use `@willowinc/ui` components. */
function ThemeProvider({
  children,
  includeGlobalStyles = true,
  name = 'dark',
}: {
  children: React.ReactNode
  includeGlobalStyles?: boolean
  name?: ThemeName
}) {
  configureEchartsTheme(name)

  return (
    // This will ensure that styles from styled-components are injected later in the DOM,
    // giving them higher priority over Emotion styles from mui even with the same specificity.
    <StyledEngineProvider injectFirst>
      <WillowThemeProvider
        includeGlobalStyles={includeGlobalStyles}
        name={name}
      >
        <MantineProviderWrapper>
          <SnackbarProvider>
            {includeGlobalStyles && <GlobalStyles />}
            {children}
          </SnackbarProvider>
        </MantineProviderWrapper>
      </WillowThemeProvider>
    </StyledEngineProvider>
  )
}

export { ThemeProvider, type ThemeName }
