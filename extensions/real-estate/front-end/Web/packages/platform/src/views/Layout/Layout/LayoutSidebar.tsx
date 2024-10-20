/* eslint-disable complexity */
import { titleCase } from '@willow/common'
import {
  Permission,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { Sidebar, SidebarGroup, SidebarLink, SidebarProps } from '@willowinc/ui'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import routes from '../../../routes'
import { useClassicExplorerLandingPath } from './Header/utils'

interface LayoutSidebarProps extends Omit<SidebarProps, 'children'> {
  /** Called whenever a SidebarLink is clicked. */
  onLinkClick?: () => void
}

export default forwardRef<HTMLDivElement, LayoutSidebarProps>(
  ({ onLinkClick, ...restProps }, ref) => {
    const featureFlags = useFeatureFlag()
    const { scopeId } = useScopeSelector()
    const {
      i18n: { language },
      t,
    } = useTranslation()
    const user = useUser()

    const classicExplorerLandingPath = useClassicExplorerLandingPath({
      hasBaseModuleOption: { hasBaseModule: false },
    })

    return (
      <Sidebar ref={ref} {...restProps}>
        <SidebarGroup>
          <SidebarLink
            component={Link}
            icon="home"
            isActive={
              window.location.pathname === routes.home ||
              window.location.pathname === routes.home_scope__scopeId(scopeId)
            }
            label={t('headers.home')}
            onClick={onLinkClick}
            to={scopeId ? routes.home_scope__scopeId(scopeId) : routes.home}
          />
        </SidebarGroup>
        <SidebarGroup>
          <SidebarLink
            component={Link}
            icon="category"
            isActive={window.location.pathname.startsWith(
              routes.portfolio_twins
            )}
            label={t('headers.searchAndExplore')}
            onClick={onLinkClick}
            to={
              scopeId
                ? routes.portfolio_twins_scope__scopeId_results(scopeId)
                : routes.portfolio_twins_results
            }
          />
          <SidebarLink
            component={Link}
            icon="emoji_objects"
            isActive={window.location.pathname.startsWith(routes.insights)}
            label={t('headers.insights')}
            onClick={onLinkClick}
            to={`${
              scopeId
                ? routes.insights_scope__scopeId(scopeId)
                : routes.insights
            }${
              user.localOptions?.insightsGroupBy
                ? `?groupBy=${user.localOptions.insightsGroupBy}`
                : ''
            }`}
          />
          <SidebarLink
            component={Link}
            icon="assignment"
            isActive={window.location.pathname.startsWith(routes.tickets)}
            label={t('headers.tickets')}
            onClick={onLinkClick}
            to={
              scopeId ? routes.tickets_scope__scopeId(scopeId) : routes.tickets
            }
          />
          <SidebarLink
            component={Link}
            icon="assignment_turned_in"
            isActive={window.location.pathname.startsWith(routes.inspections)}
            label={t('headers.inspections')}
            onClick={onLinkClick}
            to={
              scopeId
                ? routes.inspections_scope__scopeId(scopeId)
                : routes.inspections
            }
          />
        </SidebarGroup>
        <SidebarGroup>
          <SidebarLink
            component={Link}
            icon="dashboard"
            isActive={window.location.pathname.startsWith(routes.dashboards)}
            label={t('headers.dashboards')}
            onClick={onLinkClick}
            to={
              scopeId
                ? routes.dashboards_scope__scopeId(scopeId)
                : routes.dashboards
            }
          />
          <SidebarLink
            component={Link}
            icon="lab_profile"
            isActive={window.location.pathname.startsWith(routes.reports)}
            label={t('headers.reports')}
            onClick={onLinkClick}
            to={
              scopeId ? routes.reports_scope__scopeId(scopeId) : routes.reports
            }
          />

          {featureFlags.hasFeatureToggle('experimentalDashboards') && (
            <SidebarLink
              component={Link}
              icon="experiment"
              isActive={window.location.pathname.startsWith(
                routes.experimental_dashboards
              )}
              label="Experimental Dashboards"
              onClick={onLinkClick}
              to={
                scopeId
                  ? routes.experimental_dashboards_scope__scopeId(scopeId)
                  : routes.experimental_dashboards
              }
            />
          )}
        </SidebarGroup>
        <SidebarGroup fill>
          <SidebarLink
            component={Link}
            icon="timeline"
            isActive={window.location.pathname.startsWith(routes.timeSeries)}
            label={t('headers.timeSeries')}
            onClick={onLinkClick}
            to={
              scopeId
                ? routes.timeSeries_scope__scopeId(scopeId)
                : routes.timeSeries
            }
          />

          {!!classicExplorerLandingPath && (
            <SidebarLink
              component={Link}
              icon="deployed_code"
              isActive={window.location.pathname === classicExplorerLandingPath}
              label={t('plainText.3dViewer')}
              onClick={onLinkClick}
              to={classicExplorerLandingPath}
            />
          )}

          {featureFlags.hasFeatureToggle('mapView') && (
            <SidebarLink
              component={Link}
              icon="map"
              isActive={window.location.pathname === routes.map_viewer}
              label={titleCase({ language, text: t('headers.mapViewer') })}
              onClick={onLinkClick}
              to={routes.map_viewer}
            />
          )}
        </SidebarGroup>

        {(user.hasPermissions?.(Permission.CanViewConnectors) !== false ||
          user.showAdminMenu) && (
          <SidebarGroup>
            {user.hasPermissions?.(Permission.CanViewConnectors) !== false && (
              <SidebarLink
                component={Link}
                icon="cable"
                isActive={window.location.pathname.startsWith(
                  routes.connectors
                )}
                label={t('headers.connectors')}
                onClick={onLinkClick}
                to={
                  scopeId
                    ? routes.connectors_scope__scopeId(scopeId)
                    : routes.connectors
                }
              />
            )}

            {user.showAdminMenu && (
              <SidebarLink
                component={Link}
                icon="settings"
                isActive={window.location.pathname.startsWith(routes.admin)}
                label={t('headers.admin')}
                onClick={onLinkClick}
                to={routes.admin}
              />
            )}
          </SidebarGroup>
        )}
      </Sidebar>
    )
  }
)
