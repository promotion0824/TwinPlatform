import {
  createContext,
  useContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  useRef,
} from 'react'
import { useTranslation, TFunction } from 'react-i18next'
import { getModelDisplayName, Model } from '@willow/common/twins/view/models'
import {
  fileModelId,
  sensorModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import { useManageModelsOfInterest } from '../../../Provider/ManageModelsOfInterestProvider'
import useOntology from '../../../../../../hooks/useOntologyInPlatform'
import { ProviderRequiredError } from '@willow/common'

type SearchResultsContextType = {
  t?: TFunction
  modelId?: string
  changeModelId: (id: string) => void
  searchInput?: string
  setSearchInput: (searchInput: string) => void
  filteredModels: Model[]
}

const SearchResultsContext = createContext<
  SearchResultsContextType | undefined
>(undefined)

export function useSearchResults() {
  const context = useContext(SearchResultsContext)

  if (context == null) {
    throw new ProviderRequiredError('SearchResults')
  }

  return context
}

export default function SearchResults({
  selectedModelOfInterest,
  setSelectedModelOfInterest,
  formMode,
  children,
}) {
  const translation = useTranslation()
  const { t } = translation
  const [searchInput, setSearchInput] = useState<string | undefined>('')
  const [modelId, setModelId] = useState<string | undefined>()
  const { shouldRevertChangeRef } = useManageModelsOfInterest()
  const ontology = useOntology()

  const changeModelId = useCallback(
    (id: string) => {
      setModelId(id)
      const model = ontology?.data?.getModelById(id)

      const modelDisplayName =
        id && model !== undefined
          ? getModelDisplayName(model, translation)
          : undefined

      setSearchInput(modelDisplayName)

      setSelectedModelOfInterest((prev) => ({
        ...prev,
        modelId: id,
        name: modelDisplayName,
      }))
    },
    [ontology.data, setSelectedModelOfInterest, translation]
  )

  const initialStateRef = useRef(true)
  useEffect(() => {
    // When opening edit form for an existing MOI,
    // set search input field and categories to selected model.
    if (initialStateRef.current && formMode === 'edit' && ontology.data) {
      changeModelId(selectedModelOfInterest?.modelId)
      initialStateRef.current = false
    }

    // When "Revert Changes" button is clicked, set search input field and categories to original data.
    // shouldRevertChangeRef is used as a flag to prevent "Maximum update depth exceeded" warning.
    if (shouldRevertChangeRef.current) {
      changeModelId(selectedModelOfInterest?.modelId)
      shouldRevertChangeRef.current = false
    }
  }, [
    selectedModelOfInterest,
    ontology,
    formMode,
    shouldRevertChangeRef,
    changeModelId,
  ])

  // List of Willow models,
  // models with "rec_3_3" in id is removed.
  // and exclude models in ignoreModels and all its descendants.
  const ignoreModelsMap = useMemo(() => {
    const ignoreModels = [fileModelId, sensorModelId]
    // Get descendants of fileModelId and sensorModelId and add to ignoreModels
    ignoreModels.push(
      ...(ontology.data?.getModelDescendants(ignoreModels) || [])
    )
    return new Map(ignoreModels.map((model) => [model, true]))
  }, [ontology.data])

  const models = useMemo(
    () =>
      (ontology.data?.models || []).filter((model) => {
        const id = model['@id']
        return !ignoreModelsMap.has(id) && id.includes('willowinc')
      }),
    [ontology.data?.models, ignoreModelsMap]
  )

  // Filter the list of models of interest from ontology by search input.
  // Filtered models will be displayed in the search input dropdown.
  const filteredModels = useMemo(
    () =>
      models.filter((model) => {
        const displayName = getModelDisplayName(model, translation)
        if (displayName && searchInput) {
          return displayName.toLowerCase().includes(searchInput.toLowerCase())
        }
        return false
      }),
    [models, searchInput, translation]
  )

  return (
    <SearchResultsContext.Provider
      value={{
        t, // t is used in the Categories component.
        modelId,
        changeModelId,

        searchInput,
        setSearchInput,

        filteredModels,
      }}
    >
      {children}
    </SearchResultsContext.Provider>
  )
}
