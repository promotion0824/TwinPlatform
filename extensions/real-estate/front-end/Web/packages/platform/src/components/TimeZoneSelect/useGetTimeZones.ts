import { useQuery } from 'react-query'
import { api } from '@willow/ui'
import { DateTime, IANAZone } from 'luxon'
import { useMemo } from 'react'
import _ from 'lodash'

/**
 * The time zone info with:
 * - id and displayName from https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo?view=net-6.0
 * - offset - The UTC offset, formatted as +XX:XX or -XX:XX
 * - regionTimeZone, with region as key and the IANA identifier for the region as value.
 *   Each regionTimeZone, if present, will have the global default Olson time zone, represented by 001 as key.
 *   A region may consist of one or more time zones, separated by space.
 *   @see {@link https://cldr.unicode.org/development/development-process/design-proposals/extended-windows-olson-zid-mapping}
 */
export type TimeZoneInfo = {
  id: string
  displayName: string
  offset: string
  regionTimeZone?: {
    '001': string
    [region: string]: string
  }
}

/**
 * Get the IANA time zone identifier from TimeZoneInfo, if specified. The time zone is based
 * on the global default time zone ("001" in regionTimeZone). If this is not present,
 * then return the UTC offset.
 */
export const getTimeZone = (timeZoneInfo: TimeZoneInfo): string | undefined =>
  timeZoneInfo.regionTimeZone?.['001'] || `UTC${timeZoneInfo.offset}`

const useGetTimeZones = () =>
  useQuery<TimeZoneInfo[]>('timezones', async () => {
    const response = await api.get('timezones')
    return response.data
  })

/**
 * Use time zone info based on a time zone id. If time zone id is not
 * specified, then find and return the time zone info that matches the
 * system's time zone.
 */
export const useTimeZoneInfo = (timeZoneId?: string): TimeZoneInfo | null => {
  const { data: timeZones } = useGetTimeZones()
  return useMemo(
    () =>
      timeZones?.find((timeZoneInfo: TimeZoneInfo) => {
        if (timeZoneId) {
          return timeZoneInfo.id === timeZoneId
        }

        if (timeZoneInfo.regionTimeZone) {
          const regionTimeZones = Object.values(timeZoneInfo.regionTimeZone)

          // Splitting the region's time zone value by a space because a region
          // may have more than 1 time zones.
          const timeZoneList = _.flatMap(regionTimeZones, (timeZone) =>
            timeZone.split(' ')
          )
          // Add id to the list only if it is a valid IANA zone name (for example "UTC")
          if (new IANAZone(timeZoneInfo.id).isValid) {
            timeZoneList.push(timeZoneInfo.id)
          }

          const { zoneName } = DateTime.local()
          return zoneName != null ? timeZoneList.includes(zoneName) : false
        }

        return false
      }) || null,
    [timeZones, timeZoneId]
  )
}

export default useGetTimeZones
