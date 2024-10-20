import duration from '@willow/ui/hooks/useDuration/duration'
import { api } from '@willow/ui'

type Point = {
  pointId: string
  siteId: string
}

/**
 * Make a request to the `/api/livedata/export/csv` endpoint based on the
 * specified time series settings used within Time Series page and
 * Mini Time Series component.
 */
export default async function exportCsv(
  [start, end]: [string, string],
  granularity: string,
  points: Point[],
  timeZoneId: string
) {
  const response = await api.post(
    '/livedata/export/csv',
    {
      start,
      end,
      interval: duration(granularity).toDotnetString(),
      points,
      timeZoneId,
    },
    { responseType: 'text' }
  )
  return response.data
}
