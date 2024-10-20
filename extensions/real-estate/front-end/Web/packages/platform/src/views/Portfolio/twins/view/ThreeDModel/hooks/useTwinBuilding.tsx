/* eslint-disable camelcase */
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query'
import getTwinBuildingData from '../../../../../../services/ThreeDimensionModule/TwinBuildingDataService'
import { Module3d, Modules3d } from '../types'
import { TokenResponse } from '../../../../../../services/AutoDesk/AutoDeskService'

export type UseTwinBuildingParams = {
  siteId?: string
  autoDeskData?: TokenResponse
  options?: UseQueryOptions
}

export function useTwinBuilding({
  siteId,
  autoDeskData,
  options,
}: UseTwinBuildingParams) {
  const { t } = useTranslation()
  const twinBuildingContext = useQuery(
    ['twin-building', siteId, autoDeskData],
    () => {
      if (siteId != null) {
        return getTwinBuildingData({
          siteId,
          autoDeskData,
        })
      } else {
        return undefined
      }
    },
    {
      ...options,
      enabled: siteId != null && options?.enabled !== false,
    }
  ) as UseQueryResult<{
    isSiteBuilding3dModelLoaded: boolean
    siteBuilding3dData: Module3d
    floorModelData: Modules3d
  }>

  return useMemo(() => {
    const {
      isSiteBuilding3dModelLoaded,
      siteBuilding3dData,
      floorModelData = [],
    } = twinBuildingContext.data || {}

    const siteBuilding3dModel = isSiteBuilding3dModelLoaded
      ? [
          {
            ...siteBuilding3dData,
            isDefault: true,
            isUngroupedLayer: true,
            typeName: t('labels.site'),
          },
        ]
      : []

    const building3dModels = [
      ...siteBuilding3dModel,
      ...floorModelData,
    ] as Modules3d

    const building3dModelsIds = building3dModels
      .map((model) => model?.id)
      .join()

    const buildingDefaultUrns = building3dModels.map((model) => model.url)

    const is3dTabForBuildingEnabled =
      isSiteBuilding3dModelLoaded || floorModelData.length > 0

    return {
      ...twinBuildingContext,
      data: {
        building3dModels,
        building3dModelsIds,
        is3dTabForBuildingEnabled,
        siteBuilding3dModel,
        buildingDefaultUrns,
      },
    }
  }, [twinBuildingContext])
}
