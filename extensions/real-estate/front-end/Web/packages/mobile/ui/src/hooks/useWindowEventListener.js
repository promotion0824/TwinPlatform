import { useEffect } from 'react'
import { useLatest } from '@willow/common'

export default function useWindowEventListener(
  event,
  eventFn,
  options = false
) {
  const latestFn = useLatest(eventFn)

  useEffect(() => {
    window.addEventListener(event, latestFn, options)

    return () => window.removeEventListener(event, latestFn, options)
  }, [])
}
