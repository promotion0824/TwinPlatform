import { createContext, useContext } from 'react'

export const ModalContext = createContext()

export function useModal() {
  return useContext(ModalContext)
}
