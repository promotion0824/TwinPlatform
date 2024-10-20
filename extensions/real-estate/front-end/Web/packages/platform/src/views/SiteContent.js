/* eslint-disable complexity */
import { lazy } from '@loadable/component'
import {
  FullSizeContainer,
  FullSizeLoader,
  TicketStatusesProvider,
} from '@willow/common'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  api,
  PageNotFound,
  RenderIf,
  useConfig,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { useSite, useSites } from 'providers'
import { Suspense } from 'react'
import { useTranslation } from 'react-i18next'
import { Redirect, Route, Switch, useLocation, useParams } from 'react-router'
import { css } from 'styled-components'
import ValidateSiteId from '../components/ValidateSiteId'
import routes from '../routes'
import useGetGlobalRedirect from '../utils/useGetGlobalRedirect'
import CommandLayoutHeader from './Command/CommandLayoutHeader'
import ExperimentalDashboards from './Command/ExperimentalDashboards/ExperimenTalDashboards'
import Portfolio from './Portfolio/Portfolio'
import PortfolioHome from './Portfolio/PortfolioHome'
import PagedPortfolioProvider, {
  usePagedPortfolioListFeatureFlag,
} from './Portfolio/Sites/PagedPortfolioProvider'
import SearchResultsProvider from './Portfolio/twins/results/page/state/SearchResults'

const LazyLoadedWithSuspense = (importFunction) => {
  const LazyComponent = lazy(importFunction)
  return (props) => (
    <Suspense fallback={<FullSizeLoader />}>
      <LazyComponent {...props} />
    </Suspense>
  )
}

const Admin = LazyLoadedWithSuspense(() => import('./Admin/Admin'))
const CardViewInsights = LazyLoadedWithSuspense(() =>
  import('../components/Insights/CardViewInsights/CardViewInsights')
)
const Command = LazyLoadedWithSuspense(() => import('./Command/Command'))
const Connectors = LazyLoadedWithSuspense(() =>
  import('./Marketplace/Marketplace')
)
const InsightNode = LazyLoadedWithSuspense(() =>
  import('../components/Insights/InsightNode/InsightNode')
)
const InsightTypeNode = LazyLoadedWithSuspense(() =>
  import('../components/Insights/InsightTypeNode/InsightTypeNode')
)
const Inspections = LazyLoadedWithSuspense(() =>
  import('./InspectionsPage.tsx')
)
const MapView = LazyLoadedWithSuspense(() =>
  import('./Portfolio/MapView/MapView')
)
const PortfolioDashboards = LazyLoadedWithSuspense(() =>
  import('./Portfolio/PortfolioDashboards')
)
const Reports = LazyLoadedWithSuspense(() =>
  import('./Portfolio/Reports/Reports')
)
const Rules = LazyLoadedWithSuspense(() => import('components/Rules/Rules'))
const SearchResults = LazyLoadedWithSuspense(() =>
  import('./Portfolio/twins/results/page/ui/SearchResults')
)
const SiteDashboards = LazyLoadedWithSuspense(() =>
  import('./Command/Dashboard/Dashboard')
)
const Tickets = LazyLoadedWithSuspense(() => import('./TicketsPage.tsx'))
const TimeSeries = LazyLoadedWithSuspense(() =>
  import('./TimeSeries/TimeSeries')
)
const TwinView = LazyLoadedWithSuspense(() =>
  import('./Portfolio/twins/view/TwinView')
)
const NotificationSettings = LazyLoadedWithSuspense(() =>
  import('./Admin/NotificationSettings/NotificationSettings')
)

const Notifications = LazyLoadedWithSuspense(() =>
  import('./Notifications/Notifications')
)

const getTicketStatuses = async (customerId) => {
  const response = await api.get(`/customers/${customerId}/ticketStatuses`)
  return response.data
}

