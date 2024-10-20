import { withThemeFromJSXProvider } from '@storybook/addon-themes'
import { DocsContainer, type DocsContainerProps } from '@storybook/blocks'
import { Preview } from '@storybook/react'
import { darkTheme } from '@willowinc/theme'
import { ThemeProvider } from '@willowinc/ui'
import qs from 'qs'
import { useDarkMode } from 'storybook-dark-mode'
import DocsGlobalStyles from './DocsGlobalStyles'
import { createStorybookTheme } from './storybook-theme'

const ThemeProviderForStorybook = ({
  children,
}: {
  children: React.ReactNode
}) => {
  // Global styles will be provided in the document container, so if this is in the docs page
  // the global styles don't need to be included again, or they will be duplicated. On other
  // pages, such as individual story pages, the global styles should be included.
  const includeGlobalStyles =
    qs.parse(window.location.search, { ignoreQueryPrefix: true })[
      'viewMode'
    ] !== 'docs'

  return (
    <ThemeProvider
      includeGlobalStyles={includeGlobalStyles}
      name={useDarkMode() ? 'dark' : 'light'}
    >
      {includeGlobalStyles && <DocsGlobalStyles />}
      {children}
    </ThemeProvider>
  )
}

type DocumentContainerProps = {
  children: React.ReactNode
} & DocsContainerProps

const DocumentContainer = ({ children, ...rest }: DocumentContainerProps) => {
  const themeName = useDarkMode() ? 'dark' : 'light'

  return (
    <ThemeProvider name={themeName}>
      <DocsGlobalStyles />
      <DocsContainer {...rest} theme={createStorybookTheme(themeName)}>
        {children}
      </DocsContainer>
    </ThemeProvider>
  )
}

export const rootPreview: Preview = {
  parameters: {
    // Visit https://storybook.js.org/docs/react/essentials/actions for more information.
    actions: { argTypesRegex: '^on.*' },
    docs: {
      container: DocumentContainer,
      source: { type: 'code' }, //show all code same as source code
    },
    darkMode: {
      current: 'dark',
      dark: createStorybookTheme('dark'),
      light: createStorybookTheme('light'),
      stylePreview: true,
    },
  },
  decorators: [
    withThemeFromJSXProvider({
      themes: {
        dark: darkTheme,
      },
      defaultTheme: 'dark',
      Provider: ThemeProviderForStorybook,
    }),
  ],
}
