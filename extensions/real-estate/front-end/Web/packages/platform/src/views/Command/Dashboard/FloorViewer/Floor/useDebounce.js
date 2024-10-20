import { useLatest } from '@willow/common'
import { useTimer } from '@willow/ui'
import { useRef } from 'react'

export default function useDebounce(fn, ms) {
  const latestFn = useLatest(fn)
  const timer = useTimer()

  const isCallingRef = useRef(false)
  const isQueuedRef = useRef(false)

  function debounce() {
    return (...args) => {
      async function enqueue() {
        if (isCallingRef.current) {
          isQueuedRef.current = true
          return
        }

        timer.clearTimeout()

        async function promiseFn() {
          try {
            isCallingRef.current = true
            await latestFn(...args)
          } finally {
            isCallingRef.current = false

            if (isQueuedRef.current) {
              isQueuedRef.current = false
              enqueue()
            }
          }
        }

        await timer.setTimeout(ms)

        promiseFn()
      }

      enqueue()
    }
  }

  const debouncedFnRef = useRef(debounce())

  return debouncedFnRef.current
}
