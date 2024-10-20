import { useState } from 'react'
import { IntlContext } from './IntlContext'

export default function IntlProvider(props) {
  const [state, setState] = useState({
    locale: navigator.language,
    timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
  })

  const context = {
    locale: state.locale,
    timezone: state.timezone,

    setLocale(nextLocale) {
      setState((prevState) => ({
        ...prevState,
        locale: nextLocale ?? navigator.language,
      }))
    },

    setTimezone(nextTimezone) {
      setState((prevState) => ({
        ...prevState,
        timezone:
          nextTimezone ?? Intl.DateTimeFormat().resolvedOptions().timeZone,
      }))
    },
  }

  return <IntlContext.Provider {...props} value={context} />
}
