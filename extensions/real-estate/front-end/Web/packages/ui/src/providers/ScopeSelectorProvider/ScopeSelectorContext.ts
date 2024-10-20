import { UseQueryResult } from 'react-query'
import { createContext, useContext } from 'react'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'

export interface ScopeSelectorContextProps {
  /**
   * Name of current selected scope, will be either 'All Locations' or the selected twin name.
   * If scopeSelector feature flag is disabled, this will be undefined.
   */
  locationName?: string
  location?: LocationNode
  twinQuery: UseQueryResult<LocationNode[], unknown>
  isScopeSelectorEnabled: boolean
  /** to be utilized when querying data */
  scopeId?: string
  /**
   * for backward compatibility, this is used when selected location is not a building twin
   * and we need to find all the site ids for buildings under the selected location
   */
  descendantSiteIds?: string[]
  flattenedLocationList: LocationNode[]
  /**
   * a lookup table for all the locations in the tree,
   * returns the location node for a given twinId or siteId
   */
  scopeLookup: Record<string, LocationNode>
  /**
   * Give a LocationNode, this utility function tells whether the location is used as a building scope.
   * For backward compatibility, a building scope is different from anything else in following ways:
   *
   * a) Click on any scope that is not a building scope on Portfolio Landing Page will stay on the same page,
   *    but clicking on a building scope will take user to the building landing page.
   * b) A building scope is a leaf node in Scope Selector and cannot have children.
   * c) Classic Viewer is only available for building scopes.
   * d) Connectors (used to be called Marketplace) is only available for building scopes.
   * e) Dashboards for building scopes are different from other scopes in the sense that
   *    dashboards for non-building scopes is same as Portfolio Level dashboard with descendants site Ids
   *    included in the query.
   * f) Insights, Tickets, Scheduled Tickets, Inspections are available for building scope if these
   *    features are enabled for the building; these features are available for all other scopes if
   *    at least 1 descendant building has the feature enabled.
   * g) Schedules (ticket schedules) are only available for building scopes.
   */
  isScopeUsedAsBuilding: (scope?: LocationNode) => boolean
}

export const ScopeSelectorContext = createContext<
  ScopeSelectorContextProps | undefined
>(undefined)

export function useScopeSelector() {
  const context = useContext(ScopeSelectorContext)
  if (!context) {
    throw new Error('useScopeSelector requires a ScopeSelectorContext provider')
  }
  return context
}
