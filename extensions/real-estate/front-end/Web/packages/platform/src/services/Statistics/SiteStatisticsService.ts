import axios from 'axios'
import { getUrl } from '@willow/ui'

export type MetricType = 'insights' | 'tickets'

export type StatsResponse = {
  overdueCount?: number
  urgentCount: number
  highCount: number
  mediumCount: number
  lowCount: number
  openCount: number
}

export function getSiteStats(
  siteId: string,
  type: MetricType,
  floorId?: string
): Promise<StatsResponse> {
  const getSiteStatsUrl = getUrl(`/api/statistics/${type}/site/${siteId}`)
  const params = { floorId }
  return axios.get(getSiteStatsUrl, { params }).then(({ data }) => data)
}
