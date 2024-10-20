import { api } from '@willow/ui'
import _ from 'lodash'
import { useQuery } from 'react-query'

export type LiveDataPoint = {
  liveDataValue?: string | number
  liveDataTimestamp?: string
  unit: string
}

export type LiveDataPoints = {
  [liveDataId: string]: LiveDataPoint
}

type LiveData = {
  liveDataPoints: LiveDataPoint[]
}

const useLiveDataPoints = (siteId?: string, assetId?: string) =>
  useQuery<LiveData, unknown, LiveDataPoints>(
    ['pinOnLayer', 'allPoints', siteId, assetId],
    async () => {
      const response = await api.get(
        `/sites/${siteId}/assets/${assetId}/pinOnLayer`,
        {
          params: { includeAllPoints: true },
        }
      )
      return response.data
    },
    {
      enabled: !!siteId && !!assetId,
      select: (data: LiveData) => _.keyBy(data.liveDataPoints, 'id'),
    }
  )

export default useLiveDataPoints
