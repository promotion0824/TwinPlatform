import { isPriorityValid, findKeysByValue, findModel } from './index'
import {
  DbId,
  DbIdPropertiesDict,
  LayerInfo,
  DbGuidDict,
  MousePosition,
  SelectDictReferences,
  ViewerSelectFn,
  SelectOption,
  UrnIndexModelIdDict,
  LoadModelFn,
  Urn,
  LoadModelCallbacks,
  UrnIndex,
  URN_PREFIX,
  ModelStatus,
  DbGuidDicts,
  LayerPriority,
} from '../types'

export function viewerReset(viewer: Autodesk.Viewing.Viewer3D, viewerState) {
  viewer.clearSelection()
  viewer.restoreState(viewerState)
}

export function viewerSelect(
  viewer: Autodesk.Viewing.Viewer3D,
  dbIds: DbId[],
  model: Autodesk.Viewing.Model
) {
  viewer.fitToView(dbIds, model)
  viewer.select(dbIds, model)
}

function updateColor(
  viewer: Autodesk.Viewing.Viewer3D,
  dbId: DbId,
  priority: LayerPriority,
  getColor: (priority: LayerPriority) => THREE.Vector4
) {
  if (isPriorityValid(priority)) {
    const color = getColor(priority)
    viewer.setThemingColor(dbId, color)
  }
}

export function updateMaterial(
  dbId: DbId,
  instanceTree: Autodesk.Viewing.InstanceTree,
  fragList: Autodesk.Viewing.Private.FragmentList
) {
  instanceTree.enumNodeFragments(
    dbId,
    (fragId: number) => {
      const mesh = fragList.getVizmesh(fragId)
      if (mesh.material) {
        mesh.material.opacity = 0.5
        mesh.material.transparent = true
        mesh.material.needsUpdate = true
      }
    },
    true
  )
}

/**
 * Autodesk forge viewer only accepts function name userFunction for property data access
 * Function name here should not be changed
 * Reference: https://forge.autodesk.com/en/docs/viewer/v7/developers_guide/advanced_options/propdb-queries/
 */
export function userFunction(
  propertyDb: Autodesk.Viewing.PropDbLoader & {
    enumObjects: (callback: (dbId: number) => void) => void
    getObjectProperties: (dbId: number) => {
      dbId: number
      properties: Array<{
        displayName: string
        displayValue: string
        displayCategory: string
        type: number
        units: null
        hidden: boolean
        precision: number
      }>
      externalId: string
      name: string
    }
  }
): DbIdPropertiesDict {
  const dict = {}
  propertyDb.enumObjects((dbId: number) => {
    const objectProperties = propertyDb.getObjectProperties(dbId)
    dict[dbId] = objectProperties.properties
  })
  return dict
}

export async function executeUserFunction(propertyDb, userFn, logger?: any) {
  let response
  try {
    response = await propertyDb.executeUserFunction(userFn)
  } catch (e) {
    logger?.error(e)
    throw new Error(e)
  }
  return response
}

export function getDbIdPropertiesDict(
  propertyDb: Autodesk.Viewing.PropDbLoader,
  logger?: any
): Promise<DbIdPropertiesDict> {
  return executeUserFunction(propertyDb, userFunction, logger)
}

export function updateLayers(
  viewer: Autodesk.Viewing.Viewer3D,
  model: Autodesk.Viewing.Model,
  layers: LayerInfo,
  dbIdGuidDict: DbGuidDict,
  getColor: (priority: LayerPriority) => THREE.Vector4
) {
  const instanceTree = model.getInstanceTree()
  const fragList = model.getFragmentList()

  Object.entries(dbIdGuidDict).forEach(([dbId, guid]) => {
    const dbIdNum = Number(dbId)
    updateMaterial(dbIdNum, instanceTree, fragList)
    const layer = layers[guid]
    if (layer) {
      updateColor(viewer, dbIdNum, layer.priority, getColor)
    }
  })
  viewer.impl.invalidate(true)
}

