import { useLatest } from '@willow/common'
import { useEffect } from 'react'

export default function useEventListener(ref, event, eventFn, options = false) {
  const latestFn = useLatest(eventFn)

  useEffect(() => {
    ref.current.addEventListener(event, latestFn, options)

    return () => ref.current?.removeEventListener(event, latestFn, options)
  }, [ref.current])
}
