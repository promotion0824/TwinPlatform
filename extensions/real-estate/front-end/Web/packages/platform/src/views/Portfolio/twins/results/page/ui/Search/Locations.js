/* eslint-disable complexity */
import { useState } from 'react'
import styled from 'styled-components'
import { titleCase } from '@willow/common'
import {
  ALL_LOCATIONS,
  caseInsensitiveEquals,
  useAnalytics,
  useFeatureFlag,
  useLanguage,
  useScopeSelector,
} from '@willow/ui'
import { flattenTree, MultiSelectTree, SearchInput } from '@willowinc/ui'

import IconBuildings from './icon.building.svg'
import SearchSidebarTitle from './SearchSidebarTitle'
import { useSearchResults as useSearchResultsInjected } from '../../state/SearchResults'

// The data returns from the tree API route nests all of the properties beneath
// a "twin" property. This brings them all up to the top level and recursively calls
// itself for its children.
function raiseUpTwinProperties(twin) {
  const children = twin.children.map(raiseUpTwinProperties)
  return { children, ...twin.twin }
}

const StyledIcon = styled(({ Icon, ...props }) => <Icon {...props} />)({
  marginRight: '0.5rem',
})

const StyledSearchInput = styled(SearchInput)(({ theme }) => ({
  margin: `${theme.spacing.s12} 0`,
}))

const TreeContainer = styled.div(({ theme }) => ({
  marginLeft: theme.spacing.s32,
}))

const Buildings = ({
  useSearchResults = useSearchResultsInjected,
  locationSelectedTwinIds = [],
}) => {
  const analytics = useAnalytics()
  const featureFlags = useFeatureFlag()
  const language = useLanguage()
  const scopeSelector = useScopeSelector()
  const scopeId = scopeSelector.location?.twin?.id
  const scopeName = scopeSelector.locationName || ''
  const { siteIds, sites, t, updateSiteIds } = useSearchResults()

  const [searchInput, setSearchInput] = useState()

  const scopeSelectorEnabled = featureFlags.hasFeatureToggle('scopeSelector')

  function trackSiteChange(selectedSites) {
    analytics.track('Search & Explore - Site Changed', {
      'Selected Sites': selectedSites.length
        ? selectedSites
        : ['All Locations'],
    })
  }

  const locationChangeHandler = (nodes) => {
    const allLocationsSelected = nodes.length === 1 && nodes[0].isAllItemsNode
    // Currently campuses may have dummy site IDs that need to be filtered out.
    // These should be removed in the future.
    const flattenedTree = flattenTree(nodes).filter(
      (node) => node.siteId !== '00000000-0000-0000-0000-000000000000'
    )

    const newSiteIds = allLocationsSelected
      ? []
      : scopeSelectorEnabled
      ? Array.from(new Set(flattenedTree.map((node) => node.siteId)))
      : nodes.map((node) => node.id)

    const siteNamesToTrack = allLocationsSelected
      ? []
      : scopeSelectorEnabled
      ? flattenedTree.map((node) => node.name)
      : nodes.map((node) => node.name)

    updateSiteIds(newSiteIds)
    trackSiteChange(siteNamesToTrack)
  }

  const selectedTwinIds = siteIds
    .map((siteId) => scopeSelector.scopeLookup[siteId]?.twin?.id)
    .filter((id) => id)

  const isAllLocationsSelected = locationSelectedTwinIds.some(
    (item) => item === ALL_LOCATIONS
  )

  return !scopeSelectorEnabled ||
    (scopeSelectorEnabled && !scopeId) ||
    scopeSelector.location?.children?.length > 0 ? (
    <>
      <SearchSidebarTitle>
        <StyledIcon Icon={IconBuildings} />
        {t('headers.locations')}
      </SearchSidebarTitle>
      <StyledSearchInput
        onChange={(event) => setSearchInput(event.currentTarget.value)}
        placeholder={t('placeholder.filterLocations')}
        value={searchInput}
      />

      <TreeContainer>
        {scopeSelectorEnabled ? (
          // TODO: Implement openByDefault prop when  locationSelectedTwinIds.length > 0
          <MultiSelectTree
            allItemsNode={
              locationSelectedTwinIds.length > 0
                ? isAllLocationsSelected
                  ? {
                      id: ALL_LOCATIONS,
                      name: titleCase({
                        language,
                        text: scopeName,
                      }),
                    }
                  : undefined
                : {
                    id: ALL_LOCATIONS,
                    name: titleCase({
                      language,
                      text: scopeName,
                    }),
                  }
            }
            data={
              scopeId
                ? scopeSelector.location.children.map(raiseUpTwinProperties)
                : locationSelectedTwinIds.length > 0
                ? isAllLocationsSelected
                  ? (scopeSelector.twinQuery?.data || [])?.map(
                      raiseUpTwinProperties
                    )
                  : scopeSelector.twinQuery?.data
                      ?.filter((item) =>
                        locationSelectedTwinIds.includes(item.twin.id)
                      )
                      ?.map(raiseUpTwinProperties)
                : (scopeSelector.twinQuery?.data || [])?.map(
                    raiseUpTwinProperties
                  )
            }
            onChange={locationChangeHandler}
            searchTerm={searchInput}
            selection={
              siteIds.length
                ? getAllSelectedTwinIds({
                    nodes: scopeSelector.flattenedLocationList,
                    ids: selectedTwinIds,
                  })
                : locationSelectedTwinIds.length > 0
                ? isAllLocationsSelected
                  ? [ALL_LOCATIONS]
                  : getAllSelectedTwinIds({
                      nodes: scopeSelector.flattenedLocationList,
                      ids: selectedTwinIds,
                    })
                : [ALL_LOCATIONS]
            }
          />
        ) : (
          <MultiSelectTree
            allItemsNode={{
              id: ALL_LOCATIONS,
              name: titleCase({
                language,
                text: scopeName,
              }),
            }}
            data={sites}
            onChange={locationChangeHandler}
            searchTerm={searchInput}
            selection={siteIds.length ? siteIds : [ALL_LOCATIONS]}
          />
        )}
      </TreeContainer>
    </>
  ) : null
}

export default Buildings

/**
 * Given a list of twinIds of site nodes and a list of LocationNode, return
 * twinIds of all nodes satisfying the following conditions:
 * 1. It is an ancestor of any of the nodes in the list
 * 2. All of its descendants are included in the list
 */
function getAllSelectedTwinIds({ nodes, ids }) {
  const result = [...ids]

  function allChildrenIncluded(node) {
    return node.children.every((child) => result.includes(child.twin.id))
  }

  function checkParents(node) {
    nodes.forEach((nextNode) => {
      if (
        nextNode.children.some((child) => child.twin.id === node.twin.id) &&
        allChildrenIncluded(nextNode)
      ) {
        if (!result.includes(nextNode.twin.id)) {
          result.push(nextNode.twin.id)
          checkParents(nextNode)
        }
      }
    })
  }

  ids.forEach((id) => {
    const node = nodes.find((i) => i.twin.id === id)
    if (node) {
      checkParents(node)
    }
  })

  return result
}
