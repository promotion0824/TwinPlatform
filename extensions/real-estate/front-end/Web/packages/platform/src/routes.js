/*
  All the routes in the app.

  All segments in the path should be kebab-cased.
  When converting from the route to the variable, change the
  kebab-case segments into camelCase, and the slashes to underscores.

  For example, a route of '/my-long-route/with/some/long-segments'
  becomes the variable 'myLongRoute_with_some_longSegments'

  This allows us to both use typescript to see that we have the route
  correct in routes and links, and also instantly read what that route
  is for the user. The long variable names are a feature, not a bug.

  When changing a path, also change the variable everywhere in the
  code (easy with typescript), and reject PRs that do not do this.

  Params should be functions, separated in their name by double underscores.
  The function should accept strings, and default to the name of the parameter with
  the colon prefixing it. For example:
  portfolio_twins_view__twinId: (twinId = ":twinId") => `/portfolio/twins/view/${twinId}`,

  Please keep in alphabetical order, to minimize merge conflicts.
*/

const routes = {
  account_idle_timeout: '/account/idle-timeout',
  account_login: '/account/login',
  admin: '/admin',
  admin_models_of_interest: '/admin/modelsOfInterest',
  admin_notification_settings: '/admin/notification-settings',
  admin_notification_settings_add: '/admin/notification-settings/add',
  admin_notification_settings__triggerId_edit: (triggerId = ':triggerId') =>
    `/admin/notification-settings/notification-setting/${triggerId}/edit`,
  admin_portfolios: '/admin/portfolios',
  admin_portfolios__portfolioId: (portfolioId = ':portfolioId') =>
    `/admin/portfolios/${portfolioId}`,
  admin_portfolios__portfolioId_connectivity: (portfolioId = ':portfolioId') =>
    `/admin/portfolios/${portfolioId}/connectivity`,
  admin_portfolios__portfolioId_dashboardsConfig: (
    portfolioId = ':portfolioId'
  ) => `/admin/portfolios/${portfolioId}/dashboardsConfig`,
  admin_portfolios__portfolioId_reportsConfig: (portfolioId = ':portfolioId') =>
    `/admin/portfolios/${portfolioId}/reportsConfig`,
  admin_portfolios__portfolioId_sites__siteId_connectors: (
    portfolioId = ':portfolioId',
    siteId = ':siteId'
  ) => `/admin/portfolios/${portfolioId}/sites/${siteId}/connectors`,
  admin_requestors: '/admin/requestors',
  admin_sandbox: '/admin/sandbox',
  admin_users: '/admin/users',
  admin_workgroups: '/admin/workgroups',
  connectors: '/connectors',
  connectors_scope__scopeId: (scopeId = ':scopeId') =>
    `/connectors/scope/${scopeId}`,
  connectors_scope__scopeId_connector__connectorId: (
    scopeId = ':scopeId',
    connectorId = ':connectorId'
  ) => `/connectors/scope/${scopeId}/connector/${connectorId}`,
  connectors_sites: '/connectors/sites/',
  connectors_sites__siteId: (siteId = ':siteId') =>
    `/connectors/sites/${siteId}`,
  connectors_sites__siteId_connector__connectorId: (
    siteId = ':siteId',
    connectorId = ':connectorId'
  ) => `/connectors/sites/${siteId}/connector/${connectorId}`,
  dashboards: '/dashboards',
  dashboards_scope__scopeId: (scopeId = ':scopeId') =>
    `/dashboards/scope/${scopeId}`,
  dashboards_sites__siteId: (siteId = ':siteId') =>
    `/dashboards/sites/${siteId}`,
  experimental_dashboards: '/experimental-dashboards',
  experimental_dashboards_scope__scopeId: (scopeId = ':scopeId') =>
    `/experimental-dashboards/scope/${scopeId}`,
  experimental_dashboards_sites__siteId: (siteId = ':siteId') =>
    `/sites/${siteId}/experimental-dashboards`,
  home: '/',
  home_scope__scopeId: (scopeId = ':scopeId') => `/scope/${scopeId}`,
  insights: '/insights',
  insights_insight__insightId: (insightId = ':insightId') =>
    `/insights/insight/${insightId}`,
  insights_insightId: (insightId = ':insightId') => `/insights/${insightId}`,
  insights_rule__ruleId: (ruleId = ':ruleId') =>
    `/insights/insight-rule/${ruleId}`,
  insights_scope__scopeId: (scopeId = ':scopeId') =>
    `/insights/scope/${scopeId}`,
  insights_scope__scopeId_insight__insightId: (
    scopeId = ':scopeId',
    insightId = ':insightId'
  ) => `/insights/scope/${scopeId}/insight/${insightId}`,
  insights_scope__scopeId_rule__ruleId: (
    scopeId = ':scopeId',
    ruleId = ':ruleId'
  ) => `/insights/scope/${scopeId}/rule/${ruleId}`,
  inspections: '/inspections',
  inspections__inspectionId__checkId: (
    inspectionId = ':inspectionId',
    checkId = ':checkId'
  ) => `/inspections/${inspectionId}/checks/${checkId}`,
  inspections_inspection__inspectionId_check__checkId: (
    inspectionId = ':inspectionId',
    checkId = ':checkId'
  ) => `/inspections/inspection/${inspectionId}/check/${checkId}`,
  inspections_scope__scopeId: (scopeId = ':scopeId') =>
    `/inspections/scope/${scopeId}`,
  inspections_scope__scopeId_inspection__inspectionId_check__checkId: (
    scopeId = ':scopeId',
    inspectionId = ':inspectionId',
    checkId = ':checkId'
  ) =>
    `/inspections/scope/${scopeId}/inspection/${inspectionId}/check/${checkId}`,
  inspections_scope__scopeId_usage: (scopeId = ':scopeId') =>
    `/inspections/scope/${scopeId}/usage`,
  inspections_scope__scopeId_zones: (scopeId = ':scopeId') =>
    `/inspections/scope/${scopeId}/zones`,
  inspections_scope__scopeId_zones_zone__zoneId: (
    scopeId = ':scopeId',
    zoneId = ':zoneId'
  ) => `/inspections/scope/${scopeId}/zones/zone/${zoneId}`,
  map_viewer: '/map-viewer',
  notifications: '/notifications',
  portfolio: '/portfolio',
  portfolio_reports: '/portfolio/reports',
  portfolio_reports__siteId: (siteId = ':siteId') =>
    `/portfolio/reports/sites/${siteId}`,
  portfolio_twins: '/portfolio/twins',
  portfolio_twins_results: '/portfolio/twins/results',
  portfolio_twins_scope__scopeId_results: (scopeId = ':scopeId') =>
    `/portfolio/twins/scope/${scopeId}/results`,
  portfolio_twins_view: '/portfolio/twins/view',
  portfolio_twins_view__siteId__twinId: (
    siteId = ':siteId',
    twinId = ':twinId'
  ) => `/portfolio/twins/view/${siteId}/${twinId}`,
  portfolio_twins_view__twinId: (twinId = ':twinId') =>
    `/portfolio/twins/view/${twinId}`,
  reports: '/reports',
  reports_scope__scopeId: (scopeId = ':scopeId') => `/reports/scope/${scopeId}`,
  sites: '/sites',
  sites__siteId: (siteId = ':siteId') => `/sites/${siteId}`,
  sites__siteId_floors__floorId: (siteId = ':siteId', floorId = ':floorId') =>
    `/sites/${siteId}/floors/${floorId}`,
  sites__siteId_insight_rule__ruleId: (
    siteId = ':siteId',
    ruleId = ':ruleId'
  ) => `/sites/${siteId}/insight-rule/${ruleId}`,
  sites__siteId_insights: (siteId = ':siteId') => `/sites/${siteId}/insights`,
  sites__siteId_insights__insightId: (
    siteId = ':siteId',
    insightId = ':insightId'
  ) => `/sites/${siteId}/insights/${insightId}`,
  sites__siteId_inspections: (siteId = ':siteId') =>
    `/sites/${siteId}/inspections`,
  sites__siteId_inspections__inspectionId__checkId: (
    siteId = ':siteId',
    inspectionId = ':inspectionId',
    checkId = ':checkId'
  ) => `/sites/${siteId}/inspections/${inspectionId}/checks/${checkId}`,
  sites__siteId_inspections_usage: (siteId = ':siteId') =>
    `/sites/${siteId}/inspections/usage`,
  sites__siteId_inspections_zones: (siteId = ':siteId') =>
    `/sites/${siteId}/inspections/zones`,
  sites__siteId_inspections_zones__zoneId: (
    siteId = ':siteId',
    zoneId = ':zoneId'
  ) => `/sites/${siteId}/inspections/zones/${zoneId}`,
  sites__siteId_occupancy: (siteId = ':siteId') => `/sites/${siteId}/occupancy`,
  sites__siteId_reports: (siteId = ':siteId') => `/sites/${siteId}/reports`,
  sites__siteId_tickets: (siteId = ':siteId') => `/sites/${siteId}/tickets`,
  sites__siteId_tickets__ticketId: (
    siteId = ':siteId',
    ticketId = ':ticketId'
  ) => `/sites/${siteId}/tickets/${ticketId}`,
  sites__siteId_tickets_scheduled: (siteId = ':siteId') =>
    `/sites/${siteId}/tickets/scheduled-tickets`,
  sites__siteId_tickets_scheduled__ticketId: (
    siteId = ':siteId',
    ticketId = ':ticketId'
  ) => `/sites/${siteId}/tickets/scheduled-tickets/${ticketId}`,
  sites__siteId_tickets_schedules: (siteId = ':siteId') =>
    `/sites/${siteId}/tickets/schedules`,
  tickets: '/tickets',
  tickets_scheduled: '/tickets/scheduled-tickets',
  tickets_scope__scopeId: (scopeId = ':scopeId') => `/tickets/scope/${scopeId}`,
  tickets_scope__scopeId_ticket__ticketId: (
    scopeId = ':scopeId',
    ticketId = ':ticketId'
  ) => `/tickets/scope/${scopeId}/ticket/${ticketId}`,
  tickets_scheduled__ticketId: (ticketId = ':ticketId') =>
    `/tickets/scheduled-tickets/${ticketId}`,
  tickets_scope__scopeId_scheduled: (scopeId = ':scopeId') =>
    `/tickets/scope/${scopeId}/scheduled-tickets`,
  tickets_scope__scopeId_scheduled__ticketId: (
    scopeId = ':scopeId',
    ticketId = ':ticketId'
  ) => `/tickets/scope/${scopeId}/scheduled-tickets/${ticketId}`,
  tickets_schedules: '/tickets/schedules',
  tickets_scope__scopeId_schedules: (scopeId = ':scopeId') =>
    `/tickets/scope/${scopeId}/schedules`,
  tickets_ticketId: (ticketId = ':ticketId') => `/tickets/${ticketId}`,
  tickets_ticket__ticketId: (ticketId = ':ticketId') =>
    `/tickets/ticket/${ticketId}`,
  timeSeries: '/time-series',
  timeSeries_scope__scopeId: (scopeId = ':scopeId') =>
    `/time-series/scope/${scopeId}`,
  timeSeries_addTwin: '/time-series/add-twin',
}

