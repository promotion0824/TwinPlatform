/* eslint-disable @typescript-eslint/no-empty-function */
import { AnalyticsContext } from './AnalyticsContext'

/**
 * Stub version of AnalyticsProvider. Currently does not
 * push error events in an ErrorBoundary like the real AnalyticsProvider
 * does.
 */
export default function AnalyticsStubProvider({ children }) {
  const context = {
    initializeSiteContext: (_siteContext) => {},
    identify: (_id, _traits) => {},
    page: (_pageName, _properties) => {},
    track: (_event, _properties) => {},
    reset: () => {},
  }

  return (
    <AnalyticsContext.Provider value={context}>
      {children}
    </AnalyticsContext.Provider>
  )
}
