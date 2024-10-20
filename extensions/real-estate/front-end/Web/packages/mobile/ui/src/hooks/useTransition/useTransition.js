import { useState } from 'react'
import useHeightTransition from './useHeightTransition'

export default function useTransition(props) {
  const { isOpen, defaultIsOpen = false, onChange = () => {} } = props

  const [state, setState] = useState({
    isOpen: defaultIsOpen,
  })

  const derivedIsOpen = isOpen == null ? state.isOpen : isOpen

  return useHeightTransition({
    ...props,
    isOpen: derivedIsOpen,

    onChange(nextIsOpen) {
      setState({
        isOpen: nextIsOpen,
      })

      onChange(nextIsOpen)
    },
  })
}
