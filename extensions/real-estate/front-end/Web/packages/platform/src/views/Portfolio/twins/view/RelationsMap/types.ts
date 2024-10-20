import {
  InsightStats,
  TicketStatsByStatus,
} from '@willow/common/site/site/types'

export type TwinWithIds = {
  siteID: string
  id: string
}

export type Graph<NodeType, EdgeType> = {
  nodes: NodeType[]
  edges: EdgeType[]
}

export enum LayoutDirection {
  UP = 'UP',
  RIGHT = 'RIGHT',
  DOWN = 'DOWN',
  LEFT = 'LEFT',
}

/**
 * Called "RelationshipDirection" so as not to be confused with {@link LayoutDirection}
 */
export enum RelationshipDirection {
  in = 'in',
  out = 'out',
}

/**
 * The GraphState determines what `createDisplayGraph` should do when it
 * encounters a node.
 *
 * `createDisplayGraph` starts at a single initial node and traverses outwards.
 * When a node is encountered, it may be one of a group of nodes of the same
 * model. If this group includes two or more nodes, and there is no
 * corresponding entry in `expandedGroups`, a 'model' node will be displayed
 * instead of all the nodes in that group. Conversely, if there is only one
 * node in the group, or the node state present, the node will be displayed.
 *
 * When a node is displayed, the NodeExpandState for the node in the `nodes`
 * lookup is used to determine whether to continue traversing. If `out` is
 * true, we follow edges starting at the current node. If `in` is true, we
 * follow edges ending at the current node. If both are true, we follow in both
 * directions.
 */
type NodeExpandState = {
  [RelationshipDirection.in]: boolean
  [RelationshipDirection.out]: boolean
}
export type NodeState = null | NodeExpandState
export type NodeStates = { [key: string]: NodeState }

export type ExpandableDirection = RelationshipDirection | 'both'

export type RelationshipGroup = {
  twinId: string
  direction: RelationshipDirection
  relationshipName: string
  modelId: string
}

export type GraphState = {
  nodes: { [twinId: string]: NodeExpandState }
  expandedGroups: Array<RelationshipGroup>
}

export type APINode = {
  id: string
  name: string
  modelId: string

  /**
   * Total number of edges in and out of the nodes
   */
  edgeCount: number

  /**
   * Number of edges that end at the node
   */
  edgeInCount: number

  /**
   * Number of edges that start at the node
   */
  edgeOutCount: number
}

type TwinCounts = {
  /** insights count by priority */
  insightsStats?: InsightStats
  /** tickets count by status */
  ticketStatsByStatus?: TicketStatsByStatus
}

export type APIEdge = {
  id: string
  sourceId: string
  targetId: string
  name: string
}

/**
 * The expected shape of a response to a twinGraph query on PortalXL.
 */
export type APIGraph = Graph<APINode, APIEdge>

/** The expected twinGraph on Single Tenant */
export type APIGraphWithTwinCounts = Graph<APINode & TwinCounts, APIEdge>

/** Response of 'statistics/twins' */
export type TwinStatisticsResponse = {
  twins: Array<{
    twinId: string
    insightsStats: InsightStats
    ticketStatsByStatus: TicketStatsByStatus
  }>
}

export type DisplayNode =
  | (APINode & {
      type: 'twin'
      displayedEdges: {
        in: APIEdge[]
        out: APIEdge[]
      }
      state: NodeExpandState
      selected: boolean
    } & TwinCounts)
  | {
      type: 'model'
      id: string
      twinId: string
      direction: RelationshipDirection
      modelId: string
      relationshipName: string
      count: number
    }

export type DisplayGraph = Graph<DisplayNode, APIEdge>
