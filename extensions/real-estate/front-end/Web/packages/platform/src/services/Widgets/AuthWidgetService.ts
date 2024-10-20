import axios, { AxiosRequestConfig } from 'axios'
import { getUrl } from '@willow/ui'

export type AuthenticatedPowerBI = {
  expiration: string
  token: string
  url: string
}

export type AuthenticatedSigma = {
  url: string
  name: string
  reportId?: string
}

export type AuthenticatedReport = AuthenticatedPowerBI | AuthenticatedSigma

export type CustomConfig = {
  headers: {
    customerId: string
    siteIds: Array<string>
    start: string
    end: string
  }
  [key: string]: string | {}
}

export function getAuthenticatedReport(
  url: string,
  config?: AxiosRequestConfig<CustomConfig>
): Promise<AuthenticatedReport> {
  const getWidgetsUrl = getUrl(url)
  return axios.get(getWidgetsUrl, config).then(({ data }) => data)
}

export type AuthenticatedReportProps = {
  start: string
  end: string
  reportId?: string
  reportName?: string
  scopeId?: string
  customerId: string
  selectedDayRange: string
  selectedBusinessHourRange: string
  url: string
  tenantIds: string[]
}

/**
 * authenticate a simga report with POST method
 */
export function fetchAuthenticatedReport({
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
}: AuthenticatedReportProps): Promise<AuthenticatedReport> {
  const reportUrl = getUrl(url)

  return axios
    .post(reportUrl, {
      reportId,
      reportName,
      scopeId,
      customerId,
      start,
      end,
      selectedDayRange: [selectedDayRange],
      selectedBusinessHourRange: [selectedBusinessHourRange],
      tenantIds,
    })
    .then(({ data }) => data)
}
