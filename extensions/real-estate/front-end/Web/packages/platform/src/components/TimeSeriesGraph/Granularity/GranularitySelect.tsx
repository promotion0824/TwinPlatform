import { useAnalytics, useDateTime, useDuration } from '@willow/ui'
import { Icon, Select } from '@willowinc/ui'
import { Interval, getGranularityOptions } from './utils'

type GranularityProps = {
  times: [string, string]
  granularity: string
  onGranularityChange: (granularity: string) => void
}

/**
 * Select a granularity to view the live data on graph by. For example, for a 5 minutes
 * granularity, we will show a series of data for a time range, with 5 minutes interval.
 * The list of possible granularity is derived based on the time range provided
 * @see getGranularityOptions for more details on how the list is generated.
 */
export default function GranularitySelect({
  times,
  granularity,
  onGranularityChange,
  ...rest
}: GranularityProps) {
  const analytics = useAnalytics()
  const dateTime = useDateTime()
  const duration = useDuration()

  const diffInMinutes = dateTime(times[1]).differenceInMinutes(times[0])
  const granularityOptions: Interval[] = getGranularityOptions(times)

  return (
    <Select
      prefix={<Icon icon="timeline" />}
      value={granularity}
      onChange={(nextGranularity: string) => {
        onGranularityChange(nextGranularity)

        analytics.track('Time Series Granularity Changed', {
          granularity: duration(nextGranularity).toUIString(),
        })
      }}
      data={granularityOptions.map((option) => {
        const d = duration(option)
        return {
          value: d.toISOString() ?? '',
          label: d.toUIString(),
          disabled: d.isGreaterThan({ minutes: diffInMinutes }),
        }
      })}
      {...rest}
    />
  )
}
