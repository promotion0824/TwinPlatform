import { useQuery, UseQueryOptions } from 'react-query'
import { useUser, getUrl } from '@willow/ui'
import axios, { AxiosResponse } from 'axios'
import { Ontology } from './models'

export type ModelOfInterest = {
  id?: string
  modelId: string
  name: string
  color: string
  text?: string
  icon?: string
}

export const fileModelId = 'dtmi:com:willowinc:Document;1'
export const sensorModelId = 'dtmi:com:willowinc:Capability;1'
export const buildingModelId = 'dtmi:com:willowinc:Building;1'
export const assetModelId = 'dtmi:com:willowinc:Asset;1'
export const levelModelId = 'dtmi:com:willowinc:Level;1'

export const buildingModelOfInterest = {
  modelId: buildingModelId,
  name: 'Building',
  text: 'Bu',
  color: '#D9D9D9',
}

// Note: for these two models of interest, the names do not match the names of
// the models (which are "Document" and "Capability" respectively); this is
// intentional.
const fileModelOfInterest: ModelOfInterest = {
  modelId: fileModelId,
  name: 'File',
  icon: 'folder',
  color: '#7e7e7e',
}

const sensorModelOfInterest: ModelOfInterest = {
  modelId: sensorModelId,
  name: 'Sensor',
  icon: 'microchip',
  color: '#7e7e7e',
}

type Response = AxiosResponse<ModelOfInterest[]>
type Data = {
  items: ModelOfInterest[]
  etag: string
}

/**
 * Get the user's list of models of interest, with files and sensors appended.
 * Takes the standard React Query options, with one additional parameter
 * `includeExtras`. If this is true (default), files and sensors will be appended to
 * the returned list. Otherwise they won't.
 */
export function useModelsOfInterest({
  includeExtras = true,
  ...options
}: UseQueryOptions<Response, any, Data> & { includeExtras?: boolean } = {}) {
  const user = useUser()
  const customerId = user?.customer?.id

  const { enabled = true, ...rest } = options ?? {}

  return useQuery<Response, any, Data>(
    ['modelsOfInterest'],
    () => axios.get(getUrl(`/api/customers/${customerId}/modelsOfInterest`)),
    {
      select: (response) => {
        let items: ModelOfInterest[] | undefined
        if (includeExtras) {
          items = [...response.data, fileModelOfInterest, sensorModelOfInterest]
        } else {
          items = response.data
        }

        return {
          items,
          etag: response.headers.etag,
        }
      },
      enabled: customerId != null && enabled,
      ...rest,
    }
  )
}

/**
 * Return the model of interest for the given model in the ontology. That is,
 * the model in the `modelsOfInterest` structure above which is an ancestor of
 * the specified model. If there is none, return undefined.
 */
export function getModelOfInterest(
  modelId: string,
  ontology: Ontology,
  modelsOfInterest: ModelOfInterest[]
): ModelOfInterest | undefined {
  for (const ancestorId of ontology.getModelAncestors(modelId)) {
    const moi = modelsOfInterest.find((m) => m.modelId === ancestorId)
    if (moi) {
      return moi
    }
  }
  return undefined
}
