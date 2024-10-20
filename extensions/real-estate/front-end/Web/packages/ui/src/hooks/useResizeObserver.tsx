import { debounce } from 'lodash'
import { useEffect } from 'react'

/**
 * Custom hook for observing element resizes with debouncing.
 */
function useResizeObserver(
  element: HTMLElement | null,
  callback: (entries: ResizeObserverEntry[]) => void,
  debounceTime = 100
) {
  const debouncedCallback = debounce(callback, debounceTime)
  useEffect(() => {
    if (!element) return undefined

    const observer = new ResizeObserver((entries) => {
      debouncedCallback(entries)
    })

    if (element) {
      observer.observe(element)
    }

    return () => {
      observer.disconnect()
    }
  }, [element, debouncedCallback])
}

export default useResizeObserver
