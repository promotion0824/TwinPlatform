import { ViewerControls } from '@willow/ui/components/Viewer3D/types'
import { Dispatch } from 'react'
import { ModelInfo } from '@willow/common/twins/view/models'
import { RenderDropdownObject } from './Dropdown/types'
import { TwinWithIds } from '../RelationsMap/types'

type ModuleGroup = {
  id: string
  name: string
  sortOrder: number
  siteId: string
}

export type Module3d = {
  id: string
  name: string
  visualId: string
  url: string
  sortOrder: number
  canBeDeleted: boolean
  isDefault: boolean
  typeName: string
  groupType: string
  moduleTypeId: string
  moduleGroup: ModuleGroup
  isUngroupedLayer?: boolean
  isEnabled?: boolean
}

export type Modules3d = Module3d[]

export type SortOrder3d = string[]

export type ThreeDModelTabProps = {
  siteID?: string
  modelInfo: ModelInfo
  twin: TwinWithIds
  asset: {
    id: string
    moduleTypeNamePath?: string
    forgeViewerModelId?: string
    floorId: string
  }
  geometrySpatialReference?: string
}

export type ThreeDModelContentProps = {
  renderDropdownObject: RenderDropdownObject
  toggleDropdownLayer: (
    sectionName: string,
    layerName: string,
    isUngroupedLayer: boolean
  ) => void
  urns: string[]
  autoDeskToken: string
  setShown: Dispatch<boolean>
  isShown: boolean
  handleInitControls: (control: ViewerControls) => void
  defaultDisplayUrnIndices?: number[]
}
