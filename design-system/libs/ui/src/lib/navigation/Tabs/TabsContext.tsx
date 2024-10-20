import { createContext, useContext } from 'react'

export type TabWithWidth = {
  value: string
  width?: number
}

export interface TabsContextProps {
  selectedTab?: string | null
  setSelectedTab: (tab: string) => void
  setTabWidths: (taWidths: TabWithWidth[]) => void
  setVisibleCount: (count: number) => void
  tabWidths: TabWithWidth[]
  visibleCount: number
}

export const TabsContext = createContext<TabsContextProps | undefined>(
  undefined
)

export const useTabsContext = (): TabsContextProps => {
  const context = useContext(TabsContext)

  if (!context) {
    throw new Error('useTabsContext requires a TabsContext provider')
  }

  return context
}
