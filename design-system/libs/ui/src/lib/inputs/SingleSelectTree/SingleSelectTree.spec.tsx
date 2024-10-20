import {
  act,
  render,
  screen,
  userEvent,
  waitFor,
} from '../../../jest/testUtils'

import { SingleSelectTree } from '.'

const singleSelectTree = (onChangeIds: () => void) => (
  <SingleSelectTree
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

function toggleNode(name: string) {
  act(() =>
    screen.getByText(name).parentElement?.querySelector('button')?.click()
  )
}

describe('SingleSelectTree', () => {
  it('should deselect the previously selected node and select a new node when one is clicked', () => {
    const onChangeIds = jest.fn()
    render(singleSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('All Categories').click())
    expect(onChangeIds).lastCalledWith(['allCategories'])

    act(() => screen.getByText('Collection').click())
    expect(onChangeIds).lastCalledWith(['collection'])
  })

  it('should expand a parent, but not select it, if the toggle button is clicked and the parent was closed', () => {
    const onChangeIds = jest.fn()
    render(singleSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    expect(onChangeIds).not.toHaveBeenCalled()
    expect(screen.getByText('Architectural Asset')).toBeInTheDocument()
  })

  it('should close a parent, but not select it, if the toggle button is clicked and the parent was opened', () => {
    const onChangeIds = jest.fn()
    render(singleSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    expect(screen.getByText('Architectural Asset')).toBeInTheDocument()

    toggleNode('Asset')
    expect(screen.queryAllByText('Architectural Asset')).toHaveLength(0)
    expect(onChangeIds).not.toHaveBeenCalled()
  })

  it('should select the parent and leave it closed when a closed, unselected parent is clicked on', () => {
    const onChangeIds = jest.fn()
    render(singleSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith(['asset'])

    expect(screen.queryByText('Architectural Asset')).toBeNull()
    expect(screen.queryByText('Distribution Asset')).toBeNull()
  })

  it("should not deselect a selected node when it's clicked on", () => {
    const onChangeIds = jest.fn()
    render(singleSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith(['asset'])

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith(['asset'])
  })

  it('should only look at the name field when filtering items', async () => {
    render(
      <SingleSelectTree
        data={[
          {
            id: 'asset-1',
            name: 'Ceiling',
          },
          {
            id: 'asset-2',
            name: 'Facade',
          },
          {
            id: 'asset-3',
            name: 'Asset',
          },
        ]}
        searchable
      />
    )

    const searchInput = screen.getByTestId('tree-search-input')
    await userEvent.type(searchInput, 'asset')

    expect(screen.queryAllByRole('treeitem')).toHaveLength(1)
    expect(screen.getByText('Asset')).toBeInTheDocument()
  })

  it('should render a large amount of items at the same time', () => {
    render(
      <SingleSelectTree
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
