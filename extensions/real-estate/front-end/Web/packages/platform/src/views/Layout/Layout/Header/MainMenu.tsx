import { WillowLogoWhite } from '@willow/common'
import { Flex, Permission, Text, useScopeSelector, useUser } from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { Drawer } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useLocation } from 'react-router'
import { Link } from 'react-router-dom'
import { styled } from 'twin.macro'
import { useSite } from '../../../../providers'
import routes from '../../../../routes'
import makeScopedInspectionsPath from '../../../Command/Inspections/makeScopedInspectionsPath'
import MainMenuButton from './MainMenuButton'
import {
  useClassicExplorerLandingPath,
  useHomeUrl,
  useSelectedSiteId,
} from './utils'

const MenuWrapper = styled.div`
  display: none;
  @media (max-width: 1240px) {
    display: block;
  }
`
export type Layout = {
  menuItems: MenuItem[]
}

export type MenuItem = {
  id: string
  header: string
  subHeader: string
  disabled: boolean
  to: string
}

// eslint-disable-next-line complexity
export default function MainMenu({
  isOpen,
  user,
  layout,
  sites,
  featureFlags,
  config,
  onClose,
}: {
  isOpen: boolean
  user: {
    portfolios: Array<{
      features: {
        [key: string]: boolean
      }
    }>
    showAdminMenu?: boolean
    showPortfolioTab?: boolean
    showRulingEngineMenu?: boolean
    localOptions?: {
      lastSelectedSiteId?: string
      scopeSelectorLocation?: LocationNode
      insightsGroupBy?: string
    }
    options?: {
      favoriteSiteId?: string
    }
    hasPermissions: (args: Permission | Permission[]) => boolean | undefined
  }
  layout: Layout
  sites: Array<{
    id: string
  }>
  featureFlags: { hasFeatureToggle: (k: string) => boolean }
  config: { hasFeatureToggle: (k: string) => boolean }
  onClose: () => void
}) {
  const { isScopeSelectorEnabled, location: scopeLocation } = useScopeSelector()
  const location = useLocation()
  const site = useSite()
  const { t } = useTranslation()
  const classicExplorerLandingPath = useClassicExplorerLandingPath({
    hasBaseModuleOption: { hasBaseModule: false },
  })

  const isPortfolioUser = user.showPortfolioTab
  const lastSelectedSiteId = user?.localOptions?.lastSelectedSiteId
  const selectedSiteId = useSelectedSiteId()

  /**
   * Portfolio level user (isPortfolioUser is true):
   *   - if pathname starts with routes.home or lastSelectedSiteId is null:
   *     - navigate to routes.portfolio_reports (where All Sites is selected)
   *   - else, navigate to routes.portfolio_reports__siteId(lastSelectedSiteId)
   *
   * Site level user (isPortfolioUser is false):
   *   - always navigate to routes.sites__siteId_reports(site.id) where site.id come from SiteProvider
   */

  const reportUrl = isPortfolioUser
    ? location.pathname === routes.home || !lastSelectedSiteId
      ? routes.portfolio_reports
      : routes.portfolio_reports__siteId(lastSelectedSiteId)
    : routes.sites__siteId_reports(site.id)
  const scopedReportsUrl = scopeLocation
    ? routes.reports_scope__scopeId(scopeLocation.twin.id)
    : routes.reports

  const homeUrl = useHomeUrl(selectedSiteId)

  const dashboardUrl = selectedSiteId
    ? routes.dashboards_sites__siteId(selectedSiteId)
    : routes.dashboards
  const scopedDashboardUrl = scopeLocation
    ? routes.dashboards_scope__scopeId(scopeLocation.twin.id)
    : routes.dashboards

  const inspectionsUrl = selectedSiteId
    ? routes.sites__siteId_inspections(selectedSiteId) // route for single site
    : routes.inspections // route for "All Sites"

  const ticketsUrl =
    isScopeSelectorEnabled && scopeLocation?.twin?.id
      ? routes.tickets_scope__scopeId(scopeLocation.twin.id)
      : selectedSiteId
      ? routes.sites__siteId_tickets(selectedSiteId)
      : routes.tickets

  const insightsUrl = selectedSiteId
    ? routes.sites__siteId_insights(selectedSiteId)
    : routes.insights
  const scopedInsightsUrl = scopeLocation
    ? routes.insights_scope__scopeId(scopeLocation.twin.id)
    : routes.insights

  const twinsRouteWithScope =
    isScopeSelectorEnabled && scopeLocation?.twin?.id
      ? routes.portfolio_twins_scope__scopeId_results(scopeLocation.twin.id)
      : routes.portfolio_twins_results
  return (
    <Drawer
      header={
        <Link onClick={onClose} to={homeUrl}>
          <WillowLogoWhite height={18} />
        </Link>
      }
      onClose={onClose}
      opened={isOpen}
      position="left"
    >
      {layout.menuItems.length > 0 && (
        <MenuWrapper>
          <Flex padding="large large small">
            <Text type="h4">{t('plainText.menu')}</Text>
          </Flex>
          {layout.menuItems.map(
            (menuItem: {
              id: string
              header: string
              subHeader: string
              disabled: boolean
              to: string
            }) => (
              <MainMenuButton
                key={menuItem.id}
                header={t(`headers.${_.camelCase(menuItem.header)}`, {
                  defaultValue: menuItem.header,
                })}
                onClick={onClose}
                to={menuItem.to}
                disabled={menuItem.disabled}
              >
                {menuItem.subHeader}
              </MainMenuButton>
            )
          )}
          <hr />
        </MenuWrapper>
      )}
      {site != null && site?.id != null && (
        <>
          <MainMenuButton
            header={t('headers.home')}
            onClick={onClose}
            to={homeUrl}
          />
          <MainMenuButton
            header={t('headers.dashboards')}
            onClick={onClose}
            to={isScopeSelectorEnabled ? scopedDashboardUrl : dashboardUrl}
          >
            {t('plainText.visualizePortfolioData')}
          </MainMenuButton>

          <MainMenuButton
            defaultTile="E"
            header={t('headers.searchAndExplore')}
            onClick={onClose}
            to={twinsRouteWithScope}
          >
            {t('plainText.accessTwinAndRelatedInfo', {
              defaultValue: 'Access Twin And Related Info',
            })}
          </MainMenuButton>
          {config.hasFeatureToggle('wp-rules-enabled') &&
            user.showRulingEngineMenu && (
              <MainMenuButton
                header={t('headers.rules')}
                onClick={onClose}
                to="/rules"
              >
                {t('plainText.manageRules')}
              </MainMenuButton>
            )}
          <MainMenuButton
            header={t('headers.reports')}
            onClick={onClose}
            to={isScopeSelectorEnabled ? scopedReportsUrl : reportUrl}
          >
            {t('plainText.viewReports')}
          </MainMenuButton>
          <MainMenuButton
            data-testid="insights-menu-button"
            header={t('headers.insights')}
            onClick={onClose}
            to={`${isScopeSelectorEnabled ? scopedInsightsUrl : insightsUrl}${
              user?.localOptions?.insightsGroupBy
                ? `?groupBy=${user.localOptions.insightsGroupBy}`
                : ''
            }`}
          >
            {t('plainText.viewAndSetInsights')}
          </MainMenuButton>
          <MainMenuButton
            data-testid="tickets-menu-button"
            header={t('headers.tickets')}
            onClick={onClose}
            to={ticketsUrl}
          >
            {t('plainText.reviewAndManageTickets')}
          </MainMenuButton>
          <MainMenuButton
            data-testid="inspections-menu-button"
            header={t('headers.inspections')}
            onClick={onClose}
            to={
              isScopeSelectorEnabled
                ? makeScopedInspectionsPath(
                    scopeLocation ? scopeLocation.twin.id : undefined
                  )
                : inspectionsUrl
            }
          >
            {t('plainText.reviewAndManageInspections')}
          </MainMenuButton>

          {/* will show when user has permission or FGA is not enabled for user. */}
          {user?.hasPermissions?.(Permission.CanViewConnectors) !== false && (
            <MainMenuButton
              header={t('headers.connectors')}
              onClick={onClose}
              to={
                scopeLocation?.twin?.id
                  ? routes.connectors_scope__scopeId(scopeLocation.twin.id)
                  : routes.connectors
              }
            >
              {t('plainText.viewAndInstallConnectors')}
            </MainMenuButton>
          )}

          <MainMenuButton
            header={t('headers.timeSeries')}
            onClick={onClose}
            to={
              isScopeSelectorEnabled && scopeLocation?.twin?.id
                ? routes.timeSeries_scope__scopeId(scopeLocation.twin.id)
                : routes.timeSeries
            }
          >
            {t('plainText.compareLiveTrends')}
          </MainMenuButton>
          {/* POC for DFW
              reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/90924
          */}
          {featureFlags?.hasFeatureToggle('mapView') && (
            <MainMenuButton
              header="Map Viewer"
              onClick={onClose}
              to={routes.map_viewer}
            >
              Visualize trends across the map
            </MainMenuButton>
          )}
          {/*
            To be used by dashboard (data) team only
            https://dev.azure.com/willowdev/Unified/_workitems/edit/92625
          */}
          {featureFlags?.hasFeatureToggle('experimentalDashboards') &&
            isScopeSelectorEnabled && (
              <MainMenuButton
                header="Experimental Dashboards"
                onClick={onClose}
                to={
                  scopeLocation?.twin?.id
                    ? routes.experimental_dashboards_scope__scopeId(
                        scopeLocation.twin.id
                      )
                    : routes.experimental_dashboards
                }
              >
                Internal Use Only by Dashboard Team
              </MainMenuButton>
            )}
          {classicExplorerLandingPath && (
            <MainMenuButton
              defaultTile="3D"
              header={t('plainText.3dViewer')}
              onClick={onClose}
              to={classicExplorerLandingPath}
            >
              {t('plainText.visualizeTrendsAcross3D')}
            </MainMenuButton>
          )}
        </>
      )}

      {user.showAdminMenu && (
        <>
          {site !== null && <hr />}
          <MainMenuButton
            header={t('headers.admin')}
            onClick={onClose}
            to="/admin"
          >
            {t('plainText.adminSection')}
          </MainMenuButton>
        </>
      )}
    </Drawer>
  )
}
