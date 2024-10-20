import React, { ReactNode, useState } from 'react'
import { Screen } from '@testing-library/react'
import i18n, { InitOptions as I18NInitOptions } from 'i18next'
import { I18nextProvider } from 'react-i18next'
import { HelmetProvider } from 'react-helmet-async'
import { MemoryRouter } from 'react-router'
import { ReactQueryStubProvider, ThemeProvider } from '@willow/common'
import '@willow/common/utils/testUtils/matchMediaMock'
import SnackbarStubProvider from '@willow/ui/providers/SnackbarProvider/SnackbarStubProvider'
import AnalyticsStubProvider from '@willow/ui/providers/AnalyticsProvider/AnalyticsStubProvider'
import UserStubProvider from '@willow/ui/providers/UserProvider/UserStubProvider'
import LanguageStubProvider from '@willow/ui/providers/LanguageProvider/LanguageStubProvider'
import OnClickOutsideIdsStubProvider from '@willow/ui/providers/OnClickOutsideIdsProvider/OnClickOutsideIdsStubProvider'
import FeatureFlagStubProvider from '@willow/ui/providers/FeatureFlagProvider/FeatureFlagStubProvider'
import ConfigStubProvider from '@willow/ui/providers/ConfigProvider/ConfigStubProvider'
import FetchRefreshStubProvider from '@willow/ui/providers/FetchRefreshProvider/FetchRefreshStubProvider'
import ScopeSelectorStubProvider from '@willow/ui/providers/ScopeSelectorProvider/ScopeSelectorStubProvider'
import { User } from '../../../../platform/src/views/Portfolio/KPIDashboards/HeaderControls/HeaderControls'
import ConsoleProvider from '@willow/ui/providers/ConsoleProvider/ConsoleProvider'
import SitesStubProvider from '../../../../platform/src/providers/sites/SitesStubProvider'
import { Site } from '@willow/common/site/site/types'
import { LocationDisplay } from '@willow/common/utils/testUtils/LocationDisplay'
import ChatAppStubProvider from '@willow/common/copilot/ChatApp/ChatAppStubProvider'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { buildingModelId } from '@willow/common/twins/view/modelsOfInterest'

/**
 * A generic wrapper component for tests that provides stubs of most of the
 * providers we use in our real code. We should expand this component's props
 * to support configuring more of the providers - for example we should be able
 * to inject user data, feature flags etc.
 */
export default function Wrapper({
  children,
  debug = false,
  translation = {},
  i18nOptions = {
    resources: {
      en: { translation },
    },
    fallbackLng: ['en'],
  },
  hasFeatureToggle,
  user,
  initialEntries,
  sites = [],
  scopeLocation,
  isScopeSelectorEnabled = false,
  featureFlagsLoaded = true,
  errorOnLoad = false,
}: {
  children?: ReactNode
  /**
   * Translations can be passed directly as an object now instead of passing it
   * in i18nOptions.
   */
  translation?: { [key: string]: string }
  /**
   * Whether to enable debug mode. Currently this means the ConsoleProvider will
   * do logging, even in tests.
   */
  debug?: boolean
  // react-i18next gives us annoying warnings if fallbackLng is not present
  // or is an empty array.
  i18nOptions?: Omit<I18NInitOptions, 'fallbackLng'> & { fallbackLng: string[] }
  hasFeatureToggle?: (feature: string) => boolean
  user?: Partial<User>
  initialEntries?: string[]
  sites?: Array<Partial<Site>>
  isScopeSelectorEnabled?: boolean
  scopeLocation?: LocationNode
  featureFlagsLoaded?: boolean
  errorOnLoad?: boolean
}) {
  if (i18nOptions.fallbackLng.length === 0) {
    throw new Error(
      'Must specify fallbackLng as a nonempty array or we will get lots of warnings'
    )
  }

  // i18next makes us go through a bunch of hoops in order to make translations
  // available in a context without messing around with global variables that
  // affect behaviour in tests that don't use this wrapper.
  const [isReady, setIsReady] = useState(false)
  const [i18nInstance] = useState(() => {
    const instance = i18n.createInstance()
    instance.init(i18nOptions ?? {}, () => setIsReady(true))
    return instance
  })

  if (!isReady) {
    return null
  }

  return (
    <ThemeProvider>
      <ConsoleProvider debug={debug}>
        <HelmetProvider>
          <MemoryRouter initialEntries={initialEntries}>
            <ReactQueryStubProvider>
              <ChatAppStubProvider>
                <LanguageStubProvider>
                  <SnackbarStubProvider>
                    <ConfigStubProvider>
                      <AnalyticsStubProvider>
                        <UserStubProvider options={user}>
                          <I18nextProvider i18n={i18nInstance}>
                            <OnClickOutsideIdsStubProvider>
                              <FeatureFlagStubProvider
                                hasFeatureToggle={hasFeatureToggle}
                                featureFlagsLoaded={featureFlagsLoaded}
                                errorOnLoad={errorOnLoad}
                              >
                                <ScopeSelectorStubProvider
                                  scopeLocation={scopeLocation}
                                  isScopeSelectorEnabled={
                                    isScopeSelectorEnabled
                                  }
                                  isScopeUsedAsBuilding={(scope) =>
                                    scope?.twin?.metadata?.modelId ===
                                    buildingModelId
                                  }
                                >
                                  <SitesStubProvider sites={sites}>
                                    <FetchRefreshStubProvider>
                                      <LocationDisplay />
                                      {children}
                                    </FetchRefreshStubProvider>
                                    <input
                                      type="hidden"
                                      data-testid="wrapper-is-ready"
                                    />
                                  </SitesStubProvider>
                                </ScopeSelectorStubProvider>
                              </FeatureFlagStubProvider>
                            </OnClickOutsideIdsStubProvider>
                          </I18nextProvider>
                        </UserStubProvider>
                      </AnalyticsStubProvider>
                    </ConfigStubProvider>
                  </SnackbarStubProvider>
                </LanguageStubProvider>
              </ChatAppStubProvider>
            </ReactQueryStubProvider>
          </MemoryRouter>
        </HelmetProvider>
      </ConsoleProvider>
    </ThemeProvider>
  )
}

export function wrapperIsReady(screen: Screen) {
  return screen.queryByTestId('wrapper-is-ready') != null
}
