import { useLatest } from '@willow/common'
import _ from 'lodash'
import { useEffect, useRef } from 'react'

export default function useThrottle(fn, ms) {
  const latestFn = useLatest(fn)

  const throttledFnRef = useRef(_.throttle(latestFn, ms))

  useEffect(() => () => throttledFnRef.current?.cancel(), [])

  return throttledFnRef.current
}
