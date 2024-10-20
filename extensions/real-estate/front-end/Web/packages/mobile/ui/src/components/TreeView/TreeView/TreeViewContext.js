import { createContext, useContext } from 'react'

export const TreeViewContext = createContext()
export const TreeViewItemContext = createContext()

export function useTreeView() {
  return useContext(TreeViewContext)
}

export function useTreeViewItem() {
  return useContext(TreeViewItemContext)
}
