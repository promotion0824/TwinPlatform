import { useEffect } from 'react'
import { useLatest } from '@willow/common'

export default function useTimeout(fn, ms) {
  const latestFn = useLatest(fn)

  useEffect(() => {
    const timerId = window.setTimeout(latestFn, ms)

    return () => window.clearTimeout(timerId)
  }, [])
}
