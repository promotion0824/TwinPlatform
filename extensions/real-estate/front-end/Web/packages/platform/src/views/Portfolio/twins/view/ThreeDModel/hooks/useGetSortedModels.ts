import _ from 'lodash'
import { useMemo } from 'react'
import { useQuery, UseQueryOptions } from 'react-query'
import {
  getModelsAndOrders,
  ModelsAndOrders,
} from '../../../../../../services/ThreeDimensionModule/ModelsService'

type ModelsAndOrdersProp = {
  siteId: string
  floorId: string
  options?: UseQueryOptions
}

export default function useGetSortedModels({
  siteId,
  floorId,
  options,
}: ModelsAndOrdersProp) {
  const modelsAndOrdersContext = useQuery(
    ['sorted-models', siteId, floorId],
    () => getModelsAndOrders(siteId, floorId),
    options
  )

  return useMemo(() => {
    const { initialModels, orders } =
      (modelsAndOrdersContext.data as ModelsAndOrders) || {}
    const modules3d = initialModels?.modules3D || []
    const sortOrder3d = orders?.sortOrder3d || []
    const shouldSort = sortOrder3d.length > 0
    let models = modules3d
    // Layers will be named, grouped and sorted according to the names, disciplines and groups for the floors as configured in admin
    if (shouldSort) {
      models = _(modules3d)
        .orderBy((model) => sortOrder3d.indexOf(model.moduleTypeId))
        .value()
    }

    return {
      ...modelsAndOrdersContext,
      data: models,
    }
  }, [modelsAndOrdersContext])
}
