import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { SingleSelectTree } from '.'
import { TreeDecorator } from '../../../storybookUtils'
import { Button } from '../../buttons/Button'
import { Stack } from '../../layout/Stack'

const meta: Meta<typeof SingleSelectTree> = {
  title: 'SingleSelectTree',
  component: SingleSelectTree,
}

export default meta

type Story = StoryObj<typeof SingleSelectTree>

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
  render: () => (
    <SingleSelectTree allItemsNode={allItemsNode} data={treeData} />
  ),
  decorators: [TreeDecorator],
}

export const WithLabelAndDescription: Story = {
  render: () => (
    <SingleSelectTree
      allItemsNode={allItemsNode}
      data={treeData}
      description="Select a Twin type"
      label="Twin Type"
    />
  ),
  decorators: [TreeDecorator],
}

export const HorizontalLayout: Story = {
  render: () => (
    <SingleSelectTree
      allItemsNode={allItemsNode}
      data={treeData}
      description="Select a Twin type"
      label="Twin Type"
      layout="horizontal"
    />
  ),
}

export const HorizontalLayoutWithLabelWidth: Story = {
  render: () => (
    <SingleSelectTree
      allItemsNode={allItemsNode}
      data={treeData}
      description="Select a Twin type"
      label="Twin Type"
      labelWidth={300}
      layout="horizontal"
    />
  ),
}

export const Controlled: Story = {
  render: () => {
    const [selection, setSelection] = useState<string[]>()

    return (
      <Stack w={260}>
        <Button onClick={() => setSelection(['ceiling'])}>
          Select "Ceiling"
        </Button>
        <SingleSelectTree
          allItemsNode={allItemsNode}
          data={treeData}
          onChange={() => setSelection(undefined)}
          selection={selection}
        />
      </Stack>
    )
  },
}

export const Searchable: Story = {
  render: () => (
    <SingleSelectTree
      allItemsNode={allItemsNode}
      data={treeData}
      label="Twin Type"
      searchable
    />
  ),
  decorators: [TreeDecorator],
}
