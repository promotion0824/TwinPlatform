import { titleCase } from '@willow/common'
import {
  fileModelId,
  sensorModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import { useAnalytics, useFeatureFlag } from '@willow/ui'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

import { useSearchResults as InjectedSearchResults } from '../../state/SearchResults'
import InjectedCategories from './Categories'
import InjectedExploreTwins from './ExploreTwins'
import InjectedFileType from './FileType'
import InjectedLocations from './Locations'
import SearchSidebarTitle from './SearchSidebarTitle'
import InjectedSearchTypeSelector from './SearchTypeSelector'
import InjectedTextSearch from './TextSearch'
import IconSearch from './icn.search.alt.svg'
import IconCategories from './icon.assets.svg'
import IconFileType from './icon.folder.filter.svg'
import IconExplore from './icon.view.svg'

const StyledIcon = styled(({ Icon, ...props }) => <Icon {...props} />)({
  marginRight: '0.5rem',
})

const SearchContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  padding: theme.spacing.s16,
}))

const Search = ({
  hideSearchInput = false,
  TextSearch = InjectedTextSearch,
  SearchTypeSelector = InjectedSearchTypeSelector,
  ExploreTwins = InjectedExploreTwins,
  Categories = InjectedCategories,
  Locations = InjectedLocations,
  FileType = InjectedFileType,
  typeaheadZIndex = 'var(--z-dropdown)',
  useSearchResults = InjectedSearchResults,
  searchTypeSelector = 'timeSeries',
  selectedTwinIds = [],
}) => {
  const analytics = useAnalytics()

  const {
    filterInput,
    setFilterInput,

    modelId,
    changeModelId,

    searchType,
    changeSearchType,

    isCapabilityOfInput,
    setIsCapabilityOfInput,

    isCapabilityOfModelId,
    onIsCapabilityOfModelIdChange,

    ontology,
    t,
  } = useSearchResults()

  const {
    i18n: { language },
  } = useTranslation()

  const featureFlag = useFeatureFlag()

  useEffect(
    () => {
      // This is to handle redirection from the search landing page
      if (modelId === fileModelId) changeSearchType('files')
      else if (modelId === sensorModelId) changeSearchType('sensors')
    },
    // only depends on modelId, otherwise it will rerender infinitely
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [modelId]
  )

  return (
    <SearchContainer>
      {!hideSearchInput && (
        <>
          <SearchSidebarTitle>
            <StyledIcon Icon={IconSearch} />
            {t('labels.search')}
          </SearchSidebarTitle>

          <TextSearch
            enableAutomaticSearch={featureFlag.hasFeatureToggle(
              'cognitiveSearch'
            )}
          />

          {searchTypeSelector === 'timeSeries' ? <SearchTypeSelector /> : null}
        </>
      )}

      {searchType === 'files' && (
        <>
          <SearchSidebarTitle>
            <StyledIcon Icon={IconFileType} />
            {t('plainText.fileType')}
          </SearchSidebarTitle>
          <FileType />
        </>
      )}

      {searchType === 'twins' && (
        <>
          {searchTypeSelector !== 'twinCategory' ? (
            <>
              <SearchSidebarTitle>
                <StyledIcon Icon={IconExplore} />
                {t('plainText.explore')}
              </SearchSidebarTitle>
              <ExploreTwins />
            </>
          ) : null}
          <SearchSidebarTitle>
            <StyledIcon Icon={IconCategories} />
            {t('plainText.categories')}
          </SearchSidebarTitle>
          <Categories
            filterInput={filterInput}
            modelId={modelId}
            onFilterChange={setFilterInput}
            onModelIdChange={(newModelId) => {
              const selectedTwinCategory = newModelId
                ? ontology.getModelById(newModelId).displayName.en
                : 'All Categories'
              analytics.track('Search & Explore - Twin Category Changed', {
                'Selected Twin Category': selectedTwinCategory,
              })
              changeModelId(newModelId)
            }}
            typeaheadZIndex={typeaheadZIndex}
            topCategoryIds={
              searchTypeSelector === 'twinCategory'
                ? ['dtmi:com:willowinc:Asset;1', 'dtmi:com:willowinc:Space;1']
                : undefined
            }
          />
        </>
      )}

      {searchType === 'sensors' && (
        <>
          <>
            <SearchSidebarTitle>
              <StyledIcon Icon={IconExplore} />
              {t('plainText.sensorCategories')}
            </SearchSidebarTitle>
            <Categories
              allCategoriesLabel={titleCase({
                text: t('plainText.allCapabilities'),
                language,
              })}
              allCategoriesModelId="dtmi:com:willowinc:Capability;1"
              filterInput={filterInput}
              modelId={modelId}
              onFilterChange={setFilterInput}
              onModelIdChange={(newModelId) => {
                const selectedSensorCategory =
                  newModelId !== 'dtmi:com:willowinc:Capability;1'
                    ? ontology.getModelById(newModelId).displayName.en
                    : 'All Capabilities'
                analytics.track('Search & Explore - Sensor Category Changed', {
                  'Selected Sensor Category': selectedSensorCategory,
                })
                changeModelId(newModelId)
              }}
              topCategoryIds={[
                'dtmi:com:willowinc:Actuator;1',
                'dtmi:com:willowinc:Parameter;1',
                'dtmi:com:willowinc:PerformanceIndicator;1',
                'dtmi:com:willowinc:Sensor;1',
                'dtmi:com:willowinc:State;1',
              ]}
              typeaheadZIndex={typeaheadZIndex}
            />
          </>
          {!featureFlag.hasFeatureToggle('cognitiveSearch') && (
            <>
              <SearchSidebarTitle>
                <StyledIcon Icon={IconExplore} />
                {t('plainText.twinCategories')}
              </SearchSidebarTitle>
              <Categories
                filterInput={isCapabilityOfInput}
                modelId={isCapabilityOfModelId}
                onFilterChange={setIsCapabilityOfInput}
                onModelIdChange={(newModelId) => {
                  const selectedSensorTwinCategory = newModelId
                    ? ontology.getModelById(newModelId).displayName.en
                    : 'All Categories'
                  analytics.track(
                    'Search & Explore - Sensor Twin Category Changed',
                    {
                      'Selected Sensor Twin Category':
                        selectedSensorTwinCategory,
                    }
                  )
                  onIsCapabilityOfModelIdChange(newModelId)
                }}
                typeaheadZIndex={typeaheadZIndex}
              />
            </>
          )}
        </>
      )}
      {searchTypeSelector !== 'twinCategory' && (
        <Locations locationSelectedTwinIds={selectedTwinIds} />
      )}
    </SearchContainer>
  )
}

export default Search
