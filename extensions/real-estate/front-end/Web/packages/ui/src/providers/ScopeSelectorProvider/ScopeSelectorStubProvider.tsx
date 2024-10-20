import { noop } from 'lodash'
import { UseQueryResult } from 'react-query'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { ScopeSelectorContext } from './ScopeSelectorContext'

export default function ScopeSelectorStubProvider({
  twinQuery = {
    data: undefined,
    status: 'idle',
  } as UseQueryResult<LocationNode[], unknown>,
  children,
  scopeLocation,
  isScopeSelectorEnabled,
  isScopeUsedAsBuilding,
}: {
  twinQuery?: UseQueryResult<LocationNode[], unknown>
  children: React.ReactNode
  scopeLocation?: LocationNode
  isScopeSelectorEnabled: boolean
  isScopeUsedAsBuilding: (scope?: LocationNode) => boolean
}) {
  const context = {
    location: scopeLocation,
    onLocationChange: noop,
    twinQuery,
    isScopeSelectorEnabled,
    flattenedLocationList: [],
    scopeLookup: {},
    isScopeUsedAsBuilding,
  }

  return (
    <ScopeSelectorContext.Provider value={context}>
      {children}
    </ScopeSelectorContext.Provider>
  )
}
