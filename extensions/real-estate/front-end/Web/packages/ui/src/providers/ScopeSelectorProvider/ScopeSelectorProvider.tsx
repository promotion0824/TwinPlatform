import { titleCase } from '@willow/common'
import { api, useFeatureFlag } from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { uniqBy } from 'lodash'
import { useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import { useLocation } from 'react-router'

import routes from '../../../../platform/src/routes'
import { flattenTree } from '../../components/ScopeSelector/scopeSelectorUtils'
import { ScopeSelectorContext } from './ScopeSelectorContext'
import useGetCurrentScope from './useGetCurrentScope'

export { useScopeSelector } from './ScopeSelectorContext'

export function ScopeSelectorProvider({
  children,
}: {
  children: React.ReactNode
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const currentScopeRef = useRef<
    | {
        scope: LocationNode | undefined
        scopeId: string | undefined
      }
    | undefined
  >()
  const { pathname } = useLocation()
  const isScopeSelectorEnabled =
    !!useFeatureFlag()?.hasFeatureToggle('scopeSelector')

  const twinQuery = useQuery(
    ['scopes', 'twins'],
    async () => {
      const response = await api.get('/v2/twins/tree')
      return response.data
    },
    {
      select: (data) => uniqBy(data, (input: LocationNode) => input.twin.id),
      enabled: isScopeSelectorEnabled,
    }
  )
  const flattenedLocationList = flattenTree(twinQuery.data ?? [])
  const scopeLookup: Record<string, LocationNode> = {}
  flattenedLocationList.forEach((location) => {
    scopeLookup[location.twin.id] = location
    if (
      location.twin.siteId &&
      possibleBuildingScopeModelIds.includes(location.twin.metadata.modelId)
    ) {
      scopeLookup[location.twin.siteId] = location
    }
  })
  const currentScope = useGetCurrentScope({
    pathname,
    locations: flattenedLocationList,
  })

  if (
    !scopelessRoutes.some((scopelessRoute) => pathname.includes(scopelessRoute))
  ) {
    currentScopeRef.current = currentScope
  }

  const location = isScopeSelectorEnabled
    ? currentScopeRef.current?.scope
    : undefined

  const context = {
    locationName: isScopeSelectorEnabled
      ? location?.twin?.name ||
        titleCase({ text: t('headers.allLocations'), language })
      : undefined,
    location,
    twinQuery,
    isScopeSelectorEnabled,
    scopeId: currentScope?.scopeId,
    descendantSiteIds: (currentScope == null
      ? flattenedLocationList
      : flattenTree(currentScope?.scope?.children ?? [])
    )
      .filter((node) => isScopeUsedAsBuilding(node))
      .map((node) => node.twin.siteId)
      .filter((siteId): siteId is string => siteId != null),
    flattenedLocationList,
    scopeLookup,
    isScopeUsedAsBuilding,
  }

  return (
    <ScopeSelectorContext.Provider value={context}>
      {children}
    </ScopeSelectorContext.Provider>
  )
}

/**
 * the following routes are not scoped, so we don't want to update the scope
 * when the user navigates to these routes
 */
const scopelessRoutes = [
  routes.admin,
  routes.admin_models_of_interest,
  routes.admin_notification_settings,
  routes.admin_notification_settings_add,
  routes.admin_portfolios,
  routes.admin_requestors,
  routes.admin_sandbox,
  routes.admin_users,
  routes.admin_workgroups,
  routes.map_viewer,
  routes.notifications,
  routes.portfolio_twins_view,
]

export const isScopeUsedAsBuilding = (scope?: LocationNode) =>
  !!scope?.twin?.siteId &&
  possibleBuildingScopeModelIds.includes(scope?.twin?.metadata?.modelId)

/**
 * Once backend finishes working on the bug: https://dev.azure.com/willowdev/Unified/_workitems/edit/126580
 * We can remove this arbitrary list as BE will be adding an extra field to help us identify the building scope
 */
const possibleBuildingScopeModelIds = [
  'dtmi:com:willowinc:Building;1',
  'dtmi:com:willowinc:airport:AirportTerminal;1',
  'dtmi:com:willowinc:Substructure;1',
  'dtmi:com:willowinc:OutdoorArea;1',
]
