import { api } from '@willow/ui'
import { useQuery } from 'react-query'
import _ from 'lodash'

export type SensorPoint = {
  name: string
  externalId: string
  trendId: string
  connectorName: string
  properties: {
    siteID: { value: string }
    [key: string]: { value: string }
  }
  device?: {
    id: string
    name: string
  }
}

const useGroupedSensors = (siteId?: string, twinId?: string) =>
  useQuery<
    SensorPoint[],
    unknown,
    { [deviceConnectorId: string]: SensorPoint[] }
  >(
    ['sensorPoints', siteId, twinId],
    async () => {
      const response = await api.get(`/sites/${siteId}/twins/${twinId}/points`)
      return response.data
    },
    {
      enabled: !!siteId && !!twinId,
      select: (data: SensorPoint[]) =>
        // Grouped by device_connector pair.
        _.groupBy(data, (point) => {
          // Points will always have associated connector, and may not have device (hostedBy twin)
          const connectorId = point.properties.connectorID?.value || ''
          const deviceId = point?.device?.id || ''
          return `${deviceId}_${connectorId}`
        }),
    }
  )

export default useGroupedSensors
