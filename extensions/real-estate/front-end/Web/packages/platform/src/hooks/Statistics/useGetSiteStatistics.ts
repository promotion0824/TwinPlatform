import { useQuery, UseQueryOptions } from 'react-query'
import {
  getSiteStats,
  MetricType,
} from '../../services/Statistics/SiteStatisticsService'

export default function useGetSiteStatistics(
  siteId: string,
  type: MetricType,
  floorCode?: string,
  options?: UseQueryOptions
) {
  return useQuery(
    ['stats', siteId, type, floorCode],
    () => getSiteStats(siteId, type, floorCode),
    options
  )
}
