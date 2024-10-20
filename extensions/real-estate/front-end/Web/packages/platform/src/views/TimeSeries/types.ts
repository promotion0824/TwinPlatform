import { QuickRangeOption } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { TimeZoneOption } from '../../components/TimeZoneSelect/TimeZoneSelect'

export type TimeSeries = {
  quickRange?: QuickRangeOption
  setState: (state: TimeSeriesState) => void
  setTimeRange: (quickRange?: QuickRangeOption) => void
  setTimeZoneOption: (value?: TimeZoneOption['value']) => void
  state: TimeSeriesState
  timeZone?: string
  timeZoneOption?: TimeZoneOption['value']
}

export type TimeSeriesFavorite = {
  granularity: string
  name: string
  quickSelectTimeRange?: QuickRangeOption
  siteEquipmentIds: string[]
  sitePointIds: string[]
  /**
   * Time difference in milliseconds for the start and end date
   * This array contains:
   * 1) The difference in milliseconds between now and start DateTime.
   * 2) The difference in milliseconds between now and end DateTime.
   * where now is the DateTime when user was viewing the time series.
   */
  timeDiffs: [number, number]
  timeZone?: string
  timeZoneOption?: TimeZoneOption['value']
  type: TimeSeriesState['type']
}

export type TimeSeriesState = {
  granularity: string
  kind?: 'site' | 'personal'
  name?: string
  siteEquipmentIds: string[]
  siteId: string
  sitePointIds: string[]
  times: [string, string]
  type: 'asset' | 'stacked'
}
