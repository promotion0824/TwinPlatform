/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { AxiosRequestConfig } from 'axios'
import { useQuery, UseQueryOptions } from 'react-query'
/* eslint-disable-next-line */
import {
  AuthenticatedReport,
  CustomConfig,
  getAuthenticatedReport,
} from '../../services/Widgets/AuthWidgetService'

export default function useGetAuthenticatedReport(
  params: {
    url?: string
    config?: AxiosRequestConfig<CustomConfig>
  },
  options?: UseQueryOptions<AuthenticatedReport>
) {
  const { url, config } = params
  return useQuery(
    ['authenticate-report', url],
    () => getAuthenticatedReport(url!, config),
    {
      cacheTime: 0, // disable cache because report has to be authenticated every time
      ...options,
      enabled: url != null && options?.enabled !== false,
    }
  )
}
