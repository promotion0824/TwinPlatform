import { useQuery, UseQueryOptions } from 'react-query'
import {
  getConnectorTypes,
  ConnectorTypes,
} from '../../services/ConnectorTypes/ConnectorTypesService'

export default function useGetConnectorTypes(
  siteId: string,
  options?: UseQueryOptions<ConnectorTypes>
) {
  return useQuery(
    ['connectorTypes', siteId],
    () => getConnectorTypes(siteId),
    options
  )
}
