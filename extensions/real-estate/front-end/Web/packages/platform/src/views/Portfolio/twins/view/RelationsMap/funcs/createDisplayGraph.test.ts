/* eslint-disable @typescript-eslint/no-non-null-assertion */
import 'jest-extended'
import {
  APIEdge,
  DisplayGraph,
  DisplayNode,
  GraphState,
  RelationshipDirection,
} from '../types'
import createDisplayGraph from './createDisplayGraph'
import { makeGraph, unmakeGraph } from './utils'

describe('createDisplayGraph', () => {
  test('one node', () => {
    const graph = makeGraph({
      nodes: ['N1'],
      edges: [],
    })
    const state: GraphState = {
      nodes: {},
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [{ id: 'N1', type: 'twin' }],
      edges: [],
    })
  })

  test('two nodes, one edge', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2'],
      edges: [['N1', 'feeds', 'N2']],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N2', type: 'twin' },
      ],
      edges: [['N1', 'feeds', 'N2']],
    })
  })

  test('two nodes, one edge, backwards', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2'],
      edges: [['N2', 'feeds', 'N1']],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N2', type: 'twin' },
      ],
      edges: [['N2', 'feeds', 'N1']],
    })
  })

  test('two edges to same model', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2', 'N3'],
      edges: [
        ['N1', 'feeds', 'N2'],
        ['N1', 'feeds', 'N3'],
      ],
      models: {
        N2: 'myModel',
        N3: 'myModel',
      },
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'sourceId#N1#feeds#myModel', type: 'model' },
      ],
      edges: [['N1', 'feeds', 'sourceId#N1#feeds#myModel']],
    })
  })

  test('two edges to same model, backwards', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2', 'N3'],
      edges: [
        ['N2', 'feeds', 'N1'],
        ['N3', 'feeds', 'N1'],
      ],
      models: {
        N2: 'myModel',
        N3: 'myModel',
      },
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'targetId#N1#feeds#myModel', type: 'model' },
      ],
      edges: [['targetId#N1#feeds#myModel', 'feeds', 'N1']],
    })
  })

  test('two edges to different models', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2', 'N3'],
      edges: [
        ['N1', 'feeds', 'N2'],
        ['N1', 'feeds', 'N3'],
      ],
      models: {
        N2: 'myModel1',
        N3: 'myModel2',
      },
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N2', type: 'twin' },
        { id: 'N3', type: 'twin' },
      ],
      edges: [
        ['N1', 'feeds', 'N2'],
        ['N1', 'feeds', 'N3'],
      ],
    })
  })

  test('two edges to different models, backwards', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2', 'N3'],
      edges: [
        ['N2', 'feeds', 'N1'],
        ['N3', 'feeds', 'N1'],
      ],
      models: {
        N2: 'myModel1',
        N3: 'myModel2',
      },
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N2', type: 'twin' },
        { id: 'N3', type: 'twin' },
      ],
      edges: [
        ['N2', 'feeds', 'N1'],
        ['N3', 'feeds', 'N1'],
      ],
    })
  })

  test('expanded in but not out', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N_out', 'N_in'],
      edges: [
        ['N1', 'feeds', 'N_out'],
        ['N_in', 'feeds', 'N1'],
      ],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: false },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N_in', type: 'twin' },
      ],
      edges: [['N_in', 'feeds', 'N1']],
    })
  })

  test('expanded out but not in', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N_out', 'N_in'],
      edges: [
        ['N1', 'feeds', 'N_out'],
        ['N_in', 'feeds', 'N1'],
      ],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: false, out: true },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N_out', type: 'twin' },
      ],
      edges: [['N1', 'feeds', 'N_out']],
    })
  })

  test('no model nodes in unexpanded direction', () => {
    // Regression test: previously we could display model nodes in
    // the out direction even if we were only supposed to traverse in the in
    // direction, and vice versa.
    //
    // Raw graph:
    // ┌──┐┌───────┐
    // │N1││N2_in_1│
    // └┬─┘└┬──────┘
    // ┌▽───▽──────┐
    // │N2         │
    // └┬─────────┬┘
    // ┌▽───────┐┌▽───────┐
    // │N2_out_1││N2_out_2│
    // └────────┘└────────┘
    //
    // Display graph:
    // ┌─┐┌───────┐
    // │N││N2_in_1│
    // └┬┘└┬──────┘
    // ┌▽──▽┐
    // │N2  │
    // └────┘
    const graph = makeGraph({
      nodes: ['N1', 'N2', 'N2_out_1', 'N2_out_2', 'N2_in_1'],
      edges: [
        ['N1', 'feeds', 'N2'],
        ['N2', 'feeds', 'N2_out_1'],
        ['N2', 'feeds', 'N2_out_2'],
        ['N2_in_1', 'feeds', 'N2'],
      ],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
        N2: { in: true, out: false },
      },
      expandedGroups: [],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'N2', type: 'twin' },
        { id: 'N2_in_1', type: 'twin' },
      ],
      edges: [
        ['N1', 'feeds', 'N2'],
        ['N2_in_1', 'feeds', 'N2'],
      ],
    })
  })

  test('Display all edges even if node has already been seen', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N2_1', 'N2_2', 'N3'],
      edges: [
        ['N1', 'feeds', 'N2_1'],
        ['N1', 'feeds', 'N2_2'],
        ['N2_2', 'feeds', 'N3'],

        // These two edges target nodes which have already been added to the
        // display graph before we encounter them. So even though we don't want to
        // double-display the nodes, we still need to make sure to display the edges.
        ['N3', 'feeds', 'N2_1'],
        ['N3', 'feeds', 'N2_2'],
      ],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: true },
        N2_1: { in: false, out: true },
        N2_2: { in: false, out: true },
        N3: { in: false, out: true },
      },
      expandedGroups: [
        {
          twinId: 'N1',
          direction: RelationshipDirection.out,
          relationshipName: 'feeds',
          modelId: 'defaultModel',
        },
      ],
    }

    expectMatchingGraph(createDisplayGraph(graph, 'N1', state), {
      nodes: [
        { id: 'N1', type: 'twin' },
        { id: 'sourceId#N1#feeds#defaultModel', type: 'model' },
        { id: 'N2_1', type: 'twin' },
        { id: 'N2_2', type: 'twin' },
        { id: 'N3', type: 'twin' },
      ],
      edges: [
        ['N1', 'feeds', 'sourceId#N1#feeds#defaultModel'],
        ['sourceId#N1#feeds#defaultModel', 'feeds', 'N2_1'],
        ['sourceId#N1#feeds#defaultModel', 'feeds', 'N2_2'],
        ['N2_2', 'feeds', 'N3'],
        ['N3', 'feeds', 'N2_1'],
        ['N3', 'feeds', 'N2_2'],
      ],
    })
  })

  test('displayedEdges property in returned nodes', () => {
    const graph = makeGraph({
      nodes: ['N1', 'N_out', 'N_in'],
      edges: [
        ['N1', 'feeds', 'N_out'],
        ['N_in', 'feeds', 'N1'],
      ],
    })
    const state: GraphState = {
      nodes: {
        N1: { in: true, out: false },
      },
      expandedGroups: [],
    }

    const displayGraph = createDisplayGraph(graph, 'N1', state)

    expectNodeDisplayedEdges(displayGraph, 'N_in', {
      in: [],
      out: [
        {
          sourceId: 'N_in',
          name: 'feeds',
          targetId: 'N1',
        },
      ],
    })
  })
})

