import { useQuery, UseQueryOptions } from 'react-query'
import {
  getFloorsDataQualities,
  LocationDataQualitiesResponse,
} from '../../services/DataQualities/DataQualities'

export default function useGetFloorsDataQualities(
  { siteId, systemId }: { siteId: string; systemId?: string },
  options?: UseQueryOptions<
    LocationDataQualitiesResponse,
    { statusCode?: number }
  >
) {
  return useQuery(
    ['floors-data-qualities', siteId, systemId],
    () => getFloorsDataQualities({ siteId, systemId }),
    options
  )
}
