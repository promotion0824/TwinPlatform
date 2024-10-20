import { BadgeProps } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation, TFunction } from 'react-i18next'
import { useGetConnectorsStats } from '../../../../../hooks'
import {
  ConnectorsStats,
  ConnectorStat,
} from '../../../../../services/Connectors/ConnectorsService'
import { useSites } from '../../../../../providers/sites/SitesContext'
import {
  ENABLED,
  ONLINE_CONNECTOR_STATUSES,
  OFFLINE_CONNECTOR_STATUSES,
} from '../utils'
import { Site } from '@willow/common/site/site/types'

export default function useGetConnectivityData(
  customerId: string,
  portfolioId: string
) {
  const { t } = useTranslation()
  const sites = useSites()

  return useGetConnectorsStats(customerId, portfolioId, {
    select: (data: ConnectorsStats) => ({
      connectivityTableData: constructConnectivityTableData(data, sites),
      renderMetricObject: constructRenderMetricObject(data, t),
    }),
  })
}

function constructConnectivityTableData(
  connectorsStatData: ConnectorsStats,
  sites: Site[]
) {
  return Object.values(connectorsStatData)
    .map(({ siteId, connectorStats }) => {
      const site = sites.find(({ id }) => id === siteId)
      const { name, state, country, suburb, type } = site || {}

      const enabledConnector = connectorStats.filter(
        (connector) => connector.currentSetState === ENABLED
      )

      // calculate the number of connectors that are enabled
      const enabledConnectorCount = enabledConnector.length

      // calculate the number of connectors where its status is online
      const onlineConnectorCount = connectorStats.filter(
        (connector) =>
          connector.currentStatus &&
          ONLINE_CONNECTOR_STATUSES.includes(connector.currentStatus)
      ).length

      // calculate the total live data points in the last hour for all the connectors in a site
      const dataIn = _.sumBy(enabledConnector, (c: ConnectorStat) =>
        c.telemetry.length ? c.telemetry[0].totalTelemetryCount : 0
      )

      // a site is online when all its connectors is online
      const isSiteOnline =
        enabledConnectorCount === 0
          ? false
          : enabledConnectorCount === onlineConnectorCount

      return {
        siteId,
        name,
        state,
        country,
        city: suburb,
        assetClass: type,
        dataIn,
        isOnline: isSiteOnline,
        connectorStatus: `${onlineConnectorCount}/${enabledConnectorCount}`,
        // green when all connectors are online,
        // orange when some connectors are online,
        // gray when no connectors are online
        color: (isSiteOnline
          ? 'green'
          : onlineConnectorCount > 0
          ? 'orange'
          : 'gray') as BadgeProps['color'],
      }
    })
    .sort((a, b) => {
      if (a.name && b.name) return a.name.localeCompare(b.name)
      else if (a.name || b.name) return a.name ? -1 : 1 // object with undefined name will be at the bottom of the list.
      return 0
    }) // sort by alphanumeric order
}

function constructRenderMetricObject(
  connectorsStatData: ConnectorsStats,
  t: TFunction
) {
  const enabledConnectors = connectorsStatData
    .map(({ siteId, connectorStats }) => ({
      siteId,
      connectorStats: connectorStats.filter(
        (connector) => connector.currentSetState === ENABLED
      ),
    }))
    .filter((connector) => connector.connectorStats.length > 0)

  const offlineConnectorsCount = enabledConnectors
    .flatMap(({ connectorStats }) => connectorStats)
    .filter(
      (connector) =>
        connector.currentStatus &&
        OFFLINE_CONNECTOR_STATUSES.includes(connector.currentStatus)
    ).length

  // site is online when all its enabled connectors is online
  const siteOnlineCount = enabledConnectors.filter(({ connectorStats }) =>
    connectorStats.every(
      (c) =>
        c.currentStatus && ONLINE_CONNECTOR_STATUSES.includes(c.currentStatus)
    )
  ).length

  return metricObject(t, siteOnlineCount, offlineConnectorsCount)
}

export const metricObject = (
  t: TFunction,
  siteOnlineCount = 0,
  offlineConnectorsCount = 0
) => ({
  'Sites online': {
    count: siteOnlineCount.toLocaleString(),
    color: 'green',
    icon: 'buildingNew',
    type: t('plainText.connectivitySitesOnline'),
  },

  'Connection errors': {
    count: offlineConnectorsCount.toLocaleString(),
    color: 'red',
    icon: 'warning',
    type: t('plainText.connectivityConnectionErrors'),
  },
})
