import { useEffect, useRef } from 'react'

export default function useLatest(fn: (...args: unknown[]) => unknown) {
  const latestFnRef = useRef(fn)

  useEffect(() => {
    latestFnRef.current = fn
  }, [fn])

  return (...rest) => latestFnRef.current?.(...rest)
}
