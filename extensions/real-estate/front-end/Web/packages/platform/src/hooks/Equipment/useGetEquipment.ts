import { api } from '@willow/ui'
import { useQuery } from 'react-query'
import _ from 'lodash'

const useGetEquipment = (siteId: string, equipmentId: string, options = {}) =>
  useQuery(
    ['equipments', siteId, equipmentId],
    async () => {
      const response = await api.get(
        `/sites/${siteId}/equipments/${equipmentId}`
      )
      return response.data
    },
    {
      enabled: !!siteId && !!equipmentId,
      select: (data) => {
        const atLeastOneEquipmentPointOn = (data.points ?? []).some(
          (p) => p?.hasFeaturedTags === true
        )

        //  - turn on those equipment points with "hasFeaturedTags" equals to true
        //  - if there is no equipment point with "hasFeaturedTags" equals to true
        //  - turn on the first 3 equipment points
        // as per business logic from: https://dev.azure.com/willowdev/Unified/_workitems/edit/87838
        return {
          ...data,
          // Be defensive against duplicate point IDs returned by the server.
          points: _.uniqBy<{
            hasFeaturedTags?: boolean
            defaultOn?: boolean
          }>(data.points, 'id').map((p, index) => ({
            ...p,
            defaultOn: atLeastOneEquipmentPointOn
              ? p.hasFeaturedTags
              : index < 3,
          })),
        }
      },
      ...options,
    }
  )

export default useGetEquipment
