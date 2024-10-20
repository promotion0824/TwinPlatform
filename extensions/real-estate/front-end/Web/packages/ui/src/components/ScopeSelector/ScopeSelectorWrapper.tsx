import { useScopeSelector } from '@willow/ui'

import ScopeSelector, { LocationNode } from './ScopeSelector'

interface ScopeSelectorWrapperProps {
  onLocationChange?: (location: LocationNode) => void
}

const ScopeSelectorWrapper = ({
  onLocationChange,
}: ScopeSelectorWrapperProps) => {
  const scopeSelector = useScopeSelector()
  const { twinQuery } = scopeSelector

  return twinQuery.data ? (
    <ScopeSelector
      defaultLocation={scopeSelector.location}
      locations={twinQuery.data}
      onLocationChange={onLocationChange}
    />
  ) : (
    <></>
  )
}

export default ScopeSelectorWrapper
