import { createContext, useContext } from 'react'

export const EditModeContext = createContext()

export function useEditMode() {
  return useContext(EditModeContext)
}
