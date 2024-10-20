import { useScopeSelector } from '@willow/ui'
import { Redirect, useLocation } from 'react-router'
import { useSite } from '../providers'
import routes from '../routes'

/**
 * Check if the current route (including query parameters) matches an entry in an array
 * of updated URLs, and redirect accordingly if a match is found. Any future URLs that
 * need to be updated should be added to the `redirects` array below.
 */
export default function useGetGlobalRedirect() {
  const location = useLocation()
  const scopeSelector = useScopeSelector()
  const site = useSite()

  const scopeId = scopeSelector.location?.twin.id

  const redirects = [
    { old: '/marketplace', new: routes.connectors },
    {
      old: `/marketplace/scopes/${scopeId}`,
      new: routes.connectors_scope__scopeId(scopeId),
    },
    { old: '/marketplace/sites', new: routes.connectors_sites },
    {
      old: `/marketplace/sites/${site.id}`,
      new: routes.connectors_sites__siteId(site.id),
    },
    { old: '/portfolio/dashboards?view=mapView', new: routes.map_viewer },
    {
      old: '/portfolio/dashboards?view=performanceView',
      new: routes.dashboards,
    },
    { old: '/portfolio/dashboards?view=siteView', new: routes.home },
    { old: '/portfolio/twins', new: routes.portfolio_twins_results },
    {
      old: `/portfolio/twins/scope/${scopeId}`,
      new: routes.portfolio_twins_scope__scopeId_results(scopeId),
    },
    {
      old: `/sites/${site.id}?view=performanceView`,
      new: routes.dashboards_sites__siteId(site.id),
    },
  ]

  const currentSearchParams = new URLSearchParams(location.search)

  for (const redirect of redirects) {
    const [pathname, search] = redirect.old.split('?')
    const searchParams = new URLSearchParams(search)

    if (
      location.pathname === pathname &&
      Array.from(searchParams).every(
        ([paramKey, paramValue]) =>
          currentSearchParams.get(paramKey) === paramValue
      )
    ) {
      return <Redirect to={redirect.new} />
    }
  }

  return null
}
