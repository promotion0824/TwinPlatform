import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Ontology } from '@willow/common/twins/view/models'

import useSingleSearchParam from '../../../../../../hooks/useSingleSearchParam'

import useDownloadGraph, { fetchGraph } from './useDownloadGraph'
import useDisplayGraph from './useDisplayGraph'
import useLayoutGraph from './useLayoutGraph'
import {
  LaidOutEdge,
  LaidOutGraph,
  LaidOutNode,
  parseDirection,
} from '../funcs/layoutGraph'
import {
  ExpandableDirection,
  GraphState,
  LayoutDirection,
  RelationshipGroup,
  TwinWithIds,
} from '../types'

/**
 * The useRelationsMap hook manages the user's interactions with the
 * relations map.
 * The behaviour is described here:
 * https://willow.atlassian.net/wiki/spaces/MAR/pages/2122383527/Graph+expansion
 */
export default function useRelationsMap(
  initialTwin: TwinWithIds,
  ontology: Ontology,
  isSingleTenant = true,
  {
    injectedFetchGraph = fetchGraph,
  }: {
    injectedFetchGraph?: typeof fetchGraph
  } = {}
): {
  isLoading: boolean
  graph: {
    nodes: LaidOutNode[]
    edges: LaidOutEdge[]
  } | null
  graphState: GraphState
  direction: LayoutDirection
  selectedTwinId: string
  setSelectedTwinId: (id: string) => void
  toggleTwinExpansion: (
    twinId: string,
    expandDirection: ExpandableDirection
  ) => void
  toggleModelExpansion: (group: RelationshipGroup) => void
  /**
   * Is the twin overlay visible? By default there is an overlay with
   * information and operations about the selected twin. The user can manually
   * close this overlay. Next time they click a twin (the same one or a
   * different one), the overlay will open again.
   */
  isTwinOverlayVisible: boolean
  isTwinExpanded: (
    twinId: string,
    expandDirection: ExpandableDirection
  ) => boolean
  closeTwinOverlay: () => void
} {
  const [locatedTwin] = useSingleSearchParam('locatedTwin')
  const [refocus, setRefocus] = useSingleSearchParam('refocus')
  // This is used to keep track of currently selected twin.
  // On initial load, if the locatedTwin is present, we use the locatedTwin; otherwise,
  // the initial twin will be used. When user selects another twin on the relations map,
  // we will update this selection. Additionally, when user locates another twin,
  // the updated locatedTwin will be the new selected twin.
  const [selectedTwinId, _setSelectedTwinId] = useState<string>(
    (locatedTwin as string) || initialTwin.id
  )
  const lastLocatedTwinId = useRef<string | undefined>(undefined)
  const [isTwinOverlayVisible, setTwinOverlayVisible] = useState(true)

  const setSelectedTwinId = useCallback((twinId) => {
    _setSelectedTwinId(twinId)
    setTwinOverlayVisible(true)
  }, [])

  // This is an experimental hidden feature - add "?direction=right" (or left,
  // or down if you really want) to the URL to render the graph in that
  // direction.
  const [directionParam] = useSingleSearchParam('direction')
  const direction =
    parseDirection(directionParam as string) ?? LayoutDirection.UP

  const downloadGraph = useDownloadGraph(
    ontology,
    { injectedFetchGraph },
    isSingleTenant
  )
  const {
    selectedGraph,
    graphState,
    toggleTwinExpansion,
    isTwinExpanded,
    toggleModelExpansion,
    locateTwin,
  } = useDisplayGraph(downloadGraph, initialTwin, selectedTwinId)

  const { isLayingOut, laidOutGraph } = useLayoutGraph(direction, selectedGraph)

  // Apply the highlighting as a final stage so we can change the highlight
  // without having to redo the layout.
  const graphWithSelection = useMemo(() => {
    if (laidOutGraph != null) {
      return applySelection(laidOutGraph, selectedTwinId)
    } else {
      return null
    }
  }, [laidOutGraph, selectedTwinId])

  useEffect(() => {
    // ensure that the selected twin is displayed on the graph if it exists in the data
    if (lastLocatedTwinId.current !== selectedTwinId) {
      locateTwin(selectedTwinId)
      lastLocatedTwinId.current = selectedTwinId
    }
  }, [lastLocatedTwinId, locateTwin, selectedTwinId])

  useEffect(() => {
    if (locatedTwin != null) {
      setSelectedTwinId(locatedTwin)
    }
  }, [locatedTwin, setSelectedTwinId])

  useEffect(() => {
    if (refocus != null) {
      // Select the located twin so locateTwin will be triggered if the locatedTwin was
      // different from selectedTwin. Also reset the refocus param.
      setSelectedTwinId(locatedTwin)
      setRefocus(null)
    }
  }, [refocus, locatedTwin, setRefocus, setSelectedTwinId])

  const closeTwinOverlay = useCallback(() => setTwinOverlayVisible(false), [])

  return {
    isLoading: downloadGraph.isDownloading || isLayingOut,
    graph: graphWithSelection,
    graphState,
    direction,
    selectedTwinId,
    setSelectedTwinId,
    isTwinOverlayVisible,
    closeTwinOverlay,
    toggleTwinExpansion,
    isTwinExpanded,
    toggleModelExpansion,
  }
}

/**
 * Create a new graph where the node with id `selectedTwinId` is marked as
 * selected.
 */
function applySelection(graph: LaidOutGraph, selectedTwinId: string) {
  return {
    nodes: graph.nodes.map((n) => {
      if (n.data.type === 'twin' && n.data.id === selectedTwinId) {
        return { ...n, data: { ...n.data, selected: true } }
      } else {
        return n
      }
    }),
    edges: graph.edges,
  }
}
