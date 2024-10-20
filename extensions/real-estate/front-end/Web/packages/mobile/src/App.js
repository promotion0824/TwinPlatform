import './polyfill'
import React from 'react'
import { BrowserRouter } from 'react-router-dom'
import {
  AnalyticsProvider,
  cookie,
  setApiGlobalPrefix,
  ConfigProvider,
  CurrentTimeProvider,
  GlobalFetchProvider,
  GlobalPanelsProvider,
  IntlProvider,
  ModalsProvider,
  ModalProvider,
  OnClickOutsideIdsProvider,
  ScrollbarSizeProvider,
  SnackbarProvider,
  TooltipProvider,
  UserProvider,
  UserAgentProvider,
  FeatureFlagProvider,
} from '@willow/mobile-ui'
import InspectionsLoader from './views/Inspections/InspectionsLoader'
// Import from `packages/ui`
import { ReactQueryProvider, ThemeProvider } from '@willow/common'
import Site from './views/Site'

setApiGlobalPrefix(cookie.get('api'))

export default function App() {
  return (
    <BrowserRouter basename="/mobile-web/">
      <ReactQueryProvider>
        <IntlProvider>
          <GlobalFetchProvider>
            <ThemeProvider>
              <ScrollbarSizeProvider>
                <TooltipProvider>
                  <CurrentTimeProvider>
                    <SnackbarProvider>
                      <ConfigProvider configPrefix="wp-">
                        <AnalyticsProvider>
                          <UserProvider
                            configPrefix="wp-options-"
                            url="/api/me"
                          >
                            <FeatureFlagProvider>
                              <GlobalPanelsProvider>
                                <OnClickOutsideIdsProvider>
                                  <ModalsProvider>
                                    <ModalProvider>
                                      <UserAgentProvider>
                                        <InspectionsLoader />
                                        <Site />
                                      </UserAgentProvider>
                                    </ModalProvider>
                                  </ModalsProvider>
                                </OnClickOutsideIdsProvider>
                              </GlobalPanelsProvider>
                            </FeatureFlagProvider>
                          </UserProvider>
                        </AnalyticsProvider>
                      </ConfigProvider>
                    </SnackbarProvider>
                  </CurrentTimeProvider>
                </TooltipProvider>
              </ScrollbarSizeProvider>
            </ThemeProvider>
          </GlobalFetchProvider>
        </IntlProvider>
      </ReactQueryProvider>
    </BrowserRouter>
  )
}
