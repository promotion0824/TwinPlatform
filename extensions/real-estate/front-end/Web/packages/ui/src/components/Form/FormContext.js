import { createContext, useContext } from 'react'

export const FormContext = createContext()

export function useForm() {
  return useContext(FormContext)
}
