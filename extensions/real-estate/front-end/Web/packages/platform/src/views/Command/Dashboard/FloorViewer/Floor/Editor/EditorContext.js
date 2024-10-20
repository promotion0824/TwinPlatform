import { createContext, useContext } from 'react'

export const EditorContext = createContext()

export function useEditor() {
  return useContext(EditorContext)
}
