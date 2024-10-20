import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router-dom'
import styled from 'styled-components'

import { ResizeObserverContainer, qs } from '@willow/common'
import {
  ALL_LOCATIONS,
  FILTER_PANEL_BREAKPOINT,
  ScopeSelectorWrapper,
  DocumentTitle,
  useFeatureFlag,
  useScopeSelector,
} from '@willow/ui'
import routes from '../../../../../../routes'
import { LayoutHeader } from '../../../../../Layout'
import { useSearchResults as useSearchResultsInjected } from '../state/SearchResults'
import InjectedResults from './Results/Results'
import InjectedSearch from './Search/Search'
import SearchResultsHeader from './SearchResultsHeader'
import SearchResultsPanels from './SearchResultsPanels'

const LayoutHeaderContainer = styled.div({
  alignItems: 'center',
  display: 'flex',
  height: '100%',
})

export default function SearchResults({
  Results = InjectedResults,
  Search = InjectedSearch,
  useSearchResults = useSearchResultsInjected,
}) {
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()
  const history = useHistory()
  const { locationName } = useScopeSelector()

  const [currentPageWidth, setCurrentPageWidth] = useState(Infinity)
  const showFiltersPanel = currentPageWidth > FILTER_PANEL_BREAKPOINT

  // Remove query params of "siteIds" from search and keep the rest
  // as when the user picks a new scope from scope selector, it is very possible
  // the siteIds are no longer relevant, so we remove them.
  const { siteIds, ...restQueryParams } = qs.parse()

  return (
    <ResizeObserverContainer onContainerWidthChange={setCurrentPageWidth}>
      <DocumentTitle scopes={[t('headers.searchAndExplore'), locationName]} />

      {featureFlags.hasFeatureToggle('scopeSelector') && (
        <LayoutHeader>
          <LayoutHeaderContainer>
            <ScopeSelectorWrapper
              onLocationChange={(location) => {
                const urlWithQueryParams = qs.createUrl(
                  !location?.twin?.id || location.twin.id === ALL_LOCATIONS
                    ? routes.portfolio_twins_results
                    : routes.portfolio_twins_scope__scopeId_results(
                        location.twin.id
                      ),
                  restQueryParams
                )
                history.push(urlWithQueryParams)
              }}
            />
          </LayoutHeaderContainer>
        </LayoutHeader>
      )}

      <SearchResultsHeader
        showFilters={!showFiltersPanel}
        useSearchResults={useSearchResults}
      />

      <SearchResultsPanels
        Results={Results}
        Search={Search}
        showFiltersPanel={showFiltersPanel}
        useSearchResults={useSearchResults}
      />
    </ResizeObserverContainer>
  )
}
