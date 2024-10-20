import { useEffect, useState } from 'react'

import layoutGraph, { LaidOutGraph } from '../funcs/layoutGraph'
import { DisplayGraph, LayoutDirection } from '../types'

/**
 * Takes a display graph `graph` and passes it through a layout engine to
 * assign coordinates to the nodes, producing a laid out graph.
 */
export default function useLayoutGraph(
  direction: LayoutDirection,
  graph?: DisplayGraph
) {
  const [isLoading, setIsLoading] = useState(false)
  const [laidOutGraph, setLaidOutGraph] = useState<LaidOutGraph>()

  useEffect(() => {
    ;(async () => {
      if (graph) {
        setIsLoading(true)
        setLaidOutGraph(await layoutGraph(graph, direction))
        setIsLoading(false)
      } else {
        setLaidOutGraph(undefined)
      }
    })()
  }, [direction, graph])

  return {
    isLayingOut: isLoading,
    laidOutGraph,
  }
}