function expectMatchingGraph(
  graph: DisplayGraph,
  expectedGraph: {
    nodes: Array<Partial<DisplayNode>>
    edges: Array<[string, string, string]>
  }
) {
  const unmadeGraph = unmakeGraph(graph)
  expect(unmadeGraph.nodes).toIncludeAllPartialMembers(expectedGraph.nodes)
  expect(unmadeGraph.edges).toIncludeSameMembers(expectedGraph.edges)
}

/**
 * Assert that the `displayedEdges` attribute for the node with id `twinId`
 * match `expectedEdges`. All of the expected edges must exist, but only the
 * properties of the edges in `expectedEdges` will be checked.
 */
function expectNodeDisplayedEdges(
  graph: DisplayGraph,
  nodeId: string,
  expectedEdges: { in: Array<Partial<APIEdge>>; out: Array<Partial<APIEdge>> }
) {
  const node = graph.nodes.find((n) => n.id === nodeId)!
  expect(node.type).toEqual('twin')
  if (node.type !== 'twin') {
    throw new Error("This will never happen, but Typescript doesn't know that")
  }
  expect(node.displayedEdges.in).toIncludeAllPartialMembers(expectedEdges.in)
  expect(node.displayedEdges.out).toIncludeAllPartialMembers(expectedEdges.out)
}
