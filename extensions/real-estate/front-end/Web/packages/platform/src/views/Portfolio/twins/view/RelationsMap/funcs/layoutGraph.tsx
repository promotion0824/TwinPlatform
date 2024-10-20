import ELK, { ElkExtendedEdge, ElkShape } from 'elkjs'
import { Edge as ReactFlowEdge } from 'react-flow-renderer'

import { formatRelationshipName } from '../../../shared'
import {
  APIEdge,
  DisplayGraph,
  DisplayNode,
  Graph,
  LayoutDirection,
} from '../types'
import { expandEdgeName } from './createDisplayGraph'
import { RELATIONS_MAP_NODE_SIZE } from './utils'

export function parseDirection(
  dir: string | undefined
): LayoutDirection | undefined {
  if (dir == null) {
    return undefined
  }
  const upper = dir.toUpperCase()
  if (Object.values<string>(LayoutDirection).includes(upper)) {
    return upper as LayoutDirection
  } else {
    return undefined
  }
}

type ElkTwinNode = DisplayNode & ElkShape

export type LaidOutNode = {
  id: string
  data: ElkTwinNode
  position: { x: number; y: number }
  type: 'twin'
}
type ElkTwinEdge = APIEdge & ElkExtendedEdge

export type LaidOutEdge = ReactFlowEdge<{
  animate?: 'forward' | 'backward'
  sourceNode: ElkTwinNode
  targetNode: ElkTwinNode
}>

function getEdgeProps(edge: ElkTwinEdge, sourceNode, targetNode): LaidOutEdge {
  let label: string | undefined
  let animate: 'forward' | 'backward' | undefined
  switch (edge.name) {
    case expandEdgeName:
      label = ''
      break

    case 'feeds':
      label = 'FEEDS'
      animate = 'forward'
      break
    case 'serves':
      label = 'SERVES'
      animate = 'forward'
      break

    // For isFedBy and servedBy, we want:
    // 1. the lines animating towards the twins being fed / served.
    // 2. the labels inverted to say "feeds" / "serves"
    // 3. the twins being fed / served to be below the feeders / servers on the graph, and
    //
    // We could have transformed the graph to reverse the source and target of
    // these relationships and flip the relationship names - this would satisfy
    // #1 and #2 but not #3.
    //
    // Instead we can satisfy all the criteria by leaving the relationships as they are
    // but rewriting the labels and styling the animation to animate in the
    // opposite direction.
    case 'isFedBy':
      label = 'FEEDS'
      animate = 'backward'
      break
    case 'servedBy':
      label = 'SERVES'
      animate = 'backward'
      break
    default:
      if (edge.name) {
        label = formatRelationshipName(edge.name).toUpperCase()
      } else {
        label = ''
      }
  }

  return {
    id: edge.id,
    source: edge.sourceId,
    target: edge.targetId,
    label,
    data: {
      animate,
      sourceNode,
      targetNode,
    },
    labelShowBg: true,
    labelStyle: {
      fill: 'black',
      fontFamily: 'Poppins',
      fontSize: 9,
      fontWeight: 500,
    },
    type: 'edge',
  }
}

function elkLayout(
  graph: DisplayGraph,
  direction: LayoutDirection = LayoutDirection.UP
) {
  const elk = new ELK({})

  const elkInput = {
    id: 'root',
    properties: { 'elk.direction': direction },
    children: graph.nodes.map((x) => ({
      ...x,
      width: RELATIONS_MAP_NODE_SIZE,
      height: 100,
    })),
    edges: graph.edges.map((x) => ({
      ...x,
      sources: [x.sourceId],
      targets: [x.targetId],
    })),
  }

  return elk.layout(elkInput, {
    layoutOptions: {
      aspectRatio: '1.77',
      algorithm: 'org.eclipse.elk.layered',

      // This is key to getting a layout that we want - it positions the
      // initial element in the center rather than in a corner.
      'nodePlacement.strategy': 'LINEAR_SEGMENTS',

      'org.eclipse.elk.force.temperature': '0.0001',
      'org.eclipse.elk.layered.priority.direction': '1',
      'elk.spacing.nodeNode': '25',
      'elk.layered.spacing.nodeNodeBetweenLayers': '35',
    },
    logging: false,
    measureExecutionTime: false,
  })
}

export type LaidOutGraph = Graph<LaidOutNode, LaidOutEdge>
export default async function layoutGraph(
  graph: DisplayGraph,
  direction: LayoutDirection
): Promise<LaidOutGraph> {
  const laidOutGraph = await elkLayout(graph, direction)

  if (laidOutGraph.children == null || laidOutGraph.edges == null) {
    throw new Error('children or edges was null')
  }

  const nodeLookup = Object.fromEntries(
    (laidOutGraph.children as ElkTwinNode[]).map((n) => [n.id, n])
  )

  const nodes = (laidOutGraph.children as ElkTwinNode[]).map((node) => {
    if (node.x == null || node.y == null) {
      throw new Error("Elk somehow didn't give us coordinates")
    }
    return {
      id: node.id,
      data: node,
      position: { x: node.x, y: node.y },
      sourcePosition: 'top',
      targetPosition: 'bottom',
      type: node.type,
    }
  }) as LaidOutNode[]

  return {
    nodes,
    edges: (laidOutGraph.edges as ElkTwinEdge[]).map((edge) =>
      getEdgeProps(edge, nodeLookup[edge.sourceId], nodeLookup[edge.targetId])
    ),
  }
}
