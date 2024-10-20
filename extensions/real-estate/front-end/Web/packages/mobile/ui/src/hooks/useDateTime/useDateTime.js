import { useIntl } from 'providers'
import dateTime from './dateTime'

export default function useDateTime() {
  const intl = useIntl()

  const intlDateTime = (
    time,
    timezone = intl?.timezone,
    locale = intl?.locale
  ) => dateTime(time, timezone, locale)

  intlDateTime.now = (timezone = intl?.timezone, locale = intl?.locale) =>
    dateTime.now(timezone, locale)

  return intlDateTime
}
