import { useEffect, useRef } from 'react'
import _ from 'lodash'
import { useLatest } from '@willow/common'

export default function useDebounce(fn, ms) {
  const latestFn = useLatest(fn)

  const debouncedFnRef = useRef(_.debounce(latestFn, ms))

  useEffect(() => () => debouncedFnRef.current?.cancel(), [])

  return debouncedFnRef.current
}
