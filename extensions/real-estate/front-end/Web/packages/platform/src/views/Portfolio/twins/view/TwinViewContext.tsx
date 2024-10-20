import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useMemo,
} from 'react'
import { useQuery } from 'react-query'
import { useLocation } from 'react-router'

import { api } from '@willow/ui'

import { ProviderRequiredError } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { useGetTwin } from '@willow/common/twins/hooks/useGetTwin'
import { Ontology } from '@willow/common/twins/view/models'
import {
  getModelOfInterest,
  ModelOfInterest,
  useModelsOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import useOntology from '../../../../hooks/useOntologyInPlatform'
import { FilterType } from './AssetHistory/provider/AssetHistoryProvider'

// APIRelationship, APIRelationshipEnd are the types returned by the
// `/v2/twins/{twinId}/relationships` endpoint.
type APIRelationshipEnd = {
  id: string
  name: string
  siteId: string
  metadata: {
    modelId: string
  }
}

type APIRelationship = {
  id: string
  targetId: string
  sourceId: string
  name: string
  source: APIRelationshipEnd
  target: APIRelationshipEnd
  substance?: string
}

// ProcessedRelationship, ProcessedRelationshipEnd are roughly APIRelationship
// and APIRelationshipEnd but with models of interest attached.
type ProcessedRelationshipEnd = {
  id: string
  siteId: string
  modelId: string
  modelOfInterest?: ModelOfInterest
  name: string
}

type ProcessedRelationship = {
  id: string
  name: string
  source: ProcessedRelationshipEnd
  target: ProcessedRelationshipEnd
}

type File = { id: string; fileName?: string }

type ContextType = {
  tab: string | string[]
  setTab: (t) => void
  rightTab: string | string[] | undefined
  setRightTab: (t) => void
  type: FilterType
  setType: (type: FilterType) => void
  setInsightId: (insightId?: string) => void
  locateTwin: (twinId: string) => void
  relationships: ProcessedRelationship[] | undefined
  files?: File[]
  modelsOfInterest: ModelOfInterest[] | undefined
}

export const TwinViewContext = createContext<ContextType | undefined>(undefined)

export function useTwinView() {
  const context = useContext(TwinViewContext)
  if (context == null) {
    throw new ProviderRequiredError('TwinView')
  }
  return context
}

export function TwinViewProvider({
  siteId,
  twinId,
  children,
}: {
  siteId?: string
  twinId: string
  children: ReactNode
}) {
  const location = useLocation()
  const navigateOptions = useMemo(
    () => ({
      replace: true,
      state: location.state,
    }),
    [location.state]
  )

  // We have two sets of tabs on the twin view page - one on the left and one
  // on the right, each selected via a query parameter in the path.
  const [{ tab, rightTab, locatedTwin, type = 'all' }, setSearchParams] =
    useMultipleSearchParams(['tab', 'rightTab', 'locatedTwin', 'type'])

  const locateTwin = useCallback(
    (twinId: string) => {
      setSearchParams(
        {
          locatedTwin: twinId,
          rightTab: 'relationsMap',
          // The selectedTwin on Relations Map may be different from the locatedTwin.
          // If twinId is already set as locatedTwin, we attempt to refocus to
          // this twin so that it will be re-selected on Relations Map if it
          // was not already selected.
          refocus: locatedTwin === twinId ? 'true' : null,
        },
        navigateOptions
      )
    },
    [setSearchParams, navigateOptions]
  )

  const twinPath = siteId
    ? `/sites/${siteId}/twins/${twinId}`
    : `/v2/twins/${twinId}`
  const ontologyQuery = useOntology()
  const modelsOfInterestQuery = useModelsOfInterest()

  const twinQuery = useGetTwin({ siteId, twinId })

  const twinRelationshipsQuery = useQuery<APIRelationship[]>(
    ['twinRelationships', siteId, twinId],
    async () => {
      const response = await api.get(`${twinPath}/relationships`)
      return response.data
    }
  )

  const processedRelationships = useProcessedRelationships(
    {
      ontology: ontologyQuery.data,
      modelsOfInterest: modelsOfInterestQuery.data?.items,
      siteId,
      twinId,
    },
    twinRelationshipsQuery.data
  )

  const uniqueID = twinQuery.data?.twin?.uniqueID

  const filesQuery = useQuery<File[]>(
    ['twinFiles', uniqueID],
    async () => {
      const response = await api.get(
        `/sites/${twinQuery.data?.twin?.siteID}/assets/${uniqueID}/files`
      )
      return response.data
    },
    { enabled: uniqueID != null && twinQuery.data?.twin?.siteID != null }
  )

  const setTab = useCallback(
    (t) => setSearchParams({ tab: t }, navigateOptions),
    [setSearchParams, navigateOptions]
  )
  const setRightTab = useCallback(
    (t) => setSearchParams({ rightTab: t }, navigateOptions),
    [setSearchParams, navigateOptions]
  )

  const context = {
    tab: tab || 'summary',
    setTab,
    rightTab,
    setRightTab,
    locateTwin,
    type: type as FilterType,
    setType: (type: FilterType) => setSearchParams({ type }),
    setInsightId: (insightId) => setSearchParams({ insightId }),
    relationships: processedRelationships,
    files: filesQuery.data,
    modelsOfInterest: modelsOfInterestQuery.data?.items,
  }

  return (
    <TwinViewContext.Provider value={context}>
      {children}
    </TwinViewContext.Provider>
  )
}

/**
 * Combine relationships with models of interest.
 */
function useProcessedRelationships(
  {
    ontology,
    modelsOfInterest,
    siteId,
    twinId,
  }: {
    ontology?: Ontology
    modelsOfInterest?: ModelOfInterest[]
    siteId?: string
    twinId: string
  },
  relationships?: APIRelationship[]
) {
  function processEnd(end: APIRelationshipEnd): ProcessedRelationshipEnd {
    return {
      id: end.id,
      siteId: end.siteId,
      modelId: end.metadata.modelId,
      modelOfInterest: getModelOfInterest(
        end.metadata.modelId,
        ontology!,
        modelsOfInterest!
      ),
      name: end.name,
    }
  }

  return useQuery<ProcessedRelationship[]>(
    ['processedRelationships', siteId, twinId],
    () =>
      relationships!.map((relationship: APIRelationship) => ({
        id: relationship.id,
        name: relationship.name,
        target: processEnd(relationship.target),
        source: processEnd(relationship.source),
      })),
    {
      enabled:
        relationships != null && ontology != null && modelsOfInterest != null,
    }
  ).data
}
