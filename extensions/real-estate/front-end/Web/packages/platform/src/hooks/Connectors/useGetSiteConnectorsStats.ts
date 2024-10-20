import { useQuery, UseQueryOptions } from 'react-query'
import {
  getSiteConnectorsStats,
  TimeRange,
  ConnectorStat,
} from '../../services/Connectors/ConnectorsService'

export default function useGetSiteConnectorsStats(
  siteId: string,
  timeRange?: TimeRange,
  options?: UseQueryOptions<ConnectorStat[]>
) {
  return useQuery(
    ['getSiteConnectorStats', siteId],
    () => getSiteConnectorsStats(siteId, timeRange),
    options
  )
}
