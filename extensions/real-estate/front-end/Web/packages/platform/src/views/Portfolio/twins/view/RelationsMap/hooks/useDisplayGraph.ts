import _ from 'lodash'
import { useEffect, useState, useCallback, useMemo } from 'react'

import createDisplayGraph, {
  getEdgeGroups,
  getTwinLookup,
} from '../funcs/createDisplayGraph'
import {
  APIGraph,
  ExpandableDirection,
  GraphState,
  RelationshipDirection,
  RelationshipGroup,
  TwinWithIds,
} from '../types'

export default function useDisplayGraph(
  downloadGraph: {
    downloadedGraph?: APIGraph
    downloadAdditionalTwin: (twin: TwinWithIds) => Promise<void>
  },
  initialTwin: TwinWithIds,
  selectedTwinId: string
) {
  const [graphState, setGraphState] = useState<GraphState>({
    nodes: {
      [initialTwin.id]: {
        in: true,
        out: true,
      },
    },
    expandedGroups: [],
  })

  const graph = downloadGraph.downloadedGraph

  const selectedGraph = useMemo(
    () =>
      downloadGraph.downloadedGraph
        ? createDisplayGraph(
            downloadGraph.downloadedGraph,
            initialTwin.id,
            graphState,
            selectedTwinId
          )
        : undefined,
    [downloadGraph.downloadedGraph, graphState, initialTwin.id, selectedTwinId]
  )

  const toggleTwinExpansion = useCallback(
    async (twinId: string, direction: ExpandableDirection) => {
      await downloadGraph.downloadAdditionalTwin({
        id: twinId,
        siteID: initialTwin.siteID,
      })
      setGraphState((prevState) => {
        const newState = _.cloneDeep(prevState)
        if (newState.nodes[twinId] == null) {
          newState.nodes[twinId] = { in: false, out: false }
        }
        const nodeState = newState.nodes[twinId]

        if (direction === 'both') {
          // If we are currently expanded in one direction but not the other,
          // and we call `toggleTwinExpansion(twinId, 'both'), we don't want to
          // flip the two states, we want the two states to become the same.
          const currentVal = nodeState.out && nodeState.in
          nodeState.out = !currentVal
          nodeState.in = !currentVal
        } else if (direction === 'in') {
          nodeState.in = !nodeState.in
        } else if (direction === 'out') {
          nodeState.out = !nodeState.out
        }
        return newState
      })
    },
    [downloadGraph, initialTwin.siteID]
  )

  const isTwinExpanded = useCallback(
    (twinId: string, direction: ExpandableDirection) => {
      const nodeState = graphState.nodes[twinId]
      if (direction === 'in') {
        return nodeState?.in
      } else if (direction === 'out') {
        return nodeState?.out
      } else {
        return nodeState?.in && nodeState?.out
      }
    },
    [graphState.nodes]
  )

  const toggleModelExpansion = useCallback((group: RelationshipGroup) => {
    setGraphState((s) => ({
      ...s,
      expandedGroups: _.xorWith(s.expandedGroups, [group], _.isEqual),
    }))
  }, [])

  /**
   * Try to make the specified twin visible on the graph. Currently this is
   * best effort - we look to see if there is a group we can expand from the
   * initial twin which would make the specified twin visible, and if we find
   * one we expand it.
   *
   * Return true if we know that the located twin is visible, false otherwise.
   */
  const locateTwin = useCallback(
    (twinId: string) => {
      if (graph == null) {
        return false
      }

      const twinNode = graph.nodes.find((n) => n.id === twinId)
      const twinLookup = getTwinLookup(graph)

      if (twinNode != null && graphState.nodes[twinId] == null) {
        let newGroup: RelationshipGroup | undefined

        for (const { group, edges } of getEdgeGroups(
          graph,
          twinLookup,
          initialTwin.id,
          RelationshipDirection.out
        )) {
          if (edges.some((e) => e.targetId === twinId)) {
            newGroup = group
          }
        }
        for (const { group, edges } of getEdgeGroups(
          graph,
          twinLookup,
          initialTwin.id,
          RelationshipDirection.in
        )) {
          if (edges.some((e) => e.sourceId === twinId)) {
            newGroup = group
          }
        }

        if (
          newGroup != null &&
          !graphState.expandedGroups.find((g) => _.isEqual(g, newGroup))
        ) {
          setGraphState({
            ...graphState,
            expandedGroups: [...graphState.expandedGroups, newGroup],
          })
        }
        return newGroup != null
      } else {
        return false
      }
    },
    [graphState, graph, initialTwin.id]
  )

  useEffect(() => {
    downloadGraph.downloadAdditionalTwin(initialTwin)
  }, [downloadGraph, initialTwin])

  return {
    selectedGraph,
    graphState,
    isTwinExpanded,
    toggleTwinExpansion,
    toggleModelExpansion,
    locateTwin,
  }
}
