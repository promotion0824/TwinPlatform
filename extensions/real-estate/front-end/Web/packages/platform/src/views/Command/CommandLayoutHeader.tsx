/* eslint-disable complexity */
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import {
  ALL_LOCATIONS,
  ALL_SITES,
  ScopeSelectorWrapper,
  useAnalytics,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { useHistory, useLocation, useParams } from 'react-router'
import { css } from 'styled-components'
import SiteSelect from '../../components/SiteSelect/SiteSelect'
import { useSite, useSites } from '../../providers'
import routes, { branches, getSitePage } from '../../routes'
import { LayoutHeader } from '../Layout/index'

export default function CommandLayoutHeader({
  onChange,
}: {
  onChange?: (location: LocationNode) => void
}) {
  const { isScopeSelectorEnabled } = useScopeSelector()
  const user = useUser()
  const analytics = useAnalytics()
  const history = useHistory()
  const location = useLocation()
  const params = useParams<{ siteId?: string }>()
  const sites = useSites()
  const site = useSite()
  const [{ admin, category, view }] = useMultipleSearchParams([
    'admin',
    'category',
    'view',
  ])

  const isSiteRoute =
    location.pathname === routes.sites__siteId(site?.id) ||
    location.pathname.startsWith(
      routes.sites__siteId_floors__floorId(site?.id, '')
    )
  const isTicketsRoute = location.pathname.includes('/tickets')
  const isInsightsRoute = location.pathname.includes('/insights')
  const isInspectionsRoute = location.pathname.includes('/inspections')
  const isExperimentalDashboardRoute = location.pathname.includes(
    '/experimental-dashboards'
  )

  // Attempt to get the page (or branch) for
  // - pathname (/sites/:siteId/page) where a specific site is selected, or
  // - pathname (/page) where "All sites" option is selected
  const sitePage =
    getSitePage(location.pathname) ||
    Object.keys(branches).find((branch) =>
      location.pathname.includes(`/${branch}`)
    )
  const getSiteRoute = sitePage ? branches[sitePage] : null

  const isReadOnly = site.userRole !== 'admin' || admin !== 'true'

  const handleSiteChange = (nextSite) => {
    const params = new URLSearchParams()
    if (category != null) {
      params.append('category', category as string)
    }
    if (!isReadOnly) {
      params.append('admin', 'true')
    }
    if (view != null) {
      params.append('view', view as string)
    }
    const searchParams = params.toString()

    analytics.track('Site Select', {
      site: nextSite?.id != null ? nextSite : ALL_SITES,
      customer: user?.customer ?? {},
    })

    if (isScopeSelectorEnabled) {
      // TODO: complete other routes like Insights, Dashboards, etc.
      const { scopeId: nextScopeId } = nextSite
      const scopeIsDefined = nextScopeId && nextScopeId !== ALL_LOCATIONS

      const nextChildScopeRoute = isInspectionsRoute
        ? routes.inspections_scope__scopeId(nextScopeId)
        : isTicketsRoute
        ? routes.tickets_scope__scopeId(nextScopeId)
        : isInsightsRoute
        ? routes.insights_scope__scopeId(nextScopeId)
        : isExperimentalDashboardRoute
        ? routes.experimental_dashboards_scope__scopeId(nextScopeId)
        : routes.home_scope__scopeId(nextScopeId)

      const nextAllScopeRoute = isInspectionsRoute
        ? routes.inspections
        : isTicketsRoute
        ? routes.tickets
        : isInsightsRoute
        ? routes.insights
        : isExperimentalDashboardRoute
        ? routes.experimental_dashboards
        : routes.home

      const nextDestination = scopeIsDefined
        ? nextChildScopeRoute
        : nextAllScopeRoute

      if (nextDestination) {
        history.push({
          pathname: scopeIsDefined ? nextChildScopeRoute : nextAllScopeRoute,
          search: searchParams,
        })
      }
      return
    }

    if (isSiteRoute && nextSite?.children?.length) {
      history.push(routes.home)
    } else if (nextSite?.id != null) {
      history.push({
        pathname: getSiteRoute
          ? getSiteRoute(nextSite.id)
          : routes.sites__siteId(nextSite.id),
        search: searchParams,
      })
    } else if (isInsightsRoute) {
      history.push(routes.insights)
    } else if (isTicketsRoute) {
      history.push(routes.tickets)
    } else if (isInspectionsRoute) {
      history.push(routes.inspections)
    } else {
      history.push({
        pathname: routes.home,
        search: searchParams,
      })
    }
  }

  return (
    <LayoutHeader>
      <div
        css={css`
          display: flex;
          justify-content: center;
          flex-direction: column;
          height: 100%;
        `}
      >
        {isScopeSelectorEnabled ? (
          <ScopeSelectorWrapper
            onLocationChange={
              onChange ||
              ((loc) => {
                const { twin } = loc
                handleSiteChange({
                  ...twin,
                  id: twin.siteId || null,
                  children: loc.children,
                  scopeId: twin.id,
                })
              })
            }
          />
        ) : (
          <SiteSelect
            isAllSiteIncluded
            sites={sites}
            value={sites.find((s) => s.id === params.siteId) ?? site}
            onChange={handleSiteChange}
          />
        )}
      </div>
    </LayoutHeader>
  )
}
