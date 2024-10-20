/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'
import * as activities from './activities'
import * as allInsights from './allInsights'
import * as assets from './assets'
import * as buildingData from './buildingData'
import categoryTree from './categoryTree'
import * as contactUsCategories from './contactUsCategories'
import * as copilot from './copilot'
import * as dataQualities from './dataQualities'
import * as dataQualityValidations from './dataQualityValidations'
import * as diagnostics from './diagnostics'
import * as diagnosticSummary from './diagnosticSummary'
import * as equipment from './equipment'
import floors from './floors'
import * as graph from './graph'
import * as insights from './insights'
import * as insightSnackbarsStatus from './insightSnackbarsStatus'
import * as insightTypes from './insightTypes'
import * as inspections from './inspections'
import * as me from './me'
import * as models from './models'
import * as modelsOfInterest from './modelsOfInterest'
import * as myWorkgroups from './myWorkgroups'
import * as categories from './notificationCategories'
import * as notificationItems from './notificationItems'
import * as notificationSettingsDelete from './notificationSettingsDelete'
import * as occurrences from './occurrences'
import * as ticketCategories from './ticketCategories'
import * as tickets from './tickets'
import * as timeZones from './timeZones'
import * as twins from './twins'
import { csvify } from './utils'
import * as widgets from './widgets'
import * as workGroupSelectors from './workGroupSelector'

/**
 * This is a little in flux. We now have a concept called a "route collection"
 * which contains a list of handlers, some internal state, and a reset
 * function. This enables us to reset the server's internal state in between
 * tests, and lets us inspect the server's internal state if we want to.
 * Currently the models of interest endpoints are the only endpoints that make
 * use of this.
 */

function makeRouteCollection(handlers) {
  return {
    handlers,
    state: null,
    reset: () => {},
  }
}

export function makeRouteCollections() {
  return [
    makeRouteCollection(me.handlers),
    makeRouteCollection(twins.handlers),
    makeRouteCollection(models.handlers),
    makeRouteCollection(graph.handlers),
    makeRouteCollection(assets.handlers),
    makeRouteCollection(equipment.handlers),
    makeRouteCollection(inspections.handlers),
    makeRouteCollection(buildingData.handlers),
    makeRouteCollection(insights.handlers),
    makeRouteCollection(copilot.handlers),
    makeRouteCollection(notificationItems.handlers),
    makeRouteCollection(myWorkgroups.handlers),

    makeRouteCollection(allInsights.handlers),
    makeRouteCollection(insightTypes.handlers),
    makeRouteCollection(tickets.handlers),
    makeRouteCollection(widgets.handlers),
    makeRouteCollection(activities.handlers),
    makeRouteCollection(ticketCategories.handlers),
    makeRouteCollection(workGroupSelectors.handlers),
    makeRouteCollection(contactUsCategories.handlers),
    makeRouteCollection(occurrences.handlers),
    makeRouteCollection(diagnostics.handlers),
    makeRouteCollection(insightSnackbarsStatus.handlers),
    makeRouteCollection(timeZones.handlers),
    makeRouteCollection(diagnosticSummary.handlers),
    makeRouteCollection(dataQualities.handlers),
    makeRouteCollection(dataQualityValidations.handlers),
    makeRouteCollection(categories.handlers),
    modelsOfInterest.makeHandlers(),
    notificationSettingsDelete.makeHandlers(),

    makeRouteCollection([
      rest.get('/:region/api/sites/:siteId/floors', (req, res, ctx) =>
        res(ctx.json(floors))
      ),

      rest.get('/:region/api/sites/:siteId/categoryTree', (req, res, ctx) =>
        res(ctx.json(categoryTree))
      ),

      rest.post('/:region/api/livedata/export/csv', (req, res, ctx) => {
        const rows = req.body.points.map((point) => [
          point.pointId,
          '999',
          JSON.stringify({
            isTicketingDisabled: false,
            some: 'other properties',
          }),
        ])

        return res(ctx.text(csvify(rows)))
      }),
    ]),
  ]
}
