import { renderHook, waitFor } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import { setupTestServer } from '../../../../../../../mockServer/testServer'
import { withoutRegion } from '../../../../../../../mockServer/utils'
import * as graphRoutes from '../../../../../../../mockServer/graph'
import { Ontology } from '@willow/common/twins/view/models'
import { makeGraph } from '../../funcs/utils'
import useRelationsMap from '../useRelationsMap'
import { LaidOutGraph } from '../../funcs/layoutGraph'
import { RelationshipDirection } from '../../types'

const { server } = setupTestServer()

server.use(
  ...graphRoutes
    .makeHandlers(
      makeGraph({
        nodes: ['initial', 'asset1', 'asset2'],
        edges: [
          ['initial', 'isFedBy', 'asset1'],
          ['initial', 'isFedBy', 'asset2'],
        ],
      })
    )
    .map(withoutRegion)
)

beforeAll(() => server.listen())
afterEach(() => {
  server.restoreHandlers()
})
afterAll(() => server.close())

describe('useRelationsMap', () => {
  test('Expanding model node', async () => {
    const { result } = renderHook(
      () =>
        useRelationsMap(
          { id: 'initial', siteID: '123' },
          new Ontology({
            defaultModel: {
              '@id': 'defaultModel',
              '@type': 'Interface',
              '@context': 'dtmi:dtdl:context;2',
              contents: [],
              extends: [],
            },
          })
        ),
      { wrapper: Wrapper }
    )

    await waitFor(() => {
      expect(result.current.graph).not.toBeNull()
      // On initial load we just expect the initial node and the contracted model
      // node.
      assertGraphMatches(result.current.graph!, {
        nodes: ['initial', 'sourceId#initial#isFedBy#defaultModel'],
        edges: [['initial', 'sourceId#initial#isFedBy#defaultModel']],
      })
    })

    result.current.toggleModelExpansion({
      twinId: 'initial',
      direction: RelationshipDirection.out,
      relationshipName: 'isFedBy',
      modelId: 'defaultModel',
    })

    await waitFor(() => expect(result.current.graph!.nodes).toHaveLength(4))

    // When we expand the model, we expect the model's two twins to be visible.
    // The model node should still be visible.
    assertGraphMatches(result.current.graph!, {
      nodes: [
        'initial',
        'sourceId#initial#isFedBy#defaultModel',
        'asset1',
        'asset2',
      ],
      edges: [
        ['initial', 'sourceId#initial#isFedBy#defaultModel'],
        ['sourceId#initial#isFedBy#defaultModel', 'asset1'],
        ['sourceId#initial#isFedBy#defaultModel', 'asset2'],
      ],
    })
  })
})

function assertGraphMatches(
  graph: LaidOutGraph,
  match: { nodes: string[]; edges: Array<[string, string]> }
) {
  expect(graph.nodes.map((n) => n.id)).toIncludeSameMembers(match.nodes)
  expect(graph.edges.map((n) => [n.source, n.target])).toIncludeSameMembers(
    match.edges
  )
}
