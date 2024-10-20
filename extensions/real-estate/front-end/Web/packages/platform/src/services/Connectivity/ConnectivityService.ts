import { UseQueryResult } from 'react-query'
import axios from 'axios'
import { getUrl } from '@willow/ui'

export function getConnector(
  siteId: string,
  connectorId: string
): Promise<Connector> {
  const getConnectorUrl = getUrl(
    `/api/sites/${siteId}/connectors/${connectorId}`
  )
  return axios.get(getConnectorUrl).then(({ data }) => data)
}

export type GetConnectorQueryType = UseQueryResult<Connector>

export type Connectors = Connector[]

export type Connector = {
  id: string
  name?: string
  siteId: string
  configuration?: string
  connectorTypeId: string
  errorThreshold: number
  isEnabled: boolean
  isLoggingEnabled: boolean
  connectorType?: string
  pointsCount: number
  isArchived: boolean
  status: ServiceStatus
}

type ServiceStatus =
  | 'notOperational'
  | 'offline'
  | 'online'
  | 'onlineWithErrors'
  | 'archived'

export function getSiteEquipments(siteId: string): Promise<Equipments> {
  const getSiteEquipmentsUrl = getUrl(
    `/api/connectivity/sites/${siteId}/equipments`
  )
  return axios.get(getSiteEquipmentsUrl).then(({ data }) => data)
}

type Point = { name: string }
type Property = { displayName: string; value: string }
type Equipment = {
  equipmentId: string
  floorCode: string
  hasLiveData: boolean
  id: string
  identifier: string
  isEquipmentOnly: boolean
  name?: string
  pointTags: Point[]
  properties: Property[]
  tags?: string[]
}
export type Equipments = Equipment[]
