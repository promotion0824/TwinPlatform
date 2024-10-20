import type { Meta, StoryObj } from '@storybook/react'
import { Ontology, getModelLookup } from '@willow/common/twins/view/models'
import { useMemo } from 'react'
import { QueryClient, QueryClientProvider } from 'react-query'
import allModels from '../../../../../mockServer/allModels'
import RelationsMap from './RelationsMap'
import useRelationsMap from './hooks/useRelationsMap'

const queryClient = new QueryClient({})

const meta: Meta<typeof RelationsMap> = {
  component: RelationsMap,
}

export default meta
type Story = StoryObj<typeof RelationsMap>

export const Default: Story = {
  render: () => (
    <QueryClientProvider client={queryClient}>
      <Wrapper />
    </QueryClientProvider>
  ),
}

const initialTwin = {
  id: '123',
  siteID: '123',
}

function Wrapper() {
  const fetchGraph = useMemo(
    () => (_queryClient, twin) =>
      Promise.resolve({
        nodes: [
          {
            id: '123',
            name: 'initial',
            modelId: 'dtmi:com:willowinc:HVACEquipment;1',
            edgeCount: 0,
            edgeInCount: 0,
            edgeOutCount: 1,
          },
          {
            id: '124',
            name: 'intermediate',
            modelId: 'dtmi:com:willowinc:HVACEquipment;1',
            edgeCount: 0,
            edgeInCount: 1,
            edgeOutCount: 1,
          },
          {
            id: '125',
            name: 'outer',
            modelId: 'dtmi:com:willowinc:HVACEquipment;1',
            edgeCount: 0,
            edgeInCount: 1,
            edgeOutCount: 0,
          },
        ],
        edges: [
          {
            id: 'e1',
            sourceId: '123',
            targetId: '124',
            name: 'isFedBy',
          },
          {
            id: 'e2',
            sourceId: '124',
            targetId: '125',
            name: 'isFedBy',
          },
        ],
      }),
    []
  )

  const ontology = useMemo(() => new Ontology(getModelLookup(allModels)), [])

  const { graph, selectedTwinId, toggleModelExpansion, toggleTwinExpansion } =
    useRelationsMap(initialTwin, ontology, {
      injectedFetchGraph: fetchGraph,
    })

  return (
    <div style={{ width: 1000, height: 800 }}>
      <RelationsMap
        graph={graph}
        ontology={ontology}
        modelsOfInterest={[]}
        selectedTwinId={selectedTwinId}
        onTwinClick={() => {}}
        onToggleNodeExpansionClick={(twin, expandDirection) => {
          toggleTwinExpansion(twin.id, expandDirection)
        }}
        onModelClick={toggleModelExpansion}
      />
    </div>
  )
}
