import {
  getSiteIdFromUrl,
  useAnalytics,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { ReactNode, useEffect, useRef } from 'react'
import { useLocation } from 'react-router'
import routes from '../../routes'
import { SiteContext } from './SiteContext'
import { useSites } from './SitesContext'

export default function SiteProvider({ children }: { children: ReactNode }) {
  const {
    isScopeSelectorEnabled,
    location: scope,
    isScopeUsedAsBuilding,
  } = useScopeSelector()
  const analytics = useAnalytics()
  const location = useLocation()
  const sites = useSites()
  const user: {
    saveOptions: (key: string, value: string) => void
    saveLocalOptions: (key: string, value: string | null) => void
    showPortfolioTab: boolean
    options: { siteId?: string; favoriteSiteId?: string }
  } = useUser()
  const isInitialMount = useRef(true)

  // - if scope selector is enabled and current scope is a building, use the siteId of the building
  // - else, if url contains a siteId, use it
  // - else, when a site user (user.showPortfolioTab is false) logs in and favoriteSiteId exists, then use that
  // - else, use the last selected site (user.options.siteId)
  // - else, use the first site in sites
  const favoriteSite = sites.find((s) => s.id === user?.options?.favoriteSiteId)
  const shouldUseFavoriteSite =
    isInitialMount.current && !user?.showPortfolioTab && favoriteSite != null

  const site =
    (isScopeSelectorEnabled && isScopeUsedAsBuilding(scope)
      ? sites.find((prevSite) => prevSite.id === scope?.twin?.siteId)
      : undefined) ??
    sites.find(
      (prevSite) => prevSite.id === getSiteIdFromUrl(location.pathname)
    ) ??
    (shouldUseFavoriteSite ? favoriteSite : null) ??
    sites.find((prevSite) => prevSite.id === user.options?.siteId) ??
    sites[0]

  useEffect(() => {
    isInitialMount.current = false
  }, [])

  useEffect(() => {
    if (site != null) {
      user.saveOptions('siteId', site.id)
    }

    analytics.initializeSiteContext(site)
  }, [site?.id])

  /**
   * business requirement says Dashboard button on MainMenu need to always
   * navigate to routes.sites__siteId of last selected site, so having this
   * effect block not only save last selected site, but also handle the case
   * where user click "go back" or "go forward" and land on different routes.
   * note that last selected site can be either a site with id
   * or All Sites with id of null
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/66113/
   */
  useEffect(() => {
    const siteIdFromUrl = getSiteIdFromUrl(location.pathname)

    if (
      [
        routes.home,
        routes.dashboards,
        routes.portfolio_reports,
        routes.insights,
        routes.tickets,
        routes.inspections,
      ].includes(location.pathname)
    ) {
      user.saveLocalOptions('lastSelectedSiteId', null)
    } else if (siteIdFromUrl != null) {
      user.saveLocalOptions('lastSelectedSiteId', siteIdFromUrl)
    }
  }, [location.pathname])

  // site being undefined is a legitimate case where
  // a new client is onboarded and no site is created yet;
  // however, we are deliberately throwing error in SiteContext
  // when site is undefined to prevent use "useSite" context hook
  // outside of SiteProvider, so we need to default it to an empty object
  return (
    <SiteContext.Provider value={site ?? {}}>{children}</SiteContext.Provider>
  )
}
