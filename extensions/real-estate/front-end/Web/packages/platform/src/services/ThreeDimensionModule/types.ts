// types come from: https://wil-dev-plt-aue1-portalxl.azurewebsites.net/swagger/index.html
export type LayerGroupList = {
  floorId: string
  floorName: string
  layerGroups: LayerGroup[]
  layerGroups2D: LayerGroup[]
  layerGroups3D: LayerGroup[]
  modules2D: LayerGroupModule[]
  modules3D: LayerGroupModule[]
}

export type SortOrder = {
  sortOrder2d: string[] // where each sorted id is moduleTypeId
  sortOrder3d: string[]
}

type LayerGroupZone = {
  id: string
  geometry?: number[][]
  equipmentIds?: string[]
}

type LayerGroupLayer = {
  id: string
  name?: string
  tagName?: string
}

type LayerGroupEquipment = {
  id: string
  geometry?: number[][]
  name?: string
  hasLiveData: boolean
  tags?: {
    name?: string
    feature?: string
  }[]
  pointTags?: {
    name?: string
    feature?: string
  }[]
  hasInsights: boolean
  priority: number
}

type LayerGroup = {
  id: string
  name?: string
  is3D: boolean
  zones?: LayerGroupZone[]
  layers?: LayerGroupLayer[]
  equipments?: LayerGroupEquipment[]
}

// a single Model, a single .nwd file
export type LayerGroupModule = {
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
  moduleGroup: {
    id: string
    name: string
    sortOrder: number
    siteId: string
  }
}
