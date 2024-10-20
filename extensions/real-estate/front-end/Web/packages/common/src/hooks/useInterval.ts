import { useEffect } from 'react'
import useLatest from './useLatest'

export default function useInterval(fn: () => unknown, ms: number) {
  const latestFn = useLatest(fn)

  useEffect(() => {
    const intervalId = window.setInterval(latestFn, ms)
    return () => window.clearInterval(intervalId)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ms])
}
