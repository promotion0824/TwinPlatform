import { useRef } from 'react'
import { FetchRefreshContext } from './FetchRefreshContext'

export { useFetchRefresh } from './FetchRefreshContext'

export function FetchRefreshProvider({ children }) {
  const fetchesRef = useRef([])

  function fetchRefresh(name, isPolling = false) {
    fetchesRef.current
      .filter((fetch) => fetch.name === name)
      .forEach((fetch) => fetch.fetch(isPolling))
  }

  fetchRefresh.registerFetch = (fetch) => {
    fetchesRef.current = [...fetchesRef.current, fetch]
  }

  fetchRefresh.unregisterFetchId = (fetchId) => {
    fetchesRef.current = fetchesRef.current.filter(
      (prevFetch) => prevFetch.fetchId !== fetchId
    )
  }

  return (
    <FetchRefreshContext.Provider value={fetchRefresh}>
      {children}
    </FetchRefreshContext.Provider>
  )
}
