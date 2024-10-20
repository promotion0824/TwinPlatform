import { useMemo } from 'react'
import { Modules3d } from '../types'
import useGetSortedModels from './useGetSortedModels'

export type UseTwinLevelParam = {
  siteId: string
  floorId: string
  dependencies: boolean
}

export default function useTwinLevel({
  siteId,
  floorId,
  dependencies = true,
}: UseTwinLevelParam) {
  const sortedModelsContext = useGetSortedModels({
    siteId,
    floorId,
    options: {
      enabled: dependencies,
    },
  })

  return useMemo(
    () => ({
      ...sortedModelsContext,
      data: {
        levelModels: sortedModelsContext.data as Modules3d,
        levelModelIds: sortedModelsContext?.data
          .map((model) => model.id)
          .join(),
        levelDefaultUrns: sortedModelsContext?.data.map((model) => model.url),
        is3dTabForLevelEnabled: sortedModelsContext?.data.length > 0,
      },
    }),
    [sortedModelsContext]
  )
}
