import { api } from '@willow/ui'
import { useQuery } from 'react-query'

const useSensorPoint = (url, params, { enabled, onError }) => {
  const {
    data: sensorPoint,
    isLoading,
    isError,
  } = useQuery(
    ['points', url, params],
    async () => {
      const response = await api.get(url, {
        params,
      })
      return response.data
    },
    {
      enabled,
      onError,
    }
  )

  return { sensorPoint, isLoading, isError }
}

export default useSensorPoint
