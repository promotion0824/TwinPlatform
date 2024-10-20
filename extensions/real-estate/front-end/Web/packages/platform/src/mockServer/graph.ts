/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'
import { makeGraph } from '../views/Portfolio/twins/view/RelationsMap/funcs/utils'

const sampleGraph = makeGraph({
  nodes: [
    '123123',
    'Air Handling Unit',
    'Fan Powered Box',
    'Fan Powered Box Reheat',
    'Exhaust Fan',
    'MAU.LR.01',
    'PB.4S1N',
    'Room 201',
    'Thing in room',
    'Level 02',
    'Building 121',
    'Thing X',
    'Thing Y',
    'Thing Z',
    'WIL-Retail-007-Case-1-7C',
    ...[...Array(20)].map((_, i) => `Thingy ${i}`),
  ],
  edges: [
    // [source ID, relationship name, target ID]
    ['123123', 'feeds', 'Air Handling Unit'],
    ['Air Handling Unit', 'feeds', 'Fan Powered Box'],
    ['Air Handling Unit', 'feeds', 'Fan Powered Box Reheat'],
    ['123123', 'feeds', 'Exhaust Fan'],
    ['123123', 'feeds', 'MAU.LR.01'],
    ['123123', 'serves', 'PB.4S1N'],
    ['123123', 'locatedIn', 'Room 201'],
    ['Thing in room', 'locatedIn', 'Room 201'],
    ['Room 201', 'partOf', 'Level 02'],
    ['Level 02', 'partOf', 'Building 121'],
    ['Room 201', 'xxx', 'Thing X'],
    ['Room 201', 'xxx', 'Thing Y'],
    ['Room 201', 'xxx', 'Thing Z'],
    ...([...Array(20)].map((_, i) => [
      'Exhaust Fan',
      'feeds',
      `Thingy ${i}`,
    ]) as Array<[string, string, string]>),
  ],
  models: {
    123123: 'dtmi:com:willowinc:Asset;1',
    'Air Handling Unit': 'dtmi:com:willowinc:Asset;1',
    'Fan Powered Box': 'dtmi:com:willowinc:Asset;1',
    'Fan Powered Box Reheat': 'dtmi:com:willowinc:Asset;1',
    'Exhaust Fan': 'dtmi:com:willowinc:Asset;1',
    'MAU.LR.01': 'dtmi:com:willowinc:Asset;1',
    'PB.4S1N': 'dtmi:com:willowinc:Asset;1',
    'Room 201': 'dtmi:com:willowinc:Room;1',
    'Thing in room': 'dtmi:com:willowinc:Asset;1',
    'Level 02': 'dtmi:com:willowinc:Level;1',
    'Building 121': 'dtmi:com:willowinc:Building;1',

    'Thing X': 'dtmi:com:willowinc:Building;1',
    'Thing Y': 'dtmi:com:willowinc:Building;1',
    'Thing Z': 'dtmi:com:willowinc:Building;1',
    'WIL-Retail-007-Case-1-7C':
      'dtmi:com:willowinc:MediumTemperatureRefrigeratedFoodDisplayCase;1',

    ...Object.fromEntries(
      [...Array(20)].map((_, i) => [
        `Thingy ${i}`,
        'dtmi:com:willowinc:Asset;1',
      ])
    ),
  },
  siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
})

/**
 * Return the nodes and edges in `graph` within `hops` hops of the node with id
 * `twinId`.
 */
export function getGraphFromTwin(graph, twinId, hops = 1) {
  if (graph.nodes[twinId] == null) {
    throw new Error(`Graph did not contain a node ${twinId}`)
  }
  const nodes = { [twinId]: graph.nodes[twinId] }
  const edges = new Set()

  for (let i = 0; i < hops; i++) {
    // Find all edges which start or end at one of the nodes we've found
    // already.
    const hopEdges = graph.edges.filter(
      ({ sourceId, targetId }) =>
        nodes[sourceId] != null || nodes[targetId] != null
    )
    for (const { sourceId, targetId } of hopEdges) {
      nodes[sourceId] = graph.nodes[sourceId]
      nodes[targetId] = graph.nodes[targetId]
    }
    for (const e of hopEdges) {
      edges.add(e)
    }
  }

  return {
    nodes: Object.values(nodes),
    edges: Array.from(edges),
  }
}

export function makeHandlers(graph) {
  const graph1 = {
    nodes: Object.fromEntries(graph.nodes.map((n) => [n.id, n])),
    edges: graph.edges,
  }

  return [
    // Note that the real server endpoint supports multiple twinIds but we
    // currently only support one.
    rest.get(
      '/:region/api/sites/:siteId/twins/:twinId/relatedTwins',
      (req, res, ctx) => {
        const { twinId } = req.params
        return res(ctx.json(getGraphFromTwin(graph1, twinId)))
      }
    ),
    rest.get('/:region/api/v2/twins/:twinId/relatedTwins', (req, res, ctx) => {
      const { twinId } = req.params
      return res(ctx.json(getGraphFromTwin(graph1, twinId)))
    }),
  ]
}

export const handlers = makeHandlers(sampleGraph)
