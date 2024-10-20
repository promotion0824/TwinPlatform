import { useQuery, UseQueryOptions } from 'react-query'
import _ from 'lodash'
import { api } from '@willow/ui'
import { Workgroup } from '@willow/common'

export default function useGetWorkgroupSelectors(
  options: UseQueryOptions<Workgroup[]>
) {
  return useQuery(
    ['workgroupSelectors'],
    async () => {
      const { data } = await api.get(`/management/workgroups/all`)
      return data
    },
    {
      // Array sorted alphanumerically by name.
      select: (data) => _.sortBy(data, 'name'),
      ...options,
    }
  )
}
