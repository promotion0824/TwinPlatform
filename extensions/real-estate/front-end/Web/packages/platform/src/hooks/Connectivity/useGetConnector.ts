/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { useQuery, UseQueryOptions } from 'react-query'
import {
  getConnector,
  Connector,
} from '../../services/Connectivity/ConnectivityService'

export default function useGetConnector(
  siteId: string,
  connectorId?: string,
  options?: UseQueryOptions<Connector>
) {
  return useQuery(
    ['connectivity-connector', siteId, connectorId],
    () => getConnector(siteId, connectorId!),
    {
      ...options,
      enabled: connectorId != null && options?.enabled !== false,
    }
  )
}
