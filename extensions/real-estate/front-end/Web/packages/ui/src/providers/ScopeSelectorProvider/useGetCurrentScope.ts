import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { isScopeUsedAsBuilding } from './ScopeSelectorProvider'

const useGetCurrentScope = ({
  locations,
  pathname,
}: {
  locations?: LocationNode[]
  pathname: string
}) => {
  // match scopeId and siteId that is either a word characters or "-"
  const scopeIdRegex = /(\/scope\/)(?<scopeId>.+?)(\/|$)/
  const siteIdRegex = /(\/sites\/)(?<siteId>.+?)(\/|$)/

  const scopeId = pathname.match(scopeIdRegex)?.groups?.scopeId
  const siteId = pathname.match(siteIdRegex)?.groups?.siteId

  for (const { id, isSite } of [
    { id: scopeId },
    { id: siteId, isSite: true },
  ]) {
    if (id) {
      const scope = locations?.find((location) =>
        // scope used as a "site" or "building" is leaf node and cannot have children
        isSite
          ? location.twin.siteId === id && isScopeUsedAsBuilding(location)
          : location?.twin?.id === id
      )

      // leave the door open for handling cases where
      // scopeId isn't found in the locations list
      return scope
        ? {
            scope,
            scopeId: scope.twin.id,
          }
        : undefined
    }
  }
  return undefined
}

export default useGetCurrentScope
