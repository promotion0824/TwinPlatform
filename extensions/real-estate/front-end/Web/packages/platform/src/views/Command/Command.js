import { Fragment } from 'react'
import { useParams, Route, Switch, Redirect } from 'react-router'
/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import { useFeatureFlag, useScopeSelector } from '@willow/ui'
import CommandLayoutHeader from './CommandLayoutHeader'
import Dashboard from './Dashboard/Dashboard'
import Inspections from './Inspections/Inspections'
import OccupancyFloor from './Occupancy/OccupancyFloor/OccupancyFloor'
import Reports from '../Portfolio/Reports/Reports'
import Tickets from './Tickets/Tickets'
import { useSites, useSite } from '../../providers'
import { PortfolioContext } from '../Portfolio/PortfolioContext'
import routes from '../../routes'
import InsightNode from '../../components/Insights/InsightNode/InsightNode'
import CardViewInsights from '../../components/Insights/CardViewInsights/CardViewInsights'
import InsightTypeNode from '../../components/Insights/InsightTypeNode/InsightTypeNode'
import ExperimentalDashboards from './ExperimentalDashboards/ExperimenTalDashboards'

export default function Command() {
  const params = useParams()
  const site = useSite()
  const featureFlags = useFeatureFlag()

  return (
    <Fragment key={params.siteId}>
      <CommandLayoutHeader />
      <Switch>
        <Route
          path={[
            routes.sites__siteId_insights__insightId(),
            routes.sites__siteId_insight_rule__ruleId(),
            routes.sites__siteId_insights(),
          ]}
        >
          <LegacyInsightRoutesWithRedirect
            enableDiagnostics={featureFlags.hasFeatureToggle('diagnostic')}
          />
        </Route>
        <Route
          path="/sites/:siteId/tickets/:ticketId?"
          children={<Tickets />}
        />
        <Route path="/sites/:siteId/inspections">
          <Inspections />
        </Route>
        <Route path="/sites/:siteId/reports" exact>
          <PortfolioForSite>
            <Reports />
          </PortfolioForSite>
        </Route>
        <Route path="/sites/:siteId/occupancy/floors/:floorId" exact>
          {site.features.isOccupancyEnabled && <OccupancyFloor />}
        </Route>
        <Route path={routes.experimental_dashboards_sites__siteId()} exact>
          {featureFlags?.hasFeatureToggle('experimentalDashboards') ? (
            <ExperimentalDashboards />
          ) : (
            <Redirect to={routes.home} />
          )}
        </Route>
        <Route
          path="/sites/:siteId/:floors?/:floorId?"
          children={<Dashboard />}
        />
      </Switch>
    </Fragment>
  )
}

const PortfolioForSite = ({ children }) => {
  const sites = useSites()

  return (
    <PortfolioContext.Provider
      value={{
        selectedBuilding: { id: null },
        sites,
        toggleBuilding: () => {},
      }}
    >
      {children}
    </PortfolioContext.Provider>
  )
}

function LegacyInsightRoutesWithRedirect({ enableDiagnostics }) {
  const { siteId, ruleId, insightId } = useParams()
  const { isScopeSelectorEnabled, scopeLookup } = useScopeSelector()

  const scopeIdBasedOnSiteId = scopeLookup[siteId]?.twin?.id
  const enabledRedirect = siteId && scopeIdBasedOnSiteId

  // fallback to legacy routes when scope selector is not enabled or the correct scopeId is not found based on siteId
  if (!isScopeSelectorEnabled || !scopeIdBasedOnSiteId) {
    return (
      <>
        <Route path={[routes.sites__siteId_insights()]} exact>
          <CardViewInsights />
        </Route>

        <Route path={[routes.sites__siteId_insight_rule__ruleId()]} exact>
          <InsightTypeNode />
        </Route>

        <Route path={[routes.sites__siteId_insights__insightId()]} exact>
          <InsightNode enableDiagnostics={enableDiagnostics} />
        </Route>
      </>
    )
  }

  if (enabledRedirect) {
    if (ruleId) {
      return (
        <Redirect
          to={routes.insights_scope__scopeId_rule__ruleId(
            scopeIdBasedOnSiteId,
            ruleId
          )}
        />
      )
    }

    if (insightId) {
      return (
        <Redirect
          to={routes.insights_scope__scopeId_insight__insightId(
            scopeIdBasedOnSiteId,
            insightId
          )}
        />
      )
    }

    return (
      <Redirect to={routes.insights_scope__scopeId(scopeIdBasedOnSiteId)} />
    )
  }
}
