import _ from 'lodash'
import { useMemo } from 'react'
import { UseQueryOptions } from 'react-query'
import { Module3d, Modules3d } from '../types'
import useGetSortedModels from './useGetSortedModels'

type ModelsAndOrdersProp = {
  siteId: string
  floorId: string
  moduleTypeNamePath?: string
  options?: UseQueryOptions
}

export default function useTwinAsset({
  siteId,
  floorId,
  moduleTypeNamePath,
  options,
}: ModelsAndOrdersProp) {
  const sortedModelsContext = useGetSortedModels({
    siteId,
    floorId,
    options,
  })

  return useMemo(() => {
    const modules3d = sortedModelsContext.data as Modules3d
    const moduleNamesToTurnOn =
      typeof moduleTypeNamePath === 'string' && moduleTypeNamePath !== ''
        ? moduleTypeNamePath.toLocaleLowerCase().split(',')
        : []
    let models = modules3d
    // defaultAssetModule will be used for 3dViewer to highlight and zoom in
    const defaultAssetModule: Module3d | undefined = models.find(
      (module: Module3d) =>
        // TODO: https://dev.azure.com/willowdev/Unified/_workitems/edit/61584
        // to determin whether to rely on twin?.geometrySpatialReference instead of asset.moduleTypeNamePath
        module.typeName != null &&
        moduleNamesToTurnOn.includes(module.typeName.toLocaleLowerCase())
    )

    // business requirement to filter out models that are not related to the asset
    models = _(modules3d)
      .filter(
        (model) =>
          (model.typeName != null &&
            moduleNamesToTurnOn.includes(model.typeName.toLocaleLowerCase())) ||
          model.isDefault
      )
      .value()

    return {
      ...sortedModelsContext,
      data: {
        assetModels: models,
        assetModelIds: models?.map((model) => model.id).join(),
        defaultAssetModule,
        is3dTabForAssetEnabled: models.length > 0 && !!defaultAssetModule?.id,
        assetDefaultUrns: models.map((module3d: Module3d) => module3d?.url),
      },
    }
  }, [sortedModelsContext, moduleTypeNamePath])
}
