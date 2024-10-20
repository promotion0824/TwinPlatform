import { Insight } from '@willow/common/insights/insights/types'

interface ObjectProperty {
  displayName: string
  displayValue: string
}

export type DbIdPropertiesDict = Record<string, ObjectProperty[]>

export type LayerPriority = Insight['priority'] | number

type Layer = Pick<Insight, 'floorCode' | 'name'> & {
  priority: LayerPriority
}

export type LayerInfo = Record<string, Layer>

export interface MousePosition {
  x: number
  y: number
}

export const URN_PREFIX = 'urn:'

export type UrnIndex = number

export interface SelectOption {
  type: 'object' | 'asset'
  urnIndex: UrnIndex
  guid: string
}

export interface ViewerControls {
  reset: () => void
  select: (selectOption: SelectOption) => void
  showModel: (urnIndex: UrnIndex) => void
  hideModel: (urnIndex: UrnIndex) => void
}

export type ViewerCb = (
  viewer: Autodesk.Viewing.Viewer3D,
  viewerControls: ViewerControls,
  innerViewerControls: {
    updateLayersByUrnIndex: (urnIndex: UrnIndex, layer: LayerInfo) => void
  }
) => void

export type ModelStatus = 'success' | 'loading' | 'failure' | 'idle'
export interface LoadModelCallbacks {
  onDocumentLoadSuccess: (doc: any) => void
  onDocumentLoadFailure: (errorCode: any, errorMsg: string) => void
}

export type Urn = string

export type LoadModelFn = (
  urnIndex: UrnIndex,
  urns: Urn[],
  loadModelCallbacks: LoadModelCallbacks,
  onModelLoadChage: (urnIndex: UrnIndex, status: ModelStatus) => void
) => void

export type ModelId = number

export type UrnIndexModelIdDict = Record<UrnIndex, ModelId>

export type DbId = number

export type Guid = string

export type PropertyGuid = string

export type DbGuidDict = Record<DbId, Guid>

export type DbGuidDicts = Record<ModelId, DbGuidDict>

export type DbPropertyGuidDict = Record<DbId, PropertyGuid>

export type DbPropertyGuidDicts = Record<ModelId, DbPropertyGuidDict>

export interface SelectDictReferences {
  urnIndexModelIdDict: UrnIndexModelIdDict
  dbIdGuidDicts: DbGuidDicts
  dbIdPropertyGuidDicts: DbPropertyGuidDicts
}

export type ViewerSelectFn = (
  viewer: Autodesk.Viewing.Viewer3D,
  ids: DbId[],
  model: any
) => void
