import axios from 'axios'
import { getUrl } from '@willow/ui'

type ConnectorTypeColumn = { name?: string; type?: string; isRequired: boolean }
type ConnectorType = {
  id: string
  name: string
  columns?: ConnectorTypeColumn[]
}
export type ConnectorTypes = ConnectorType[]

export function getConnectorTypes(siteId: string): Promise<ConnectorTypes> {
  const getConnectorTypesUrl = getUrl(`/api/sites/${siteId}/connectorTypes`)

  return axios.get(getConnectorTypesUrl).then(({ data }) => data)
}
