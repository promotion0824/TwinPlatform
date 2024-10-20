import { createContext, useContext } from 'react'

interface SidebarContextProps {
  isCollapsed: boolean
}

export const SidebarContext = createContext<SidebarContextProps | undefined>(
  undefined
)

export const useSidebarContext = (): SidebarContextProps => {
  const context = useContext(SidebarContext)

  if (!context) {
    throw new Error('useSidebarContext requires a SidebarContext provider')
  }

  return context
}
