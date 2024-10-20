import { useQuery, UseQueryOptions } from 'react-query'
import {
  getConnectorsStats,
  ConnectorsStats,
} from '../../services/Connectors/ConnectorsService'
import { ConnectivityTableData } from '../../views/Admin/Portfolios/Connectivity/types/ConnectivityProvider'
import { RenderMetricObject } from '../../views/Admin/Portfolios/Connectivity/types/ConnectivityMetric'

type TData = {
  connectivityTableData: ConnectivityTableData
  renderMetricObject: RenderMetricObject
}

export default function useGetConnectorsStats(
  customerId: string,
  portfolioId: string,
  options?: UseQueryOptions<ConnectorsStats, any, TData>
) {
  return useQuery(
    ['getConnectorStats', customerId, portfolioId],
    () => getConnectorsStats(customerId, portfolioId),
    options
  )
}
