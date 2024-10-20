import { useLatest } from '@willow/common'
import { useEffect } from 'react'

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
