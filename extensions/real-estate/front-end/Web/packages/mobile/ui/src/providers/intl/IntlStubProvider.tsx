import { ReactNode } from 'react'
import { IntlContext } from './IntlContext'

export default function IntlStubProvider({
  locale,
  timezone,
  setLocale = (_nextLocale) => {},
  setTimezone = (_nextTimezone) => {},
  children,
}: {
  locale?: string
  timezone?: string
  setLocale?: (locale: string) => void
  setTimezone?: (timezone: string) => void
  children: ReactNode
}) {
  return (
    <IntlContext.Provider
      value={{
        locale,
        timezone,
        setLocale,
        setTimezone,
      }}
    >
      {children}
    </IntlContext.Provider>
  )
}
