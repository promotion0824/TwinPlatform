import { useQuery, UseQueryOptions } from 'react-query'
import {
  AuthenticatedReportProps,
  fetchAuthenticatedReport,
  AuthenticatedReport,
} from '../../services/Widgets/AuthWidgetService'

function usePostAuthenticatedReport(
  {
    reportId,
    reportName,
    scopeId,
    customerId,
    start,
    end,
    selectedDayRange,
    selectedBusinessHourRange,
    url,
    tenantIds,
  }: AuthenticatedReportProps,
  options?: UseQueryOptions<AuthenticatedReport>
) {
  return useQuery(
    [
      'fetch-authenticated-report',
      reportId,
      reportName,
      scopeId,
      customerId,
      start,
      end,
      selectedDayRange,
      selectedBusinessHourRange,
      url,
      tenantIds,
    ],
    () =>
      fetchAuthenticatedReport({
        reportId,
        reportName,
        scopeId,
        customerId,
        start,
        end,
        selectedDayRange,
        selectedBusinessHourRange,
        url,
        tenantIds,
      }),
    {
      cacheTime: 0, // disable cache because report has to be authenticated every time
      ...options,
    }
  )
}

export default usePostAuthenticatedReport
