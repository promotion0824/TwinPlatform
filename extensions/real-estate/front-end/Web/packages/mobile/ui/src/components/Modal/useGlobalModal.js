import { useContext } from 'react'
import GlobalModalContext from './GlobalModalContext'

export default function useGlobalModal() {
  return useContext(GlobalModalContext)
}
