import { getUrl } from '@willow/ui'
import axios from 'axios'

export function getFloors(
  siteId: string,
  params: { hasBaseModule?: boolean } = { hasBaseModule: false }
): Promise<Floors> {
  const getFloorsUrl = getUrl(`/api/sites/${siteId}/floors`)
  return axios
    .get(getFloorsUrl, {
      params,
    })
    .then(({ data }) => data)
}

export type Floors = Floor[]

type Floor = {
  id: string
  name: string
  code: string
  geometry: string
  modelReference?: string
  isSiteWide?: boolean
}
