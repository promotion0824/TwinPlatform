import { useLayoutEffect, useRef } from 'react'

export default function useLayoutEffectOnUpdate(fn, deps) {
  const hasMountedRef = useRef(false)

  useLayoutEffect(() => {
    if (!hasMountedRef.current) {
      hasMountedRef.current = true
      return () => {}
    }

    return fn()
  }, deps)
}
