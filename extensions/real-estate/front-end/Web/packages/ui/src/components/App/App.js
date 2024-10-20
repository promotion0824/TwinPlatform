import { ChatContextProvider, i18n, ReactQueryProvider } from '@willow/common'
import {
  AnalyticsProvider,
  ApplicationInsightsProvider,
  ConfigProvider,
  DocumentTitle,
  FeatureFlagProvider,
  FetchRefreshProvider,
  IntlProvider,
  LanguageProvider,
  OnClickOutsideIdsProvider,
  SnackbarProvider,
  TooltipProvider,
  UserAgentProvider,
  UserProvider,
} from '@willow/ui'
import { Snackbars } from '@willowinc/ui'
import { HelmetProvider } from 'react-helmet-async'
import { BrowserRouter } from 'react-router-dom'
import ConsoleProvider from '../../providers/ConsoleProvider/ConsoleProvider'

export default function App({ children }) {
  return (
    <ConsoleProvider>
      <BrowserRouter>
        <ReactQueryProvider>
          <IntlProvider>
            <UserAgentProvider>
              <FetchRefreshProvider>
                <TooltipProvider>
                  <SnackbarProvider>
                    <ConfigProvider configPrefix="wp-">
                      <ApplicationInsightsProvider>
                        <AnalyticsProvider>
                          <UserProvider
                            configPrefix="wp-options-"
                            url="/api/me"
                          >
                            <OnClickOutsideIdsProvider>
                              <FeatureFlagProvider>
                                <LanguageProvider i18n={i18n}>
                                  <ChatContextProvider>
                                    <HelmetProvider>
                                      <DocumentTitle />
                                      {children}
                                      <Snackbars />
                                    </HelmetProvider>
                                  </ChatContextProvider>
                                </LanguageProvider>
                              </FeatureFlagProvider>
                            </OnClickOutsideIdsProvider>
                          </UserProvider>
                        </AnalyticsProvider>
                      </ApplicationInsightsProvider>
                    </ConfigProvider>
                  </SnackbarProvider>
                </TooltipProvider>
              </FetchRefreshProvider>
            </UserAgentProvider>
          </IntlProvider>
        </ReactQueryProvider>
      </BrowserRouter>
    </ConsoleProvider>
  )
}
