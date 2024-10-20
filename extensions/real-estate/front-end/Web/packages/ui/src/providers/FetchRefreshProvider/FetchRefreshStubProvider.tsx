/* eslint-disable @typescript-eslint/no-empty-function */
import { ReactNode } from 'react'
import { FetchRefreshContext } from './FetchRefreshContext'

/**
 * Stub version of FetchRefreshProvider
 */
export default function FetchRefreshStubProvider({
  children,
}: {
  children: ReactNode
}) {
  function fetchRefresh() {}
  fetchRefresh.unregisterFetchId = () => {}
  fetchRefresh.registerFetch = () => {}

  return (
    <FetchRefreshContext.Provider value={fetchRefresh}>
      {children}
    </FetchRefreshContext.Provider>
  )
}
