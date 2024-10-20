import { ReactNode } from 'react'
import { Site } from '@willow/common/site/site/types'
import { SitesContext } from './SitesContext'
import SiteProvider from './SiteProvider'

export default function SitesProvider({
  sites,
  children,
}: {
  sites: Site[]
  children: ReactNode
}) {
  return (
    <SitesContext.Provider value={sites}>
      <SiteProvider>{children}</SiteProvider>
    </SitesContext.Provider>
  )
}
