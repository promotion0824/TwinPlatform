/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { createContext, useContext, useMemo } from 'react'
import { useParams } from 'react-router'
import { useQueryClient } from 'react-query'
import { useDateTime } from '@willow/ui'
import { ProviderRequiredError } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import {
  useGetSiteConnectorsStats,
  useGetConnectorTypes,
  useGetConnector,
} from '../../../../../../../hooks'
import {
  UseParamsType,
  ManageConnectorsContextType,
  ConnectorDetails,
} from '../types'
import { ConnectorStat } from '../../../../../../../services/Connectors/ConnectorsService'
import { ARCHIVED } from '../../../../Connectivity/utils'

const ManageConnectorsContext = createContext<
  ManageConnectorsContextType | undefined
>(undefined)

export function useManageConnectors() {
  const context = useContext(ManageConnectorsContext)
  if (context == null) {
    throw new ProviderRequiredError('ManageConnectors')
  }
  return context
}

export default function ManageConnectorsProvider({
  children,
}: {
  children: JSX.Element
}) {
  const { siteId } = useParams<UseParamsType>()
  const dateTime = useDateTime()
  const [searchParams, setSearchParams] = useMultipleSearchParams([
    { name: 'connectorId', type: 'string' },
  ])
  const { connectorId }: { connectorId?: string } = searchParams

  const connectorsStatsQuery = useGetSiteConnectorsStats(
    siteId,
    {
      start: dateTime.now().addDays(-2).format(),
      end: dateTime.now().format(),
    },
    {
      select: (data: ConnectorStat[]) =>
        data.filter((connector) => connector.currentSetState !== ARCHIVED),
    }
  )

  const selectedConnector = connectorsStatsQuery?.data?.find(
    (c: ConnectorStat) => c.connectorId === connectorId
  )

  const connectorQuery = useGetConnector(siteId, connectorId, {
    enabled: !!selectedConnector && connectorId != null,
  })

  const { data: connectorTypesData = [] } = useGetConnectorTypes(siteId)

  const connectorDetails = useMemo<ConnectorDetails>(() => {
    const connectorStat = connectorsStatsQuery?.data?.find(
      (c: ConnectorStat) => c.connectorId === connectorId
    )

    return {
      ...connectorQuery.data,
      telemetry: connectorStat?.telemetry || [],
      pointsCount:
        (connectorStat?.totalCapabilitiesCount ?? 0) -
          (connectorStat?.disabledCapabilitiesCount ?? 0) || 0,
    }
  }, [selectedConnector, connectorQuery.isSuccess, connectorQuery.isRefetching])

  const queryClient = useQueryClient()
  const invalidateConnectorQueries = () => {
    queryClient.invalidateQueries(['getSiteConnectorStats', siteId])
    queryClient.invalidateQueries([
      'connectivity-connector',
      siteId,
      connectorId,
    ])
  }

  return (
    <ManageConnectorsContext.Provider
      value={{
        connectorId,
        selectedConnector,
        setConnectorId: (connectorId: string) =>
          setSearchParams({ connectorId }),

        connectorsStatsQuery,
        connectorTypesData,

        connectorQuery,
        connectorDetails,

        invalidateConnectorQueries,
      }}
    >
      {children}
    </ManageConnectorsContext.Provider>
  )
}
