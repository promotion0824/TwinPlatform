import { titleCase } from '@willow/common'
import { Badge } from '@willowinc/ui'
import { forwardRef, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { DataTile, DataTileField } from '../DataTile/DataTile'

export interface LocationDataTileProps {
  /** The area of the location. Should be in a format similar to "21,000 sqft" or "21,000 sqm". */
  area: string
  /** The location's geographical location. Ideally in the format City, State (New York, NY). */
  location: string
  /** The status of the location. */
  status: string
  /** The location's local timezone's name, in the format "America/Chicago". */
  timeZone: string
  /** Will be displayed if the status is set to `operations`. */
  yearOpened: number
}

export const LocationDataTile = forwardRef<
  HTMLDivElement,
  LocationDataTileProps
>(({ area, location, status, timeZone, yearOpened, ...restProps }, ref) => {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [currentTime, setCurrentTime] = useState(new Date())

  useEffect(() => {
    const interval = setInterval(() => setCurrentTime(new Date()), 10000)
    return () => clearInterval(interval)
  }, [])

  const timeFormatter = new Intl.DateTimeFormat(undefined, {
    hour12: true,
    timeStyle: 'short',
    timeZone,
  })

  const fields: Array<DataTileField> = [
    {
      icon: 'construction',
      label: t('labels.status'),
      value: (
        <Badge
          color={status.toLowerCase() === 'operations' ? 'green' : 'gray'}
          size="sm"
          variant="dot"
        >
          {titleCase({
            language,
            text: t(`plainText.${status.toLowerCase()}`),
          })}
        </Badge>
      ),
    },
    {
      icon: 'my_location',
      label: titleCase({ language, text: t('labels.location') }),
      value: location,
    },
    {
      icon: 'square_foot',
      label: t('labels.size'),
      value: area,
    },
    ...(status.toLowerCase() === 'operations'
      ? [
          {
            icon: 'build_circle' as const,
            label: titleCase({ language, text: t('labels.yearOpened') }),
            value: yearOpened,
          },
        ]
      : []),
    {
      icon: 'schedule',
      label: titleCase({ language, text: t('labels.localTime') }),
      value: timeFormatter.format(currentTime),
    },
  ]

  return <DataTile fields={fields} ref={ref} {...restProps} />
})
