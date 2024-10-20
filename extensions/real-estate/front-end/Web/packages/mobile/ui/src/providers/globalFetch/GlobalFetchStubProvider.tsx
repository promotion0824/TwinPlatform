/* eslint-disable @typescript-eslint/no-empty-function */
import { ReactNode } from 'react'
import { GlobalFetchContext } from './GlobalFetchContext'

/**
 * Stub version of GlobalFetchProvider
 */
export default function GlobalFetchStubProvider({
  children,
}: {
  children: ReactNode
}) {
  const context = {
    registerFetch: (_fetch) => {},
    unregisterFetch: (_fetchId: string) => {},
    refresh: (_name: string, _polling: boolean = false) => {},
  }

  return (
    <GlobalFetchContext.Provider value={context}>
      {children}
    </GlobalFetchContext.Provider>
  )
}
