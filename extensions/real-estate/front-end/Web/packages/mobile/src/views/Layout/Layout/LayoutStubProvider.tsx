/* eslint-disable @typescript-eslint/no-empty-function */
import { ReactNode, useRef } from 'react'
import { LayoutContext } from './LayoutContext'
export { default as LayoutHeader } from './LayoutHeader'

/**
 * Stub version of LayoutProvider
 */
export default function LayoutStubProvider({
  children,
}: {
  children: ReactNode
}) {
  const headerRef = useRef()

  const context = {
    headerRef,
    title: 'title',
    subTitle: 'subtitle',
    showBackButton: true,
    backUrl: 'https://willowinc.com',
    sites: [],
    site: null,
    selectSite: (_nextSite: any) => {},
    getSiteUrl: (_siteId: string) => '?',
    setShowBackButton: () => {},
    setTitle: () => {},
    setSubTitle: () => {},
  }

  return (
    <LayoutContext.Provider value={context}>{children}</LayoutContext.Provider>
  )
}
