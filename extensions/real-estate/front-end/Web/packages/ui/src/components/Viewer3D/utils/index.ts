import getGuid from '../../../utils/getGuid'
import {
  DbGuidDict,
  DbId,
  DbIdPropertiesDict,
  Guid,
  LayerInfo,
  MousePosition,
  UrnIndexModelIdDict,
  LayerPriority,
} from '../types'

export function getGuids(model: Autodesk.Viewing.Model, dbIds: DbId[]) {
  return Promise.all(
    dbIds.map((dbId: number) =>
      getGuid(model, dbId).then(({ guid }: { guid: Guid }) => guid)
    )
  )
}

export function filterMatchedGuids(
  dbIds: number[],
  dbIdGuidDict: DbGuidDict = {}
) {
  return Object.entries(dbIdGuidDict)
    .filter(([matchingDbId]) =>
      dbIds.find((dbId: number) => dbId === Number(matchingDbId))
    )
    .map(([_, guid]) => guid)
}

export function getDbIdGuidDict(dbIds: DbId[], guids: Guid[]) {
  const dict: DbGuidDict = {}
  dbIds.forEach((dbId, i) => {
    dict[dbId] = guids[i]
  })
  return dict
}

export function findKeysByValue<Key extends keyof Value, Value>(
  dict: Record<Key, Value>,
  value: Value
) {
  return Object.entries(dict)
    .filter(([_, v]) => v === value)
    .map(([k]) => k)
}

export function findModel(id: number, models: any) {
  return models.find((currModel) => currModel.id === id)
}

export function findModelIndex(
  dbId: number,
  modelId: number,
  urnIndexModelIdDict: UrnIndexModelIdDict
) {
  if (dbId === -1) return -1
  if (modelId === -1) return 0

  const urnEntry = Object.entries(urnIndexModelIdDict).find(
    ([_, mId]) => mId === modelId
  )
  if (!urnEntry) {
    return -1
  }
  return Number(urnEntry[0])
}

export function isPriorityValid(priority: LayerPriority) {
  return priority < 4 && priority >= 0
}

export function getDbIdPropertyGuidDict(
  dbIdPropertiesDict: DbIdPropertiesDict
) {
  const dbIdPropertyGuidDict = {}
  Object.entries(dbIdPropertiesDict).forEach(([dbId, properties]) => {
    const guidProp = properties.find(
      (prop) => prop.displayName === 'GUID' && !!prop.displayValue
    )
    if (guidProp) {
      dbIdPropertyGuidDict[dbId] = guidProp.displayValue
    }
  })
  return dbIdPropertyGuidDict
}

export function updateTooltipStyles(
  $el: HTMLElement,
  options: { mousePosition?: MousePosition; display: boolean }
) {
  const { mousePosition, display } = options
  if (mousePosition) {
    const { x, y } = mousePosition
    $el.style.left = `${x}px`
    $el.style.top = `${y}px`
  }
  $el.style.display = display ? 'flex' : 'none'
}

export function getPriority(
  layers: LayerInfo[],
  modelIndex?: number,
  guid?: Guid
) {
  return modelIndex != null &&
    guid != null &&
    layers[modelIndex] &&
    layers[modelIndex][guid]
    ? layers[modelIndex][guid]?.priority
    : -1
}

export function getLayerName(
  layers: LayerInfo[],
  modelIndex: number,
  guid: Guid
) {
  return layers[modelIndex] && layers[modelIndex][guid]
    ? layers[modelIndex][guid]?.name
    : undefined
}

export function getDefaultColor(priority: LayerPriority) {
  const colors = {
    red: new THREE.Vector4(252 / 255, 45 / 255, 59 / 255, 1),
    orange: new THREE.Vector4(252 / 255, 98 / 255, 0 / 255, 1),
    yellow: new THREE.Vector4(1, 193 / 255, 26 / 255, 1),
    blue: new THREE.Vector4(65 / 255, 124 / 255, 191 / 255, 1),
  }

  switch (priority) {
    case 1:
      return colors.red
    case 2:
      return colors.orange
    case 3:
      return colors.yellow
    default:
      throw new Error(`Priority ${priority} does not match the color`)
  }
}
