import { useLatest } from '@willow/common'
import _ from 'lodash'
import { useEffect, useRef } from 'react'

export default function useDebounce(fn, ms) {
  const latestFn = useLatest(fn)

  const debouncedFnRef = useRef(_.debounce(latestFn, ms))

  useEffect(() => () => debouncedFnRef.current?.cancel(), [])

  return debouncedFnRef.current
}
