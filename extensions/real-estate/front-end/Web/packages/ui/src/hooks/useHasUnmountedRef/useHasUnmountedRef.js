import { useEffect, useRef } from 'react'

export default function useHasUnmountedRef() {
  const hasUnmountedRef = useRef(false)

  useEffect(
    () => () => {
      hasUnmountedRef.current = true
    },
    []
  )

  return hasUnmountedRef
}
