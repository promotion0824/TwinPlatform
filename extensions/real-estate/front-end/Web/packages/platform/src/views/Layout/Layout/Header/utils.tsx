import { useScopeSelector, useUser } from '@willow/ui'
import { useSite } from '../../../../providers'
import useGetFloors from '../../../../hooks/Floors/useGetFloors'
import routes from '../../../../routes'

/* eslint-disable import/prefer-default-export */
/**
 * This is the path the user is taken to if they click "Classic Explorer" in
 * the hamburger menu, or if they navigate to the to the twin explorer without
 * having that feature enabled.
 */
export function useClassicExplorerLandingPath({
  siteId,
  hasBaseModuleOption,
}: {
  siteId?: string
  hasBaseModuleOption?: { hasBaseModule: boolean }
} = {}) {
  const { id: siteIdFromContext } = useSite()
  const siteIdForNavigation = siteId ?? siteIdFromContext

  const floors = useGetFloors(
    siteIdForNavigation,
    hasBaseModuleOption ?? { hasBaseModule: true },
    {
      onError: (err) => console.error(err),
      enabled: !!siteId || !!siteIdFromContext,
    }
  )

  const floorId = floors.isSuccess ? floors.data[0]?.id : null

  if (floorId != null) {
    return routes.sites__siteId_floors__floorId(siteIdForNavigation, floorId)
  } else {
    return undefined
  }
}

/** get the user selected site id no matter has scope selector enabled or not. */
export function useSelectedSiteId() {
  const user = useUser()
  const { isScopeSelectorEnabled, location: scopeLocation } = useScopeSelector()

  const lastSelectedSiteId = user?.localOptions?.lastSelectedSiteId
  const scopeSelectorSiteId = isScopeSelectorEnabled
    ? scopeLocation?.twin?.siteId || undefined // scopeSelectorContext.location?.twin?.siteId could be empty string
    : undefined

  // Site ID is selected as per AC in
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/68116
  // Site ID could be undefined in which case it means "All Sites" was selected
  return (
    scopeSelectorSiteId ?? lastSelectedSiteId ?? user?.options?.favoriteSiteId
  )
}

export function useHomeUrl(selectedSiteId?: string) {
  const { isScopeSelectorEnabled, location } = useScopeSelector()

  if (isScopeSelectorEnabled) {
    return location?.twin?.id
      ? routes.home_scope__scopeId(location?.twin?.id)
      : routes.home
  }

  return selectedSiteId ? routes.sites__siteId(selectedSiteId) : routes.home
}
