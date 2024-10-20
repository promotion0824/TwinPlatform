import { render, RenderResult, screen, waitFor } from '@testing-library/react'
import { act } from 'react-test-renderer'
import userEvent from '@testing-library/user-event'
import { Site } from '@willow/common/site/site/types'
import {
  Ontology,
  getModelLookup,
  ModelsResponse,
} from '@willow/common/twins/view/models'
import { assertPathnameContains } from '@willow/common/utils/testUtils/LocationDisplay'
import Wrapper, { wrapperIsReady } from '@willow/ui/utils/testUtils/Wrapper'
import { supportDropdowns } from '@willow/ui/utils/testUtils/dropdown'

import { makeGraph } from '../funcs/utils'
import RelationsMapTabPanel from '../RelationsMapTabPanel'
import allModels from '../../../../../../mockServer/allModels'
import * as graphRoutes from '../../../../../../mockServer/graph'
import { setupTestServer } from '../../../../../../mockServer/testServer'
import { withoutRegion } from '../../../../../../mockServer/utils'
import { SitesProvider } from '../../../../../../providers'

supportDropdowns() // We don't actually use dropdowns but we do use ResizeObserver
const { server } = setupTestServer()

const basicGraph = makeGraph({
  nodes: ['initial', 'intermediate', 'outer'],
  edges: [
    ['initial', 'isFedBy', 'intermediate'],
    ['intermediate', 'isFedBy', 'outer'],
  ],
  models: {
    initial: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
    intermediate: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
    outer: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
  },
})

const graph2 = makeGraph({
  nodes: ['N1', 'N2', 'N2_in', 'N2_out'],
  edges: [
    ['N1', 'feeds', 'N2'],
    ['N2', 'feeds', 'N2_out'],
    ['N2_in', 'feeds', 'N2'],
  ],
  models: {
    N1: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
    N2: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
    N2_in: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
    N2_out: 'dtmi:com:willowinc:HVACEquipmentGroup;1',
  },
})

function useGraph(graph) {
  server.use(...graphRoutes.makeHandlers(graph).map(withoutRegion))
}

beforeAll(() => server.listen())
afterEach(() => {
  server.restoreHandlers()
})
afterAll(() => server.close())

async function setup(initialTwinId: string): Promise<{ result: RenderResult }> {
  const modelLookup = getModelLookup(allModels as ModelsResponse)

  const renderResult = render(
    <RelationsMapTabPanel
      initialTwin={{ id: initialTwinId, siteID: 'site123' }}
      modelsOfInterest={[]}
      ontology={new Ontology(modelLookup)}
    />,
    {
      wrapper: getWrapper(),
    }
  )

  await waitFor(() => expect(wrapperIsReady(screen)).toBeTrue())

  return {
    result: renderResult,
  }
}

describe('RelationsMap', () => {
  test('Expanding and contracting nodes via twin chips', async () => {
    useGraph(basicGraph)
    const {
      result: { container },
    } = await setup('initial')
    await expectTwinNamesToBe(container, ['initial', 'intermediate'])

    toggleExpandOut('intermediate')
    await expectTwinNamesToBe(container, ['initial', 'intermediate', 'outer'])

    toggleExpandOut('initial')
    await expectTwinNamesToBe(container, ['initial'])

    toggleExpandOut('initial')
    await expectTwinNamesToBe(container, ['initial', 'intermediate', 'outer'])
  })
})

