import { useIntl } from 'providers/IntlProvider/IntlContext'
import dateTime from './dateTime'

export default function useDateTime() {
  const intl = useIntl()

  const intlDateTime = (time, timezone, locale) =>
    dateTime(time, timezone ?? intl?.timezone, locale ?? intl?.locale)

  intlDateTime.now = (timezone, locale) =>
    dateTime.now(timezone ?? intl?.timezone, locale ?? intl?.locale)

  return intlDateTime
}
