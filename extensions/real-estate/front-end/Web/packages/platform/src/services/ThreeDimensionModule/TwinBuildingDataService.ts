/* eslint-disable camelcase */
import _ from 'lodash'
import { get3dModule } from './ThreeDimensionModuleService'
import { getAutoDeskFileManifest } from '../AutoDesk/AutoDeskService'
import { getFloors } from '../Floors/FloorsService'
import { getModelsAndOrders } from './ModelsService'

export default async function getTwinBuildingData({
  siteId,
  autoDeskData,
}: {
  siteId: string
  autoDeskData?: {
    access_token?: string
    token_type?: string
  }
}) {
  try {
    const siteBuilding3dData = await get3dModule(siteId)
    const isModelExist =
      !!siteBuilding3dData?.url && !!autoDeskData?.access_token

    const accessToken = autoDeskData?.access_token
    const tokenType = autoDeskData?.token_type
    const authorization = `${tokenType} ${accessToken}`

    const autoDeskManifest = isModelExist
      ? await getAutoDeskFileManifest(siteBuilding3dData?.url, authorization)
      : undefined

    const floors = await getFloors(siteId)

    const siteFloor = floors.find((floor) => floor.isSiteWide)
    const floorModelsAndOrders =
      typeof siteFloor?.id === 'string'
        ? await getModelsAndOrders(siteId, siteFloor.id)
        : null

    const modules3d = floorModelsAndOrders?.initialModels?.modules3D || []
    const sortOrder3d = floorModelsAndOrders?.orders?.sortOrder3d || []
    const shouldSort = sortOrder3d.length > 0
    let models = modules3d
    // Layers will be named, grouped and sorted according to the names, disciplines and groups for the floors as configured in admin
    if (shouldSort) {
      models = _(modules3d)
        .orderBy((model) => sortOrder3d.indexOf(model.moduleTypeId))
        .value()
        .map((model) => ({
          ...model,
          // business requirement to turn all floor models isDefault off when building model exist
          isDefault: isModelExist ? false : model.isDefault,
        }))
    }

    return {
      isSiteBuilding3dModelLoaded:
        isModelExist &&
        (
          autoDeskManifest as {
            progress: string
          }
        )?.progress === 'complete',
      siteBuilding3dData,
      floorModelData: models,
    }
  } catch (error) {
    // rethrow whatever error happens in any of the service call in try block
    throw new Error(error)
  }
}
