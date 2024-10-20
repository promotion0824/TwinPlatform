import { createContext, useContext } from 'react'

export const DatePickerContext = createContext()

export function useDatePicker() {
  return useContext(DatePickerContext)
}
