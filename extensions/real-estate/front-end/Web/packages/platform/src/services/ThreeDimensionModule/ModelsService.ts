import axios from 'axios'
import { getUrl } from '@willow/ui'
import { LayerGroupList, SortOrder } from './types'

export async function getModels(
  siteId: string,
  floorId: string
): Promise<LayerGroupList> {
  const getModelsUrl = getUrl(
    `/api/sites/${siteId}/floors/${floorId}/layerGroups`
  )
  return axios.get(getModelsUrl).then(({ data }) => data)
}

export async function getSortOrder(siteId: string): Promise<SortOrder> {
  const getSortOrderUrl = getUrl(
    `/api/sites/${siteId}/preferences/moduleGroups`
  )
  return axios.get(getSortOrderUrl).then(({ data }) => data)
}

export async function getModelsAndOrders(siteId: string, floorId: string) {
  return Promise.all([getModels(siteId, floorId), getSortOrder(siteId)])
    .then(([models, orders]) => ({
      initialModels: models,
      orders,
    }))
    .catch((e) => {
      throw new Error(e)
    })
}

export type ModelsAndOrders = {
  initialModels: LayerGroupList
  orders: SortOrder
}
