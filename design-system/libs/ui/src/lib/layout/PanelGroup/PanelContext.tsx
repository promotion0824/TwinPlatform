import { createContext, useContext } from 'react'
import { PanelGroupContextProps } from './PanelGroupContext'

export type PanelContextProps = {
  gapSize: PanelGroupContextProps['gapSize']
  id?: string
  collapsible: boolean
  onCollapse: (collapsed: boolean) => void
}

export const PanelContext = createContext<PanelContextProps | undefined>(
  undefined
)

export const usePanelContext = (): PanelContextProps => {
  const context = useContext(PanelContext)
  if (!context) {
    throw new Error('PanelHeader and PanelContent must be wrapped in <Panel/>')
  }
  return context
}
