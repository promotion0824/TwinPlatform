import axios from 'axios'
import { getUrl } from '@willow/ui'

export type PerformanceResponse = {
  name: string
  values: { xValue: string; yValue?: number }[]
  yuom: string
}[]

export type GetPerformanceDataProp = {
  startDate: string
  endDate: string
  portfolioId: string
  customerId: string
  selectedDayRange: string
  selectedBusinessHourRange: string
  siteIds: string[]
  url: string
}

export function getPerformanceData({
  startDate,
  endDate,
  portfolioId,
  customerId,
  selectedDayRange,
  selectedBusinessHourRange,
  siteIds,
  url,
}: GetPerformanceDataProp): Promise<PerformanceResponse> {
  const getPerformanceUrl = getUrl(url)
  // using post for security reason as per backend
  return axios
    .post(
      getPerformanceUrl,
      {
        SiteIds: siteIds.join(','), // multiple site ids will be a comma separated string as per backend specification
        StartDate: startDate,
        EndDate: endDate,
        selectedDayRange,
        selectedBusinessHourRange,
      },
      {
        headers: {
          'Content-Type': 'application/json; charset=utf-8',
          PortfolioId: portfolioId,
          CustomerId: customerId,
        },
      }
    )
    .then(({ data }) => data)
}
