import { useState, useCallback, useMemo } from 'react'
import usePromise from './usePromise'

const cached = {}

export default function useCache(getData, dataKey) {
  const [count, setCount] = useState(0)

  const key = useMemo(() => {
    if (typeof dataKey === 'string') {
      return dataKey
    }

    return dataKey()
  }, [dataKey])

  const updateCache = useCallback(
    (newData) => {
      cached[key] = Promise.resolve(newData)
      setCount(Date.now())
    },
    [key]
  )

  const removeCache = useCallback(() => {
    delete cached[key]
  }, [key])

  const retrieveData = useCallback(() => {
    const isTicketWorkgroupsEnabled = true

    if (isTicketWorkgroupsEnabled) {
      // eventually remove the use of this useCache hook, and clean up all its usages
      return getData()
    }

    let cachedPromise = cached[key]

    if (cachedPromise === undefined) {
      const dataPromise = getData()

      if (dataPromise === null) {
        return null
      }

      cached[key] = dataPromise
      cachedPromise = dataPromise
    }

    return cachedPromise
  }, [key, count])

  const { data, isFetching } = usePromise(retrieveData)

  return {
    isFetching,
    data,
    updateCache,
    removeCache,
  }
}
