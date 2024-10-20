import { useQuery, UseQueryOptions } from 'react-query'
/* eslint-disable-next-line */
import {
  getPerformanceData,
  GetPerformanceDataProp,
} from '../../services/KpiDashboard/PerformanceDataService'

export interface UseGetPerformanceDataProp extends GetPerformanceDataProp {
  options?: UseQueryOptions
}

export default function useGetPerformanceData({
  startDate,
  endDate,
  portfolioId,
  customerId,
  selectedDayRange,
  selectedBusinessHourRange,
  siteIds = [] /* will return data for all sites if [] */,
  url,
  options,
}: UseGetPerformanceDataProp) {
  return useQuery(
    [
      'kpi-performance',
      startDate,
      endDate,
      portfolioId,
      customerId,
      selectedDayRange,
      selectedBusinessHourRange,
      url,
    ],
    () =>
      getPerformanceData({
        startDate,
        endDate,
        portfolioId,
        customerId,
        selectedDayRange,
        selectedBusinessHourRange,
        siteIds,
        url,
      }),
    options
  )
}
