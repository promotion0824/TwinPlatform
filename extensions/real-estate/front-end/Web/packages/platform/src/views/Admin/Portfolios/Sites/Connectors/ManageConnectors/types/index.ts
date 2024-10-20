import { Dispatch, SetStateAction } from 'react'
import { UseQueryResult } from 'react-query'
import {
  ConnectorStat,
  Telemetries,
} from '../../../../../../../services/Connectors/ConnectorsService'
import { ConnectorTypes } from '../../../../../../../services/ConnectorTypes/ConnectorTypesService'
import {
  GetConnectorQueryType,
  Connector,
} from '../../../../../../../services/Connectivity/ConnectivityService'

export type UseParamsType = { siteId: string }

export type SetSelectedConnector = Dispatch<SetStateAction<ConnectorStat>>

export type ManageConnectorsContextType = {
  connectorId?: string
  selectedConnector?: Partial<ConnectorStat>
  setConnectorId: (connectorId?: string) => void

  connectorsStatsQuery: ConnectorsStatsQueryType
  connectorTypesData: ConnectorTypes

  connectorQuery: GetConnectorQueryType
  connectorDetails: ConnectorDetails
  invalidateConnectorQueries: () => void
}

export type SetShowAddConnectorModal = Dispatch<SetStateAction<boolean>>

export type ConnectorsStatsQueryType = UseQueryResult<ConnectorStat[]>

export type ConnectivityTableDataType = ConnectorStat[]

export type ConnectorDetails = Partial<Connector> & { telemetry: Telemetries }
