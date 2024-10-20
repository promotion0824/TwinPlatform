import { api } from '@willow/ui'
import { AxiosResponse } from 'axios'
import _, { clone, find } from 'lodash'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { QueryClient, useQueryClient } from 'react-query'

import { capabilityModelId } from '../../../shared'

// Types
import { Ontology } from '@willow/common/twins/view/models'
import {
  APIGraph,
  APIGraphWithTwinCounts,
  TwinStatisticsResponse,
  TwinWithIds,
} from '../types'

/**
 * Given two graphs, create a new graph with the unions of the nodes and edges
 * in the two existing graphs.
 */
function combineGraphs(g1: APIGraph, g2: APIGraph): APIGraph {
  return {
    nodes: _.uniqBy([...g1.nodes, ...g2.nodes], (n) => n.id),
    edges: _.uniqBy([...g1.edges, ...g2.edges], (e) => e.id),
  }
}

/**
 * Remove all nodes that extend the Capability model, and remove all edges that
 * have a source or target that was removed. Can easily be generalised to
 * remove an arbitrary model.
 */
function removeCapabilities(graph: APIGraph, ontology: Ontology): APIGraph {
  const nodeIdsToRemove = new Set<string>()
  const edgeIdsToRemove = new Set<string>()
  for (const n of graph.nodes) {
    if (ontology.getModelAncestors(n.modelId).includes(capabilityModelId)) {
      nodeIdsToRemove.add(n.id)
    }
  }
  for (const e of graph.edges) {
    if (nodeIdsToRemove.has(e.sourceId) || nodeIdsToRemove.has(e.targetId)) {
      edgeIdsToRemove.add(e.id)
    }
  }
  return {
    nodes: graph.nodes.filter((n) => !nodeIdsToRemove.has(n.id)),
    edges: graph.edges.filter((n) => !edgeIdsToRemove.has(n.id)),
  }
}

/**
 * Multi-tenant API calls to query about relatedTwins still requires a siteId
 * in the URL.
 */
export async function fetchGraph(
  queryClient: QueryClient,
  twin: TwinWithIds,
  isSingleTenant = true
): Promise<APIGraph | APIGraphWithTwinCounts> {
  return queryClient.fetchQuery<APIGraph | APIGraphWithTwinCounts>(
    ['twin-graph', twin.siteID, twin.id],
    async () => {
      const response = (await api.get(
        isSingleTenant
          ? `/v2/twins/${twin.id}/relatedTwins`
          : `/sites/${twin.siteID}/twins/${twin.id}/relatedTwins`
      )) as AxiosResponse<APIGraph>
      // Be defensive about the backend not returning an edges list for nodes
      // that have no relationships:
      // https://dev.azure.com/willowdev/Unified/_workitems/edit/63192
      if (response.data.edges == null) {
        response.data.edges = []
      }

      if (isSingleTenant) {
        try {
          const twinNodesData = clone(response.data) as APIGraphWithTwinCounts

          const counts = (await api.post(`/statistics/twins`, {
            TwinIds: twinNodesData.nodes.map((n) => n.id),
          })) as AxiosResponse<TwinStatisticsResponse>

          for (const node of twinNodesData.nodes) {
            node.insightsStats = find(counts.data?.twins, {
              twinId: node.id,
            })?.insightsStats || {
              openCount: 0,
              urgentCount: 0,
              highCount: 0,
              mediumCount: 0,
              lowCount: 0,
            }

            node.ticketStatsByStatus = find(counts.data?.twins, {
              twinId: node.id,
            })?.ticketStatsByStatus || {
              closedCount: 0,
              openCount: 0,
              resolvedCount: 0,
            }
          }

          return twinNodesData
        } catch (e) {
          console.error(`Failed to get /statistics/twins. Error: ${e}`)
        }
      }

      return response.data
    }
  )
}

export default function useDownloadGraph(
  ontology: Ontology,
  {
    injectedFetchGraph = fetchGraph,
  }: {
    injectedFetchGraph?: typeof fetchGraph
  },
  isSingleTenant = true
) {
  const [isLoading, setIsLoading] = useState(false)
  const [state, setState] = useState<{
    downloadedGraph?: APIGraph
    downloadedTwins: TwinWithIds[]
  }>({
    downloadedTwins: [],
  })
  const queryClient = useQueryClient()

  // We imperatively make API requests in this component (instead of using eg.
  // useQuery), and so we need to manually make sure we don't make requests
  // after unmount. In particular this can cause tests to try to make requests
  // after the mock server has been torn down and create failures that are very
  // difficult to diagnose.
  const isMountedRef = useRef(true)
  useEffect(
    () => () => {
      isMountedRef.current = false
    },
    []
  )

  const downloadAdditionalTwin = useCallback(
    async (twin: TwinWithIds) => {
      if (
        isMountedRef.current &&
        !state.downloadedTwins.find((t) => _.isEqual(t, twin))
      ) {
        setIsLoading(true)
        const graph = await injectedFetchGraph(
          queryClient,
          twin,
          isSingleTenant
        )
        const processedGraph = removeCapabilities(graph, ontology)
        setState((prevState) => ({
          downloadedGraph: prevState.downloadedGraph
            ? combineGraphs(prevState.downloadedGraph, processedGraph)
            : processedGraph,
          downloadedTwins: [...prevState.downloadedTwins, twin],
        }))
        setIsLoading(false)
      }
    },
    [queryClient, ontology, state, injectedFetchGraph, isSingleTenant]
  )

  return useMemo(
    () => ({
      isDownloading: isLoading,
      downloadedGraph: state.downloadedGraph,
      downloadAdditionalTwin,
    }),
    [isLoading, state.downloadedGraph, downloadAdditionalTwin]
  )
}
