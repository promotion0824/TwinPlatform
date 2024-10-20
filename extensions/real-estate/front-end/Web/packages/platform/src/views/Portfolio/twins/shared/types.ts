import { Json } from '@willow/common/twins/view/twinModel'

/**
 * A twin returned by the GET /sites/{siteId}/twins/{twinId}.
 */
export type TwinResponse = {
  id: string
  siteID: string
  metadata: {
    modelId: string
    [key: string]: Json
  }
  [key: string]: Json
}
