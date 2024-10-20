import { createContext, useContext } from 'react'

export const FormControlContext = createContext()

export function useFormControl() {
  return useContext(FormControlContext)
}
