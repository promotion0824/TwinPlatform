import { useQuery, UseQueryOptions } from 'react-query'
/* eslint-disable-next-line */
import { getReportsConfig } from '../../services/ReportsConfig/ReportsConfigService'
import { WidgetsResponse } from '../../services/Widgets/WidgetsService'

export default function useGetReportsConfig(
  portfolioId: string,
  options?: UseQueryOptions<WidgetsResponse>
) {
  return useQuery(
    ['portfolio-reports', portfolioId],
    () => getReportsConfig(portfolioId),
    options
  )
}
