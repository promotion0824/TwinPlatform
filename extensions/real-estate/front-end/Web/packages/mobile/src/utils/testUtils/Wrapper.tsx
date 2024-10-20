import { ThemeProvider } from '@willow/common'
import ReactQueryStubProvider from '@willow/common/providers/ReactQueryProvider/ReactQueryStubProvider'
import { LocationDisplay } from '@willow/common/utils/testUtils/LocationDisplay'
import { setApiGlobalPrefix } from '@willow/mobile-ui'
import FeatureFlagStubProvider from '@willow/mobile-ui/providers/FeatureFlagProvider/FeatureFlagStubProvider'
import OnClickOutsideIdsStubProvider from '@willow/mobile-ui/providers/OnClickOutsideIdsProvider/OnClickOutsideIdsStubProvider'
import AnalyticsStubProvider from '@willow/mobile-ui/providers/analytics/AnalyticsStubProvider'
import GlobalFetchStubProvider from '@willow/mobile-ui/providers/globalFetch/GlobalFetchStubProvider'
import IntlStubProvider from '@willow/mobile-ui/providers/intl/IntlStubProvider'
import SnackbarStubProvider from '@willow/mobile-ui/providers/snackbar/SnackbarStubProvider'
import { User } from '@willow/mobile-ui/providers/user/UserContext'
import UserStubProvider from '@willow/mobile-ui/providers/user/UserStubProvider'
import UserAgentStubProvider from '@willow/mobile-ui/providers/userAgent/UserAgentStubProvider'
import FetchRefreshStubProvider from '@willow/ui/providers/FetchRefreshProvider/FetchRefreshStubProvider'
import { ReactNode } from 'react'
import { HelmetProvider } from 'react-helmet-async'
import { MemoryRouter } from 'react-router'

setApiGlobalPrefix('us')

/**
 * An incomplete wrapper providing stubs for some of the providers used in the
 * mobile app.
 */
export default function Wrapper({
  children,
  initialEntries,
  user,
  intl,
  hasFeatureToggle,
}: {
  children: ReactNode
  initialEntries?: string[]
  user?: Partial<User>
  intl?: {
    locale?: string
    timezone?: string
    setLocale?: (nextLocale: string) => void
    setTimezone?: (nextTimezone: string) => void
  }
  hasFeatureToggle?: (featureFlag) => boolean
}) {
  return (
    <ThemeProvider>
      <HelmetProvider>
        <MemoryRouter initialEntries={initialEntries}>
          <ReactQueryStubProvider>
            <IntlStubProvider {...intl}>
              <GlobalFetchStubProvider>
                <OnClickOutsideIdsStubProvider>
                  <FeatureFlagStubProvider hasFeatureToggle={hasFeatureToggle}>
                    <SnackbarStubProvider>
                      <AnalyticsStubProvider>
                        <FetchRefreshStubProvider>
                          <UserAgentStubProvider>
                            <UserStubProvider options={user}>
                              <LocationDisplay />
                              {children}
                            </UserStubProvider>
                          </UserAgentStubProvider>
                        </FetchRefreshStubProvider>
                      </AnalyticsStubProvider>
                    </SnackbarStubProvider>
                  </FeatureFlagStubProvider>
                </OnClickOutsideIdsStubProvider>
              </GlobalFetchStubProvider>
            </IntlStubProvider>
          </ReactQueryStubProvider>
        </MemoryRouter>
      </HelmetProvider>
    </ThemeProvider>
  )
}
