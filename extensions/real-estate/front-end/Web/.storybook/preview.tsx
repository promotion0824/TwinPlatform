import { withThemeFromJSXProvider } from '@storybook/addon-styling'
import {
  Description,
  DocsContainer,
  DocsContainerProps,
  Stories,
  Subtitle,
  Title,
} from '@storybook/blocks'
import { Preview } from '@storybook/react'
import { themes } from '@storybook/theming'
import { darkTheme } from '@willowinc/theme'
import { ThemeProvider } from '@willowinc/ui'
import i18next from 'i18next'
import { initialize, mswLoader } from 'msw-storybook-addon'
import qs from 'qs'
import React from 'react'
import { HelmetProvider } from 'react-helmet-async'
import { initReactI18next } from 'react-i18next'
import { MemoryRouter } from 'react-router-dom'

import GlobalCSSVariables from '../packages/common/src/providers/ThemeProvider/ThemeProvider'
import enTranslation from '../packages/platform/src/public/translations/en.json'
import frTranslation from '../packages/platform/src/public/translations/fr.json'
import '../packages/ui/src/index.css'
import { OnClickOutsideIdsProvider } from '../packages/ui/src/providers/OnClickOutsideIdsProvider/OnClickOutsideIdsProvider'
import { UserAgentProvider } from '../packages/ui/src/providers/UserAgentProvider/UserAgentProvider'
import '../packages/ui/src/theme.css'

i18next.use(initReactI18next).init({
  lng: 'en',
  resources: {
    en: enTranslation,
    fr: frTranslation,
  },
  debug: true,
})

initialize({
  onUnhandledRequest: 'bypass',
  serviceWorker: {
    url: '/mockServiceWorker.js',
  },
})

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
    <ThemeProvider includeGlobalStyles={includeGlobalStyles} name="dark">
      {includeGlobalStyles && <GlobalCSSVariables />}
      {children}
    </ThemeProvider>
  )
}

type DocumentContainerProps = {
  children: React.ReactNode
} & DocsContainerProps

const DocumentContainer = ({ children, ...rest }: DocumentContainerProps) => (
  <ThemeProvider name="dark">
    <GlobalCSSVariables />
    <MemoryRouter>
      <HelmetProvider>
        <OnClickOutsideIdsProvider>
          <UserAgentProvider>
            <DocsContainer {...rest}>{children}</DocsContainer>
          </UserAgentProvider>
        </OnClickOutsideIdsProvider>
      </HelmetProvider>
    </MemoryRouter>
  </ThemeProvider>
)

const preview: Preview = {
  decorators: [
    withThemeFromJSXProvider({
      themes: {
        dark: darkTheme,
      },
      defaultTheme: 'dark',
      Provider: ThemeProviderForStorybook,
    }),
  ],
  loaders: [mswLoader],
  parameters: {
    docs: {
      container: DocumentContainer,
      theme: themes.dark,
      // Removing controls on docs page until they are configured properly
      // see https://storybook.js.org/docs/react/writing-docs/autodocs#write-a-custom-template
      page: () => (
        <>
          <Title />
          <Subtitle />
          <Description />
          <Stories />
        </>
      ),
    },
    backgrounds: {
      default: 'dark',
      values: [
        { name: 'dark', value: 'var(--theme-color-neutral-bg-panel-default)' },
      ],
    },
    actions: { argTypesRegex: '^on[A-Z].*' },
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/,
      },
    },
  },
}

export default preview
