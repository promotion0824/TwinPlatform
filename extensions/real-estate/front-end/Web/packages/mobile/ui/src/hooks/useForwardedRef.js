import { useRef } from 'react'

export default function useForwardedRef(forwardedRef) {
  const ref = useRef()

  return forwardedRef ?? ref
}
