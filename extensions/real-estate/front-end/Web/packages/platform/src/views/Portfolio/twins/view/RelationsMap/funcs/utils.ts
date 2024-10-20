import _ from 'lodash'
import { DisplayGraph } from '../types'

/**
 * Allows creation of a graphs in a more succinct way.
 */
export function makeGraph({
  nodes,
  edges,
  models,
  siteId = '123',
}: {
  nodes?: string[]
  edges: Array<[sourceId: string, relationshipName: string, targetId: string]>
  models?: { [key: string]: string }
  siteId?: string
}) {
  const graph = {
    nodes: (
      nodes ??
      _.uniq(edges.flatMap(([sourceId, , targetId]) => [sourceId, targetId]))
    ).map((id) => ({
      id,
      name: id,
      siteId,
      modelId: models?.[id] ?? 'defaultModel',
      edgeCount: 0,
      edgeInCount: 0,
      edgeOutCount: 0,
    })),
    edges: edges.map(([sourceId, name, targetId], index) => ({
      id: `edge-${index}`,
      sourceId,
      name,
      targetId,
    })),
  }

  for (const { sourceId, targetId } of graph.edges) {
    const sourceNode = graph.nodes.find((n) => n.id === sourceId)
    if (sourceNode == null) {
      throw new Error(`Node with id ${sourceId} not found`)
    }

    const targetNode = graph.nodes.find((n) => n.id === targetId)
    if (targetNode == null) {
      throw new Error(`Node with id ${targetId} not found`)
    }

    targetNode.edgeInCount += 1
    sourceNode.edgeOutCount += 1
  }

  for (const node of graph.nodes) {
    node.edgeCount = node.edgeInCount + node.edgeOutCount
  }

  return graph
}

/**
 * Inverse of `makeGraph` - takes a real graph and converts the edges back to
 * the succinct form to make it more convenient to write assertions against.
 * Used in tests and in the mock server.
 */
export function unmakeGraph(graph: DisplayGraph) {
  return {
    ...graph,
    edges: graph.edges.map((e) => [e.sourceId, e.name, e.targetId]),
  }
}

/** The fixed width for each node displays in the relations map  */
export const RELATIONS_MAP_NODE_SIZE = 250
