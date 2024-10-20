import { act, render, screen, waitFor } from '../../../jest/testUtils'

import { DrilldownTree } from '.'

const drilldownTree = (onChangeIds: () => void) => (
  <DrilldownTree
    allItemsNode={{
      id: 'allCategories',
      name: 'All Categories',
    }}
    data={[
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
    ]}
    onChangeIds={onChangeIds}
  />
)

describe('DrilldownTree', () => {
  it('should deselect the previously selected node and select a new node when one is clicked', () => {
    const onChangeIds = jest.fn()
    render(drilldownTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Collection').click())
    expect(onChangeIds).lastCalledWith(['collection'])

    act(() => screen.getByText('All Categories').click())
    expect(onChangeIds).lastCalledWith(['allCategories'])
  })

  it("should not deselect a selected node when it's clicked on", () => {
    const onChangeIds = jest.fn()
    render(drilldownTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith(['asset'])

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith(['asset'])
  })

  it('should render a large amount of items at the same time', () => {
    render(
      <DrilldownTree
        allItemsNode={{
          id: 'allCategories',
          name: 'All Categories',
        }}
        data={new Array(100).fill(null).map((_, i) => ({
          id: `item-${i + 1}`,
          name: `Item ${i + 1}`,
        }))}
      />
    )

    waitFor(() => screen.getByText('All Categories'))
    expect(screen.getByText('Item 1')).toBeInTheDocument()
    expect(screen.getByText('Item 100')).toBeInTheDocument()
  })
})
