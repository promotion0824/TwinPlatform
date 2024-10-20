import { useQuery, UseQueryOptions } from 'react-query'
import {
  getSitesDataQualities,
  LocationDataQualitiesResponse,
} from '../../services/DataQualities/DataQualities'

export default function useGetSitesDataQualities(
  { customerId, portfolioId }: { customerId: string; portfolioId: string },
  options?: UseQueryOptions<
    LocationDataQualitiesResponse,
    { statusCode?: number }
  >
) {
  return useQuery(
    ['sites-data-qualities', customerId, portfolioId],
    () => getSitesDataQualities({ customerId, portfolioId }),
    options
  )
}