export default function SiteContent() {
  const config = useConfig()
  const sites = useSites()
  const site = useSite()
  const user = useUser()
  const featureFlags = useFeatureFlag()
  const showRules =
    config.hasFeatureToggle('wp-rules-enabled') && user.showRulingEngineMenu

  // Eagerly load the models of interest.
  useModelsOfInterest({ enabled: user.isAuthenticated })

  const redirect = useGetGlobalRedirect()
  if (redirect) return redirect

  return (
    <TicketStatusesProvider
      customerId={user.customer.id}
      getTicketStatuses={getTicketStatuses}
    >
      <Switch>
        <Route path={[routes.home, routes.home_scope__scopeId()]} exact>
          <Portfolio>
            <ScopeTypeDrivenHome sites={sites} />
          </Portfolio>
        </Route>

        <Route
          path={[
            routes.portfolio,
            // newly set up reports routes with scopeId will not include "portfolio" in the path
            routes.reports,
            routes.reports_scope__scopeId(),
          ]}
        >
          <Switch>
            <Route path={routes.portfolio} exact>
              <Portfolio>
                <Redirect to={routes.home} />
              </Portfolio>
            </Route>

            <Route path={[routes.portfolio_twins]}>
              <TwinExplorerPage />
            </Route>

            <Route
              path={[
                routes.portfolio_reports__siteId(),
                routes.portfolio_reports,
                routes.reports,
                routes.reports_scope__scopeId(),
              ]}
            >
              <Portfolio>
                <Reports />
              </Portfolio>
            </Route>
          </Switch>
        </Route>
        <Route
          path={[
            routes.insights,
            routes.insights_insightId(), // legacy route
            routes.insights_insight__insightId(),
            routes.insights_scope__scopeId(),
            routes.insights_scope__scopeId_insight__insightId(),
            routes.insights_rule__ruleId(),
            routes.insights_scope__scopeId_rule__ruleId(),
            routes.insights_scope__scopeId_insight__insightId(),
          ]}
        >
          <CommandLayoutHeader />
          <Route
            path={[routes.insights, routes.insights_scope__scopeId()]}
            exact
          >
            <CardViewInsights />
          </Route>

          <Route
            path={[
              routes.insights_rule__ruleId(),
              routes.insights_scope__scopeId_rule__ruleId(),
            ]}
            exact
          >
            <InsightTypeNode />
          </Route>

          <Route
            path={[
              routes.insights_insightId(),
              routes.insights_insight__insightId(),
              routes.insights_scope__scopeId_insight__insightId(),
            ]}
            exact
          >
            <InsightNode
              enableDiagnostics={featureFlags.hasFeatureToggle('diagnostic')}
            />
          </Route>
        </Route>

        <Route path={routes.dashboards}>
          <DashboardsPage />
        </Route>

        <Route path={routes.notifications} exact>
          <Notifications />
        </Route>

        <Route path={routes.tickets}>
          <Tickets />
        </Route>

        <Route
          path={[
            routes.inspections_scope__scopeId_inspection__inspectionId_check__checkId(),
            routes.inspections_inspection__inspectionId_check__checkId(),
            routes.inspections_scope__scopeId_zones_zone__zoneId(),
            routes.sites__siteId_inspections__inspectionId__checkId(),
            routes.sites__siteId_inspections_zones__zoneId(),
            routes.sites__siteId_inspections_usage(),
            routes.sites__siteId_inspections_zones(),
            routes.inspections__inspectionId__checkId(),
            routes.inspections_scope__scopeId_usage(),
            routes.inspections_scope__scopeId(),
            routes.inspections,
          ]}
        >
          <Inspections />
        </Route>

        <Route path={routes.map_viewer}>
          <MapView />
        </Route>
        <Route
          path={[
            routes.experimental_dashboards,
            routes.experimental_dashboards_scope__scopeId(),
          ]}
          exact
        >
          {featureFlags?.hasFeatureToggle('experimentalDashboards') ? (
            <>
              <CommandLayoutHeader />
              <ExperimentalDashboards />
            </>
          ) : (
            <Redirect to={routes.home} />
          )}
        </Route>
        <Route path="/sites" exact>
          <Redirect to={`/sites/${site.id}`} />
        </Route>
        {/* Eventually sites/:siteId will be completely retired and replaced with scopeId;
            until then, when we have scopeSelector enabled, we do not validate the siteId
            as siteId for a "land" or a "space" twin function as a scope would be needed for 
            backwards compatibility.
        */}
        <Route path="/sites/:siteId">
          {featureFlags.hasFeatureToggle('scopeSelector') ? (
            <Command />
          ) : (
            <ValidateSiteId>
              <Command />
            </ValidateSiteId>
          )}
        </Route>
        <Route path="/rules" exact>
          <RenderIf condition={showRules}>
            <Rules />
          </RenderIf>
        </Route>
        <Route path={routes.connectors} exact>
          {featureFlags.hasFeatureToggle('scopeSelector') ? (
            <Connectors />
          ) : (
            <Redirect to={routes.connectors_sites__siteId(site.id)} />
          )}
        </Route>
        <Route
          path={[
            routes.connectors_sites__siteId(),
            routes.connectors_scope__scopeId(),
            routes.connectors_sites,
          ]}
        >
          <Connectors />
        </Route>
        <Route path={[routes.timeSeries, routes.timeSeries_scope__scopeId()]}>
          <TimeSeries />
        </Route>
        {/* 
          We allow anyone to access notification settings page if the feature is enabled,
          for non-admin users, we hide other admin tabs like "Portfolios", "Users", and "Model of Interests".
        */}
        <Route
          path={[
            routes.admin_notification_settings,
            routes.admin_notification_settings_add,
            routes.admin_notification_settings__triggerId_edit(),
          ]}
          exact
        >
          <RenderIf
            condition={featureFlags?.hasFeatureToggle('isNotificationEnabled')}
          >
            <NotificationSettings />
          </RenderIf>
        </Route>
        <Route path="/admin">
          <RenderIf condition={user.showAdminMenu}>
            <Admin />
          </RenderIf>
        </Route>
        <Route>
          <PageNotFound />
        </Route>
      </Switch>
    </TicketStatusesProvider>
  )
}

