import { cookie, setApiGlobalPrefix, App } from '@willow/ui'
import { ThemeProvider } from '@willow/common'

import AppContent from './AppContent'

setApiGlobalPrefix(cookie.get('api'))

export default function AppComponent() {
  return (
    <ThemeProvider>
      <App>
        <AppContent />
      </App>
    </ThemeProvider>
  )
}
