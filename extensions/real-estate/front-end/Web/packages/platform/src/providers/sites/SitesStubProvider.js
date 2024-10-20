import { SitesContext } from './SitesContext'

export default function SitesProvider({ sites, children }) {
  return <SitesContext.Provider value={sites}>{children}</SitesContext.Provider>
}