/**
 * Twin Explorer, also known as "Search & Explore".
 */
function TwinExplorerPage() {
  return (
    <SearchResultsProvider>
      <Switch>
        <Route
          path={[
            routes.portfolio_twins_results,
            routes.portfolio_twins_scope__scopeId_results(),
          ]}
        >
          <SearchResults />
        </Route>
        <Route
          path={[
            routes.portfolio_twins_view__siteId__twinId(),
            routes.portfolio_twins_view__twinId(),
          ]}
        >
          <TenantDependentTwinView />
        </Route>
      </Switch>
    </SearchResultsProvider>
  )
}

/**
 * For Multi-tenant client instances, it is still required to have a siteId in the URL
 * to query a twin detail, so keep siteId in URL for Multi-tenant client instances, and
 * redirect to the twin view page without siteId for Single-tenant client instances.
 */
const TenantDependentTwinView = () => {
  const { isSingleTenant } = useConfig()
  const { siteId, twinId } = useParams()
  const { pathname } = useLocation()

  if (
    isSingleTenant &&
    pathname === routes.portfolio_twins_view__siteId__twinId(siteId, twinId)
  ) {
    return <Redirect to={routes.portfolio_twins_view__twinId(twinId)} />
  }

  return <TwinView />
}

/**
 * Dashboards Page display Sigma Visualizations for a location.
 */
function DashboardsPage() {
  const user = useUser()
  const site = useSite()
  const sites = useSites()
  const { pathname } = useLocation()
  const {
    isScopeSelectorEnabled,
    location,
    scopeLookup,
    isScopeUsedAsBuilding,
  } = useScopeSelector()

  const isSiteLevelUser = !user.showPortfolioTab

  // legacy routes for dashboards with siteId
  if (!isScopeSelectorEnabled) {
    return (
      <>
        <Route path={routes.dashboards} exact>
          <Portfolio>
            {isSiteLevelUser ? <DashboardDisabled /> : <PortfolioDashboards />}
          </Portfolio>
        </Route>
        <Route path={routes.dashboards_sites__siteId()}>
          <Portfolio>
            <SiteDashboards site={site} />
          </Portfolio>
        </Route>
      </>
    )
  }

  // redirect to the path with scopeId if scope selector is enabled and
  // the current path is a siteId based dashboard
  const scopeIdBasedOnSiteId = scopeLookup[site.id]?.twin?.id

  if (pathname.includes(site.id)) {
    if (scopeIdBasedOnSiteId) {
      return (
        <Redirect to={routes.dashboards_scope__scopeId(scopeIdBasedOnSiteId)} />
      )
      // be defensive against an edge case where scope for a legacy siteId based route
      // isn't found in the scope selector so we have to fallback to the siteId based route
    } else {
      return (
        <Portfolio>
          <SiteDashboards site={site} />
        </Portfolio>
      )
    }
  }

  // dashboard routes for a building twin as scope
  if (isScopeUsedAsBuilding(location)) {
    return (
      <Route path={routes.dashboards_scope__scopeId()}>
        <Portfolio>
          <SiteDashboards
            site={sites.find((s) => s.id === location.twin.siteId) ?? site}
          />
        </Portfolio>
      </Route>
    )
  }

  // all location dashboard route
  return (
    <Portfolio>
      {isSiteLevelUser ? <DashboardDisabled /> : <PortfolioDashboards />}
    </Portfolio>
  )
}

/**
 * design is in process to update this section, will be updated soon
 */
const DashboardDisabled = () => {
  const { t } = useTranslation()
  return (
    <FullSizeContainer
      css={css(({ theme }) => ({
        ...theme.font.heading.xl,
        color: theme.color.neutral.fg.default,
      }))}
    >
      {t('plainText.NoDashboardsAvailableForThisLocation')}
    </FullSizeContainer>
  )
}

/**
 * The home page is driven by the scope selector. If the scope
 * selector is enabled and the current scope is a building, we
 * redirect to the building home page. Otherwise, we show
 * the portfolio home page.
 */
const ScopeTypeDrivenHome = ({ sites }) => {
  const { scopeId } = useParams()
  const {
    isScopeSelectorEnabled,
    location,
    scopeLookup,
    isScopeUsedAsBuilding,
  } = useScopeSelector()
  const scope = scopeLookup[scopeId]
  const pagedPortfolioListEnabled = usePagedPortfolioListFeatureFlag()

  const isBuildingScope = isScopeUsedAsBuilding(scope)
  const siteBasedOnScope = sites.find((s) => s.id === location?.twin?.siteId)

  if (isScopeSelectorEnabled && isBuildingScope && siteBasedOnScope) {
    return <SiteDashboards site={siteBasedOnScope} />
  } else {
    return (
      // can move this provider to upper lever when needed later,
      // currently only used in the portfolio home page
      <PagedPortfolioProvider enabled={pagedPortfolioListEnabled}>
        <PortfolioHome />
      </PagedPortfolioProvider>
    )
  }
}
