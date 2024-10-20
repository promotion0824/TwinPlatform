import { filterMatchedGuids, findModelIndex } from './index'
import { DbGuidDicts, UrnIndexModelIdDict, Guid } from '../types'

export function aggregateSelectionChangedListener(dbIdGuidDicts, onClick) {
  return ({ selections }) => {
    if (selections.length === 0) return

    const [selection] = selections
    const { dbIdArray, model } = selection
    const guids = dbIdArray.length
      ? filterMatchedGuids(dbIdArray, dbIdGuidDicts[model.id])
      : null
    onClick({ guids })
  }
}

export function objectUnderMouseChangedListener(
  dicts: {
    dbIdGuidDicts: DbGuidDicts
    urnIndexModelIdDict: UrnIndexModelIdDict
  },
  onMouseHoverChange: (guid: Guid, modelIndex: number) => void,
  callback: (isMouseOverOn: boolean) => void
) {
  return (e) => {
    const { dbId, modelId, target } = e
    const { dbIdGuidDicts, urnIndexModelIdDict } = dicts

    const isModelShown = target.impl?.model
    if (!isModelShown) return
    const targetModelId = modelId === -1 ? target.impl.model.id : modelId
    const modelIndex = findModelIndex(dbId, targetModelId, urnIndexModelIdDict)

    callback(dbId !== -1)

    const [guid] = filterMatchedGuids([dbId], dbIdGuidDicts[targetModelId])
    onMouseHoverChange(guid, modelIndex)
  }
}

export function mouseMoveListener(
  isMouseOverOn,
  onMousePositionChange,
  positionPadding
) {
  return ({ offsetX, offsetY }) => {
    if (isMouseOverOn) {
      const x = offsetX + positionPadding.x
      const y = offsetY + positionPadding.y
      onMousePositionChange({ x, y })
    } else {
      onMousePositionChange(null)
    }
  }
}
