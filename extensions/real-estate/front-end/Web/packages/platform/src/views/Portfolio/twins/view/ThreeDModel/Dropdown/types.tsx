import { ReactNode, MutableRefObject } from 'react'
import { Module3d } from '../types'

export type SectionName = string
export type LayerName = string

export type RenderLayer = Module3d

export type RenderSection = Record<string, RenderLayer>
export type RenderDropdownObject = RenderSection | Record<string, RenderSection>

export type ToggleDropdownLayerType = (
  sectionName: SectionName,
  layerName: LayerName,
  isUngroupedLayer?: boolean
) => void

export type DropdownProps = {
  tabHeaderRef: MutableRefObject<HTMLDivElement | null>
  isShown: boolean
  setShown: (shown: boolean) => void
  dropdownContent: ReactNode
}

type SectionProps = {
  sectionName: SectionName
}

export type DropdownContentProps = {
  renderDropdownObject: RenderDropdownObject
  toggleDropdownLayer: ToggleDropdownLayerType
  $isLoading: boolean
}

export type LayersProps = SectionProps & DropdownContentProps

export type LayerProps = SectionProps & {
  layerName: LayerName
  isEnabled?: boolean
  isUngroupedLayer?: boolean
  toggleDropdownLayer: ToggleDropdownLayerType
  $isLoading: boolean
}
