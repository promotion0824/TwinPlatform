import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { getModelDisplayName } from '@willow/common/twins/view/models'
import {
  fileModelId,
  sensorModelId,
  useModelsOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import { api, useFeatureFlag, useScopeSelector } from '@willow/ui'
import { fileExtensionMap } from '@willow/ui/components/FileIcon/FileIcon.tsx'
import _ from 'lodash'
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react'
import { useTranslation } from 'react-i18next'
import { useInfiniteQuery } from 'react-query'

import useOntology from '../../../../../../hooks/useOntologyInPlatform'
import { useSites } from '../../../../../../providers'
import useTwinAnalytics from '../../../useTwinAnalytics'
import getCsv from '../utils/getCsv'

// The backend requires that we generate our own possible extensions
// from a user selected file type. For example, the user selects 'doc'
// but we want to search for 'doc' and 'docx'.
const fileExtensionsFor = _.invertBy(
  _.mapKeys(fileExtensionMap, (v, k) => k.substring(1))
)
const specialTwinTypes = {
  files: fileModelId,
  sensors: sensorModelId,
}

const SearchResultsContext = createContext()
export const useSearchResults = () => useContext(SearchResultsContext)

export default function SearchResultsProvider({ children }) {
  const ontologyQuery = useOntology()
  const scopeSelector = useScopeSelector()
  const scopeId = scopeSelector.location?.twin?.id
  const { t } = useTranslation()
  const translation = useTranslation()
  const featureFlag = useFeatureFlag()
  const analytics = useTwinAnalytics()

  const [searchParams, setSearchParams] = useMultipleSearchParams([
    'term',
    'modelId',
    'fileType',
    {
      name: 'siteIds',
      type: 'array',
    },
    'isCapabilityOfModelId',
    'display',
    'focus',
  ])

  const {
    term,
    modelId,
    fileType,
    siteIds,
    isCapabilityOfModelId,
    display: explicitDisplay,
    focus,
  } = searchParams

  // This provider is shared across 3 pages, which are Search & Explore landing page,
  // SearchResult page and TwinView page. And we need these params for the PageTitle in
  // TwinView page but the url won't include that info.
  // This storedParams will preserve the params states between page navigation.
  const [storedParams, setStoredParams] = useState(searchParams)
  useEffect(() => {
    // the value for searchParams.siteIds was undefined initially,
    // then changes to [].
    // This update will make sure storedParams and searchParams are equal initially
    // for siteIds .
    setStoredParams(searchParams)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const updateParams = (newParams) => {
    setSearchParams(newParams)
    setStoredParams((oldParams) => ({
      ...oldParams,
      ...newParams,
    }))
  }

  const isModelASensor = useCallback(
    (id) => {
      if (!id) return false
      return ontologyQuery.data
        ?.getModelAncestors(id)
        .includes('dtmi:com:willowinc:Capability;1')
    },
    [ontologyQuery]
  )

  useEffect(() => {
    if (!useOntology.isLoading) {
      setSensorSearchEnabled(isModelASensor(modelId))
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ontologyQuery.isLoading])

  const tableDisplayIsDisabled = modelId === fileModelId
  const display = tableDisplayIsDisabled ? 'list' : explicitDisplay || 'list'

  const initialSearchType = 'twins'
  const [filterInput, setFilterInput] = useState('')
  const [isCapabilityOfInput, setIsCapabilityOfInput] = useState('')
  const [hasFileSearch, setHasFileSearch] = useState(true)
  const [searchInput, setSearchInput] = useState(term)
  const [searchType, setSearchType] = useState(initialSearchType)
  const [sensorSearchEnabled, setSensorSearchEnabled] = useState(false)

  // Allows cognitive search to be disabled in specific contexts,
  // even if the feature flag has been enabled for it.
  const [disableCognitiveSearch, setDisableCognitiveSearch] = useState(false)
  const useCognitiveSearch =
    featureFlag.hasFeatureToggle('cognitiveSearch') && !disableCognitiveSearch

  const sites = useSites()
  const getSiteNames = (sIds) =>
    sIds.map((sId) => sites.find((s) => s.id === sId)?.name)
  const siteNames = getSiteNames(siteIds)

  const selectSiteId = (siteId) => {
    updateSiteIds(siteId ? [siteId] : [])
  }

  const toggleSiteId = (siteId) => {
    let newSiteIds
    if (!siteId) {
      // 'All buildings' was selected
      newSiteIds = []
    } else if (siteIds.includes(siteId)) {
      newSiteIds = siteIds.filter((s) => s !== siteId)
    } else {
      newSiteIds = siteIds.concat(siteId)
    }

    updateSiteIds(newSiteIds)
  }

  function updateSiteIds(newSiteIds) {
    // Sort them so they always appear in the same order.
    // This allows bookmarks to be always the same, and the cache to work independent of selection order
    // Also filter out any undefined entries.
    const filteredSiteIds = newSiteIds.filter((s) => s).sort()

    analytics.trackTwinSearch({
      source: 'searchResults',
      display,
      term,
      modelId,
      fileType,
      siteNames: getSiteNames(filteredSiteIds),
      isCapabilityOfModelId,
    })

    updateParams({ siteIds: filteredSiteIds })
  }

  /** The actions required when modelId changes.
   * It won't updates params, but returns the new value for changed params that
   * needs to update.
   */
  const handleModelIdChange = (newModelId) => {
    const model = ontologyQuery.data?.getModelById(newModelId)
    const displayName = model
      ? getModelDisplayName(model, translation)
      : undefined

    setFilterInput(displayName)

    return {
      modelId: newModelId,
      fileType: newModelId === fileModelId ? fileType : undefined,
    }
  }

  const changeModelId = (newModelId) => {
    // Whenever changing the model id, we reset the fileType
    analytics.trackTwinSearch({
      source: 'searchResults',
      display,
      term,
      modelId: newModelId,
      fileType: undefined,
      siteNames,
      isCapabilityOfModelId,
    })

    updateParams(handleModelIdChange(newModelId))
  }

  const changeFileType = (newFileType) => {
    analytics.trackTwinSearch({
      source: 'searchResults',
      display,
      term,
      modelId,
      fileType: newFileType,
      siteNames,
      isCapabilityOfModelId,
    })
    updateParams({ fileType: newFileType })
  }

  const changeTerm = (newTerm) => {
    analytics.trackTwinSearch({
      source: 'searchResults',
      display,
      term: newTerm,
      modelId,
      fileType,
      siteNames,
      isCapabilityOfModelId,
    })
    updateParams({ term: newTerm })
    setSearchInput(newTerm) // also updates searchInput if term changed
  }

  const changeSearchType = (newSearchType) => {
    // get changes for modelId related params based on newSearchType
    let modelIdRelatedParams = {}
    if (newSearchType === 'twins' && modelId != null) {
      modelIdRelatedParams = handleModelIdChange(undefined)
    } else {
      modelIdRelatedParams = handleModelIdChange(
        specialTwinTypes[newSearchType]
      )
    }

    // clear `isCapabilityOfModelId` for searchType 'twins' and 'files'.
    let newIsCapabilityOfModelId = isCapabilityOfModelId
    if (['twins', 'files'].includes(newSearchType) && isCapabilityOfModelId) {
      newIsCapabilityOfModelId = undefined
    }

    // We cannot update params more than once per render,
    // otherwise the later params updates will override the previous updates in
    // setSearchParams.
    updateParams({
      isCapabilityOfModelId: newIsCapabilityOfModelId,
      ...modelIdRelatedParams,
    })
    setSearchType(newSearchType)
  }

  // Resources for results

  const changeDisplay = (newDisplay) => {
    analytics.trackDisplayChange({
      source: 'searchResults',
      display: newDisplay,
      term,
      modelId,
      fileType,
      siteNames,
      isCapabilityOfModelId,
    })
    updateParams({ display: newDisplay })
  }

  const onIsCapabilityOfModelIdChange = (newModelId) => {
    analytics.trackTwinSearch({
      source: 'searchResults',
      display,
      term,
      modelId,
      fileType,
      siteNames,
      isCapabilityOfModelId: newModelId,
    })
    updateParams({ isCapabilityOfModelId: newModelId })

    const model = ontologyQuery.data?.getModelById(newModelId)
    const displayName = model ? getModelDisplayName(model, translation) : ''
    setIsCapabilityOfInput(displayName)
  }

  const resetFilters = (resetSearchInput = true) => {
    if (resetSearchInput) {
      setFilterInput('')
      setIsCapabilityOfInput('')
      setSearchInput('')
    }

    updateParams({
      term: resetSearchInput ? undefined : term,
      siteIds: undefined,
      fileType: undefined,
      ...(searchType === 'twins' ? { modelId: undefined } : {}),
      ...(searchType === 'sensors'
        ? {
            modelId: 'dtmi:com:willowinc:Capability;1',
            isCapabilityOfModelId: undefined,
          }
        : {}),
    })

    analytics.trackTwinSearch({
      source: 'searchResults',
      display,
      term: undefined,
      modelId: undefined,
      fileType: undefined,
      siteNames: [],
      isCapabilityOfModelId: undefined,
    })
  }

  /** Use this to reset the user edited search value in context
   *  to their initial states. */
  const resetAllSearchParams = () => {
    resetFilters()
    // resetFilters will keep searchType for some reason,
    // so need to reset remaining values in this context
    setSearchType(initialSearchType)
    changeModelId(undefined)
  }

  const modelsOfInterestQuery = useModelsOfInterest()

  const sentTerm = term?.trim()
  const queryKey = [
    'twinSearch',
    sentTerm,
    modelId,
    fileType,
    siteIds,
    scopeId,
    isCapabilityOfModelId,
    disableCognitiveSearch,
    sensorSearchEnabled,
  ]

  const twinsQuery = useInfiniteQuery(
    queryKey,
    async ({ pageParam }) => {
      let response
      if (pageParam) {
        analytics.trackTwinSearchResultsAdditionalPage({
          term,
          modelId,
          fileType,
          siteNames,
          display,
          isCapabilityOfModelId,
        })
        response = await api.get(pageParam)
      } else {
        setSensorSearchEnabled(isModelASensor(modelId))

        const searchUrl = useCognitiveSearch
          ? '/twins/cognitiveSearch'
          : '/twins/search'

        response = await api.get(searchUrl, {
          params: {
            term: sentTerm,
            // if focus is `twinCategory` and modeIld is undefined, we need only filtered results
            modelId:
              modelId ??
              (focus === 'twinCategory'
                ? ['dtmi:com:willowinc:Asset;1', 'dtmi:com:willowinc:Space;1']
                : modelId),
            fileTypes: fileExtensionsFor[fileType],
            siteIds,
            scopeId,
            isCapabilityOfModelId,
            sensorSearchEnabled,
          },
        })
        analytics.trackTwinSearchResults({
          term,
          modelId,
          fileType,
          siteNames,
          display,
          isCapabilityOfModelId,
        })
      }
      return response.data
    },
    {
      getNextPageParam: (lastPage) => lastPage.nextPage,
      onSuccess: (data) => {
        if (data.pages.length === 1 && data.pages[0].twins.length === 0) {
          analytics.trackNoSearchResults({
            term,
            modelId,
            fileType,
            siteNames,
            display,
            isCapabilityOfModelId,
          })
        }
      },
    }
  )

  const twins = twinsQuery.data?.pages
    .flatMap((page) => page.twins)
    ?.map((twin) => ({
      ...twin,
      siteName: sites.find((s) => s.id === twin.siteId)?.name,
    }))
    ?.filter((twin) => twin?.siteName != null)

  const queryId = twinsQuery.data?.pages?.[0]?.queryId

  const exportAll = useCallback(async () => {
    setSensorSearchEnabled(isModelASensor(modelId))
    analytics.trackTwinExportAll()
    await getCsv({
      fileType,
      modelId,
      queryId,
      sensorSearchEnabled,
      siteIds,
      scopeId,
      term,
      useCognitiveSearch,
    })
  }, [
    analytics,
    fileType,
    isModelASensor,
    modelId,
    queryId,
    sensorSearchEnabled,
    siteIds,
    scopeId,
    term,
    useCognitiveSearch,
  ])

  const exportSelected = useCallback(
    async (selectedTwins) => {
      setSensorSearchEnabled(isModelASensor(modelId))
      analytics.trackTwinExport({ count: selectedTwins.length })
      await getCsv({
        fileType,
        modelId,
        queryId,
        sensorSearchEnabled,
        siteIds,
        scopeId,
        term,
        twins: selectedTwins,
        useCognitiveSearch,
      })
    },
    [
      analytics,
      fileType,
      modelId,
      sensorSearchEnabled,
      queryId,
      siteIds,
      scopeId,
      term,
      useCognitiveSearch,
      isModelASensor,
    ]
  )

  return (
    <SearchResultsContext.Provider
      value={{
        t,

        queryKey,

        display,
        changeDisplay,
        tableDisplayIsDisabled,

        term,
        changeTerm,

        modelId,
        changeModelId,

        fileType,
        changeFileType,

        sites,
        siteIds,
        toggleSiteId,
        selectSiteId,
        updateSiteIds,

        scopeId,

        resetFilters,

        ontology: ontologyQuery.data,

        modelsOfInterestQuery,
        modelsOfInterest: modelsOfInterestQuery.data?.items,

        isLoading:
          ontologyQuery.isLoading ||
          twinsQuery.isLoading ||
          modelsOfInterestQuery.isLoading,
        isError:
          ontologyQuery.isError ||
          twinsQuery.isError ||
          modelsOfInterestQuery.isError,
        hasNextPage: twinsQuery.data?.pages.at(-1).nextPage != null,
        isLoadingNextPage: twinsQuery.isFetchingNextPage,
        fetchNextPage: () => twinsQuery.fetchNextPage(),

        twins,

        exportSelected,
        exportAll,

        filterInput,
        setFilterInput,

        isCapabilityOfInput,
        setIsCapabilityOfInput,

        isCapabilityOfModelId,
        onIsCapabilityOfModelIdChange,

        hasFileSearch,
        setHasFileSearch,

        hasSensorSearch: featureFlag.hasFeatureToggle(
          'twinExplorerSensorSearch'
        ),

        searchInput,
        setSearchInput,

        searchType,
        changeSearchType,

        setDisableCognitiveSearch,

        storedParams,

        searchParams,
        resetAllSearchParams,
      }}
    >
      {children}
    </SearchResultsContext.Provider>
  )
}
