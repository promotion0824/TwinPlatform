import './index.css'
import {
  cookie,
  setApiGlobalPrefix,
  Fetch,
  FetchRefreshProvider,
  Flex,
  LanguageProvider,
  UserProvider,
  UserAgentProvider,
} from '@willow/ui'
import { i18n, ReactQueryProvider, ThemeProvider } from '@willow/common'
import AutodeskViewerNew from './AutodeskViewerNew/AutodeskViewer'

setApiGlobalPrefix(cookie.get('api'))

export default function Site() {
  return (
    <ThemeProvider>
      <FetchRefreshProvider>
        <UserAgentProvider>
          <ReactQueryProvider>
            <UserProvider configPrefix="wp-options-" url="/api/me">
              <LanguageProvider i18n={i18n}>
                <Flex height="100%">
                  <Fetch url="/api/forge/oauth/token">
                    {(token) => <AutodeskViewerNew token={token} />}
                  </Fetch>
                </Flex>
              </LanguageProvider>
            </UserProvider>
          </ReactQueryProvider>
        </UserAgentProvider>
      </FetchRefreshProvider>
    </ThemeProvider>
  )
}
