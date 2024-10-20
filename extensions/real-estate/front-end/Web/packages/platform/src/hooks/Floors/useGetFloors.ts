import { useQuery, UseQueryOptions } from 'react-query'
import { Floors, getFloors } from '../../services/Floors/FloorsService'

export default function useGetFloors(
  siteId: string,
  params,
  options?: UseQueryOptions<Floors>
) {
  return useQuery<Floors>(
    ['floors', siteId, params],
    () => getFloors(siteId, params),
    options
  )
}
