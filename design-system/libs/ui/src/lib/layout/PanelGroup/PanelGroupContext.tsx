import { createContext, useContext, RefObject } from 'react'
import { PanelIdsAction } from './utils'

export type PanelGroupContextProps = {
  gapSize: 'medium' | 'small'
  isVertical: boolean
  idsRef: RefObject<string[]>
  resizable: boolean
  activePanelIds: string[] | null
  dispatchActivePanelIds: React.Dispatch<PanelIdsAction>
  persistPanelLayout: boolean
}

export const PanelGroupContext = createContext<
  PanelGroupContextProps | undefined
>(undefined)

export const usePanelGroupContext = (): PanelGroupContextProps => {
  const context = useContext(PanelGroupContext)
  if (!context) {
    throw new Error('Panel components must be wrapped in <PanelGroup/>')
  }
  return context
}
