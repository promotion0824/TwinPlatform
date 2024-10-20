import { useEffect, useState } from 'react'

export default function usePromise(getData) {
  const [isFetching, setIsFetching] = useState(true)
  const [data, setData] = useState()

  useEffect(() => {
    let mounted = true

    async function fetchData() {
      const promise = getData()

      if (promise === null) {
        return
      }

      setIsFetching(true)
      setData(undefined)

      const result = await promise

      if (!mounted) {
        return
      }

      setData(result)
      setIsFetching(false)
    }

    fetchData()

    return () => {
      mounted = false
    }
  }, [getData])

  return {
    isFetching,
    data,
  }
}