/*
  the following branches object contains branches that are valid routes,
  please note `/sites/${siteId}/floors` is not a valid route, hence floor
  is not included in branches
*/
export const branches = {
  tickets: routes.sites__siteId_tickets,
  insights: routes.sites__siteId_insights,
  occupancy: routes.sites__siteId_occupancy,
  inspections: routes.sites__siteId_inspections,
  reports: routes.sites__siteId_reports,
}

/**
 * if pathname matches pattern of /sites/:siteId/, getSitePage
 * return the first word (called branch) right after the pattern;
 * otherwise, return undefined
 */
export function getSitePage(pathname) {
  const sitePageRegex = /(\/sites\/)(.+?)(\/|$)(?<branch>[a-z-]*)(\/|$)/
  const {
    groups: { branch },
  } = sitePageRegex.exec(pathname) ?? { groups: {} }

  return branch
}

export default routes

/**
 * Returns an insight link based on the current scope, site, and
 * whether the scope selector feature is enabled.
 */
export function makeInsightLink({
  siteId,
  scopeId,
  isScopeSelectorEnabled,
  insightId,
}) {
  if (isScopeSelectorEnabled) {
    return scopeId
      ? routes.insights_scope__scopeId_insight__insightId(scopeId, insightId)
      : routes.insights_insight__insightId(insightId)
  }

  return siteId
    ? routes.sites__siteId_insights__insightId(siteId, insightId)
    : routes.insights_insight__insightId(insightId)
}
