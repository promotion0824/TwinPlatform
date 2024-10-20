import _ from 'lodash'
import { Urn, ViewerControls } from '@willow/ui/components/Viewer3D/types'
import { RenderDropdownObject, RenderLayer } from './types'
import { Modules3d } from '../types'

// Construct render dropdown object for displaying the toggling layers dropdown.
export function constructRenderDropdownObject(
  sortedModules3d: Modules3d,
  moduleTypeNamePath?: string
): RenderDropdownObject {
  const layers = sortedModules3d

  const renderObject = {} as RenderDropdownObject

  for (const layer of layers) {
    const { groupType, typeName, isDefault, isUngroupedLayer } = layer
    if (isUngroupedLayer) {
      renderObject[typeName] = {
        ...layer,
        isUngroupedLayer,
        isEnabled: isDefault,
      }
    } else {
      if (!renderObject[groupType]) {
        renderObject[groupType] = {}
      }
      renderObject[groupType][typeName] = {
        ...layer,
        isEnabled:
          // the utilization of moduleTypeNamePath is relevant to only asset twin
          typeof moduleTypeNamePath === 'string' && moduleTypeNamePath !== ''
            ? moduleTypeNamePath
                .toLocaleLowerCase()
                .split(',')
                .includes(typeName?.toLocaleLowerCase())
            : isDefault,
      }
    }
  }

  return renderObject
}

// Calculate the number of enabled layers in RenderDropdownObject
export function getEnabledLayersCount(
  renderDropdownObject: RenderDropdownObject
): number {
  return Object.values(renderDropdownObject).reduce((total, layers) => {
    if (layers.isUngroupedLayer) {
      return layers.isEnabled ? total + 1 : total
    }
    return (
      total +
      Object.values(layers).filter((layer: RenderLayer) => layer.isEnabled)
        .length
    )
  }, 0)
}

// Viewer3D accepts urns and defaultDisplayUrnIndices as input params
// where urns is the array of all model urns(urls) while defaultDisplayUrnIndices
// represents the indices of the urns gets loaded when Viewer3D is initialized
export function constructDefaultIndices({
  renderDropdownObject,
  urns,
}: {
  renderDropdownObject: RenderDropdownObject
  urns: string[]
}): number[] {
  const indices: number[] = []

  Object.values(renderDropdownObject).forEach((section) => {
    if (
      section.isUngroupedLayer &&
      section.isEnabled &&
      urns.indexOf(section.url) !== -1
    ) {
      indices.push(urns.indexOf(section.url))
    }
    Object.values(section).forEach((model: RenderLayer) => {
      if (model.isEnabled && urns.indexOf(model.url) !== -1) {
        indices.push(urns.indexOf(model.url))
      }
    })
  })

  return indices
}

export function toggleModel({
  urns,
  modelUrn,
  viewerControls,
  isEnable,
}: {
  urns: Urn[]
  modelUrn?: Urn
  viewerControls?: ViewerControls
  isEnable?: boolean
}) {
  const urnIndex = modelUrn ? urns.indexOf(modelUrn) : -1

  if (
    typeof modelUrn === 'string' &&
    typeof isEnable === 'boolean' &&
    urnIndex !== -1 &&
    typeof viewerControls?.showModel === 'function' &&
    typeof viewerControls?.hideModel === 'function'
  ) {
    if (isEnable) {
      viewerControls.showModel(urnIndex)
    } else {
      viewerControls.hideModel(urnIndex)
    }
  }
}
