import { useContext } from 'react'
import GlobalModalContext from './GlobalModalContext'
import SingleModalContext from './SingleModalContext'

export default function useModal() {
  const singleModal = useContext(SingleModalContext)
  const globalModal = useContext(GlobalModalContext)

  return singleModal ?? globalModal
}