describe('Overlay', () => {
  test('Clicking expand/contract when expanded should contract', async () => {
    useGraph(basicGraph)
    const {
      result: { container },
    } = await setup('initial')

    await expectTwinNamesToBe(container, ['initial', 'intermediate'])

    userEvent.click(screen.getByTestId('expand-contract-out'))
    await expectTwinNamesToBe(container, ['initial'])
  })

  test('Clicking expand/contract when contracted should expand', async () => {
    useGraph(basicGraph)
    const {
      result: { container },
    } = await setup('initial')

    await waitFor(() =>
      expect(getNodeNames(container)).toIncludeSameMembers([
        'initial',
        'intermediate',
      ])
    )

    selectTwin('intermediate')

    userEvent.click(screen.getByTestId('expand-contract-out'))

    await waitFor(() =>
      expect(getNodeNames(container)).toIncludeSameMembers([
        'initial',
        'intermediate',
        'outer',
      ])
    )
  })

  test('Expand / contract both', async () => {
    useGraph(graph2)
    const {
      result: { container },
    } = await setup('N1')

    await expectTwinNamesToBe(container, ['N1', 'N2'])

    selectTwin('N2')

    // Expand in and out, so we expect N2_in and N2_out to be added.
    userEvent.click(screen.getByTestId('expand-contract-both'))
    await expectTwinNamesToBe(container, ['N1', 'N2', 'N2_in', 'N2_out'])

    userEvent.click(screen.getByTestId('expand-contract-in'))
    await expectTwinNamesToBe(container, ['N1', 'N2', 'N2_out'])

    // When we have one direction expanded an the other direction not expanded,
    // the expand/contract both button should expand.
    userEvent.click(screen.getByTestId('expand-contract-both'))
    await expectTwinNamesToBe(container, ['N1', 'N2', 'N2_in', 'N2_out'])

    // When both directions are expanded, it should contract.
    userEvent.click(screen.getByTestId('expand-contract-both'))
    await expectTwinNamesToBe(container, ['N1', 'N2'])
  })

  test('Go to twin', async () => {
    useGraph(graph2)
    const {
      result: { container },
    } = await setup('N1')

    await expectTwinNamesToBe(container, ['N1', 'N2'])

    selectTwin('N2')
    userEvent.click(screen.getByText(/plainText.goToTwin/i))

    await waitFor(() => {
      assertPathnameContains('/portfolio/twins/view/site123/N2')
    })
  })

  test('Expanding and contracting model nodes', async () => {
    const modelId = 'dtmi:com:willowinc:HVACEquipmentGroup;1'
    server.use(
      ...graphRoutes
        .makeHandlers(
          makeGraph({
            nodes: ['N1', 'N2_1', 'N2_2', 'N3'],
            edges: [
              ['N1', 'isFedBy', 'N2_1'],
              ['N1', 'isFedBy', 'N2_2'],
              ['N2_2', 'isFedBy', 'N3'],
            ],
            models: {
              N1: modelId,
              N2_1: modelId,
              N2_2: modelId,
              N3: modelId,
            },
          })
        )
        .map(withoutRegion)
    )
    server.listen()

    const {
      result: { container },
    } = await setup('N1')
    await expectTwinNamesToBe(container, ['N1'])

    clickWithinReactFlow(
      container.querySelector(
        `[data-testid='ModelChipNode-${modelId}'] button`
      )!
    )

    await waitFor(() =>
      expect(getNodeNames(container)).toIncludeSameMembers([
        'N1',
        'N2_1',
        'N2_2',
      ])
    )

    clickWithinReactFlow(
      container.querySelector(
        `[data-testid='ModelChipNode-${modelId}'] button`
      )!
    )

    await waitFor(() =>
      expect(getNodeNames(container)).toIncludeSameMembers(['N1'])
    )
  })
})

function getWrapper() {
  return ({ children }) => (
    <Wrapper>
      <SitesProvider
        sites={
          [
            {
              id: 'site123',
            },
          ] as Site[]
        }
      >
        {children}
      </SitesProvider>
    </Wrapper>
  )
}

/**
 * There is a bug in d3-drag which caused the test to fails as it doesn't support
 * being run in non-browser environment - https://github.com/wbkd/react-flow/issues/2461
 * A workaround to this is to perform userEvent within a window or mocked window as
 * suggested in https://github.com/wbkd/react-flow/issues/2587#issuecomment-1406405472
 */
function clickWithinReactFlow(element: Element) {
  userEvent.click(element, { view: window })
}

function selectTwin(twinId) {
  act(() => {
    clickWithinReactFlow(
      document.body.querySelector(
        `[data-testid=TwinChipNode-${twinId}] [data-testid=chip]`
      )!
    )
  })
}

function toggleExpandOut(twinId) {
  act(() => {
    clickWithinReactFlow(
      document.body.querySelector(
        `[data-testid=TwinChipNode-${twinId}] [data-testid=expand-out-button]`
      )!
    )
  })
}

function getNodeNames(container) {
  return Array.from(
    container.querySelectorAll('[data-testid=chip] [title]')
  ).map((el: HTMLElement) => el.textContent)
}

async function expectTwinNamesToBe(container, names) {
  return waitFor(() =>
    expect(getNodeNames(container)).toIncludeSameMembers(names)
  )
}
