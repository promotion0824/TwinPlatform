import { UseQueryResult } from 'react-query'
import { BadgeProps } from '@willowinc/ui'
import axios from 'axios'
import { getUrl } from '@willow/ui'

type Telemetry = {
  timestamp: string
  totalTelemetryCount: number
  uniqueCapabilityCount: number
  setState: string
  status: string
}
export type Telemetries = Telemetry[]
type Status = { timestamp: string; setState?: string; status?: string }
type Statuses = Status[]

export type ConnectorStat = {
  siteId: string
  connectorId: string
  connectorName: string
  currentSetState?: string
  currentStatus?: string
  disabledCapabilitiesCount: number
  hostingDevicesCount: number
  totalTelemetryCount: number
  totalCapabilitiesCount: number
  telemetry: Telemetries
  status: Statuses
  color?: BadgeProps['color']
}
export type ConnectorsStats = {
  siteId: string
  connectorStats: ConnectorStat[]
}[]

export type TimeRange = {
  start: string
  end: string
}
export type ConnectorsStatsQueryType = UseQueryResult<ConnectorsStats>

export function getConnectorsStats(
  customerId: string,
  portfolioId: string
): Promise<ConnectorsStats> {
  const getConnectorsStatsUrl = getUrl(
    `/api/customers/${customerId}/portfolio/${portfolioId}/livedata/stats/connectors`
  )

  return axios
    .post(getConnectorsStatsUrl, { ConnectorIds: [] })
    .then(({ data }) => data)
}

export function getSiteConnectorsStats(
  siteId: string,
  timeRange?: TimeRange
): Promise<ConnectorStat[]> {
  const getSiteConnectorsStatsUrl = getUrl(
    `/api/sites/${siteId}/livedata/stats/connectors`
  )

  return axios
    .get(getSiteConnectorsStatsUrl, { params: timeRange })
    .then(({ data }) => data)
}
