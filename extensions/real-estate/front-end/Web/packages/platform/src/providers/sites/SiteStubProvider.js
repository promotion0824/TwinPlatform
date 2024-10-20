import { SiteContext } from './SiteContext'

export default function SitesProvider({ site, children }) {
  return <SiteContext.Provider value={site}>{children}</SiteContext.Provider>
}
