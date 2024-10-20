import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'
import { DrilldownTree } from '.'
import { TreeDecorator } from '../../../storybookUtils'
import { Button } from '../../buttons/Button'

const meta: Meta<typeof DrilldownTree> = {
  title: 'DrilldownTree',
  component: DrilldownTree,
}

export default meta

type Story = StoryObj<typeof DrilldownTree>

const allItemsNode = {
  id: 'allCategories',
  name: 'All Categories',
}

const treeData = [
  {
    id: 'asset',
    name: 'Asset',
    children: [
      {
        id: 'architecturalAsset',
        name: 'Architectural Asset',
      },
      {
        id: 'distributionAsset',
        name: 'Distribution Asset',
      },
    ],
  },
  {
    id: 'buildingComponent',
    name: 'Building Component',
    children: [
      {
        id: 'architecturalBuildingComponent',
        name: 'Architectural Building Component',
        children: [
          {
            id: 'ceiling',
            name: 'Ceiling',
          },
          {
            id: 'facade',
            name: 'Facade',
          },
          {
            id: 'floor',
            name: 'Floor',
          },
          {
            id: 'wall',
            name: 'Wall',
          },
        ],
      },
      {
        id: 'structuralBuildingComponent',
        name: 'Structural Building Component',
      },
    ],
  },
  {
    id: 'collection',
    name: 'Collection',
  },
  {
    id: 'component',
    name: 'Component',
  },
  {
    id: 'space',
    name: 'Space',
  },
]

export const Playground: Story = {
  render: () => <DrilldownTree allItemsNode={allItemsNode} data={treeData} />,
  decorators: [TreeDecorator],
}

export const WithLabelAndDescription: Story = {
  render: () => (
    <DrilldownTree
      allItemsNode={allItemsNode}
      data={treeData}
      description="Select a Twin type"
      label="Twin Type"
    />
  ),
  decorators: [TreeDecorator],
}

export const Controlled: Story = {
  render: () => {
    const [selection, setSelection] = useState<string[]>()

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
        <Button onClick={() => setSelection(['buildingComponent'])}>
          Select "Building Component"
        </Button>
        <Button onClick={() => setSelection(['ceiling'])}>
          Select "Ceiling"
        </Button>
        <DrilldownTree
          allItemsNode={allItemsNode}
          data={treeData}
          onChange={() => setSelection(undefined)}
          selection={selection}
        />
      </div>
    )
  },
  decorators: [TreeDecorator],
}

export const Searchable: Story = {
  render: () => (
    <DrilldownTree
      allItemsNode={allItemsNode}
      data={treeData}
      label="Twin Type"
      searchable
    />
  ),
  decorators: [TreeDecorator],
}