export function updateViewerCursor(
  autodeskViewer: Autodesk.Viewing.Viewer3D,
  mousePosition?: MousePosition
) {
  if (mousePosition) {
    autodeskViewer.canvas.style.cursor = 'pointer'
  } else {
    autodeskViewer.canvas.style.cursor = 'auto'
  }
}

export function getViewerResetFn(viewer, viewerResetFn) {
  const viewerState = viewer.viewerState?.getState({
    viewport: true,
  })
  return () => viewerResetFn(viewer, viewerState)
}

export function getSelectFn(
  viewer: Autodesk.Viewing.Viewer3D,
  selectDictReferences: SelectDictReferences,
  viewerSelectFn: ViewerSelectFn
) {
  return ({ type, urnIndex, guid }: SelectOption) => {
    const { urnIndexModelIdDict, dbIdGuidDicts, dbIdPropertyGuidDicts } =
      selectDictReferences
    const urnModelId = urnIndexModelIdDict[urnIndex]
    const dict = type === 'object' ? dbIdGuidDicts : dbIdPropertyGuidDicts
    if (!dict[urnModelId]) {
      throw new Error(`Data for model id ${urnModelId} in ${type} type dict`)
    }
    const ids = findKeysByValue(dict[urnModelId], guid).map((id) => Number(id))

    const targetModel = findModel(urnModelId, viewer.getAllModels())
    viewerSelectFn(viewer, ids, targetModel)
  }
}

export function getShowModelFn(
  viewer: Autodesk.Viewing.Viewer3D,
  urnIndexModelIdDict: UrnIndexModelIdDict,
  loadModelFn: LoadModelFn,
  urns: Urn[],
  loadCallbacks: LoadModelCallbacks,
  onModelLoadChange: (urnIndex: UrnIndex, status: ModelStatus) => void
) {
  return (urnIndex: UrnIndex) => {
    const urnModelId = urnIndexModelIdDict[urnIndex]
    if (!urnModelId) {
      loadModelFn(urnIndex, urns, loadCallbacks, onModelLoadChange)
      onModelLoadChange(urnIndex, 'loading')
    } else {
      const isModelDisplayed = viewer.showModel(urnModelId, false)
      if (!isModelDisplayed) {
        throw new Error(`Failed to show a model by model id : ${urnModelId}`)
      }
    }
  }
}

export function getHideModelFn(
  viewer: Autodesk.Viewing.Viewer3D,
  urnIndexModelIdDict: UrnIndexModelIdDict
) {
  return (urnIndex: UrnIndex) => {
    const urnModelId = urnIndexModelIdDict[urnIndex]
    const isModelHidden = viewer.hideModel(urnModelId)
    if (!isModelHidden) {
      throw new Error(`Failed to hide a model by model id : ${urnModelId}`)
    }
  }
}

/**
 * Provide a functionality to update each layer on a model.
 * urn index indicates the order of urns prop and find the model by urn index
 */
export function getUpdateLayersByUrnIndexFn(
  viewer: Autodesk.Viewing.Viewer3D,
  urnIndexModelIdDict: UrnIndexModelIdDict,
  dbIdGuidDicts: DbGuidDicts,
  getColor: (priority: LayerPriority) => THREE.Vector4
) {
  return (urnIndex, layers) => {
    const modelId = urnIndexModelIdDict[urnIndex]
    const model = viewer.getAllModels().find((m) => m.id === modelId)
    if (
      Object.keys(layers).length > 0 &&
      Object.keys(dbIdGuidDicts).length > 0 &&
      model != null
    ) {
      updateLayers(viewer, model, layers, dbIdGuidDicts[modelId], getColor)
    }
  }
}

export function loadModel(autodeskDocumentLoad) {
  return (urnIndex, urns, { onDocumentLoadSuccess, onDocumentLoadFailure }) => {
    const formattedUrn = `${URN_PREFIX}${urns[urnIndex]}`
    autodeskDocumentLoad(
      formattedUrn,
      onDocumentLoadSuccess(urnIndex),
      onDocumentLoadFailure(urnIndex)
    )
  }
}
