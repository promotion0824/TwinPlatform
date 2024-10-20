import { ReactNode } from 'react'
import { AnalyticsContext } from './AnalyticsContext'

export default function AnalyticsStubProvider({
  children,
  options,
}: {
  children: ReactNode
  options?: {
    initializeSiteContext: (siteContext: any) => void
  }
}) {
  return (
    <AnalyticsContext.Provider
      value={{
        initializeSiteContext: (siteContext) => {},
        identify: (id, traits) => {},
        track: (event, properties) => {},
        reset: () => {},
        ...options,
      }}
    >
      {children}
    </AnalyticsContext.Provider>
  )
}
