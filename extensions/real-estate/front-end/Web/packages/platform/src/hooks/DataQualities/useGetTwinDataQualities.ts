import { useQuery, UseQueryOptions } from 'react-query'
import {
  getTwinDataQualities,
  TwinDataQualitiesResponse,
} from '../../services/DataQualities/DataQualities'

export default function useGetTwinDataQualities(
  { siteId, twinId }: { siteId: string; twinId: string },
  options?: UseQueryOptions<TwinDataQualitiesResponse, { statusCode?: number }>
) {
  return useQuery(
    ['twin-data-qualities', siteId, twinId],
    () => getTwinDataQualities({ siteId, twinId }),
    options
  )
}
