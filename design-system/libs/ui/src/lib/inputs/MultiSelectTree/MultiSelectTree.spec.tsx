import {
  act,
  render,
  screen,
  userEvent,
  waitFor,
} from '../../../jest/testUtils'

import { MultiSelectTree } from '.'

const multiSelectTree = (onChangeIds: () => void) => (
  <MultiSelectTree
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

function getBadgeLabel(name: string) {
  return screen
    .getByText(name)
    .parentElement?.querySelector('.mantine-Badge-label')?.textContent
}

function toggleNode(name: string) {
  act(() =>
    screen.getByText(name).parentElement?.querySelector('button')?.click()
  )
}

describe('MultiSelectTree', () => {
  it('should deselect the "All Items" node and select an individual node when one is clicked', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('All Categories').click())
    expect(onChangeIds).lastCalledWith(['allCategories'])

    act(() => screen.getByText('Collection').click())
    expect(onChangeIds).lastCalledWith(['collection'])
  })

  it('should automatically select the "All Items" node if all other nodes become deselected', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Collection').click())
    expect(onChangeIds).lastCalledWith(['collection'])

    act(() => screen.getByText('Collection').click())
    expect(onChangeIds).lastCalledWith(['allCategories'])
  })

  it('should expand a parent, but not select it, if the toggle button is clicked and the parent was closed', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    expect(onChangeIds).not.toHaveBeenCalled()
    expect(screen.getByText('Architectural Asset')).toBeInTheDocument()
  })

  it('should close a parent, but not select it, if the toggle button is clicked and the parent was opened', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    expect(screen.getByText('Architectural Asset')).toBeInTheDocument()

    toggleNode('Asset')
    expect(screen.queryAllByText('Architectural Asset')).toHaveLength(0)
    expect(onChangeIds).not.toHaveBeenCalled()
  })

  it('should select all children and open the parent node when a closed, unselected parent is clicked on', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith([
      'asset',
      'architecturalAsset',
      'distributionAsset',
    ])

    expect(screen.getByText('Architectural Asset')).toBeInTheDocument()
    expect(screen.getByText('Distribution Asset')).toBeInTheDocument()
  })

  it("should select all children, including nested children, and open the parent node when a closed, unselected parent is clicked on, but shouldn't open the nested parent nodes", () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Building Component').click())
    expect(onChangeIds).lastCalledWith([
      'buildingComponent',
      'architecturalBuildingComponent',
      'ceiling',
      'facade',
      'floor',
      'wall',
      'structuralBuildingComponent',
    ])

    expect(
      screen.getByText('Architectural Building Component')
    ).toBeInTheDocument()
    expect(
      screen.getByText('Structural Building Component')
    ).toBeInTheDocument()
    expect(screen.queryAllByText('Ceiling')).toHaveLength(0)
  })

  it("should deselect a parent node if one of it's children is deselected", () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith([
      'asset',
      'architecturalAsset',
      'distributionAsset',
    ])

    act(() => screen.getByText('Architectural Asset').click())
    expect(onChangeIds).lastCalledWith(['distributionAsset'])
  })

  it("should deselect all parent nodes if one of it's children is deselected", () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Building Component').click())
    expect(onChangeIds).lastCalledWith([
      'buildingComponent',
      'architecturalBuildingComponent',
      'ceiling',
      'facade',
      'floor',
      'wall',
      'structuralBuildingComponent',
    ])

    toggleNode('Architectural Building Component')

    act(() => screen.getByText('Ceiling').click())
    expect(onChangeIds).lastCalledWith([
      'facade',
      'floor',
      'wall',
      'structuralBuildingComponent',
    ])
  })

  it('should select a parent node if all of its children have been selected', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    act(() => screen.getByText('Architectural Asset').click())
    act(() => screen.getByText('Distribution Asset').click())

    expect(onChangeIds).lastCalledWith([
      'architecturalAsset',
      'distributionAsset',
      'asset',
    ])
  })

  it('should select all parent nodes if all of their nested children have been selected', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Building Component')
    toggleNode('Architectural Building Component')
    act(() => screen.getByText('Ceiling').click())
    act(() => screen.getByText('Facade').click())
    act(() => screen.getByText('Floor').click())
    act(() => screen.getByText('Wall').click())
    act(() => screen.getByText('Structural Building Component').click())

    expect(onChangeIds).lastCalledWith([
      'ceiling',
      'facade',
      'floor',
      'wall',
      'architecturalBuildingComponent',
      'structuralBuildingComponent',
      'buildingComponent',
    ])
  })

  it('should select all child nodes if an unselected, open parent node is clicked on', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    act(() => screen.getByText('Asset').click())

    expect(onChangeIds).lastCalledWith([
      'asset',
      'architecturalAsset',
      'distributionAsset',
    ])
  })

  it('should deselect the node and all child nodes if a selected, open parent node is clicked on', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith([
      'asset',
      'architecturalAsset',
      'distributionAsset',
    ])

    act(() => screen.getByText('Asset').click())
    expect(onChangeIds).lastCalledWith(['allCategories'])
  })

  it('should select the node and all child nodes if an open parent node is clicked on with some selected children, while keeping the parent node open', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    act(() => screen.getByText('Architectural Asset').click())
    act(() => screen.getByText('Asset').click())

    expect(onChangeIds).lastCalledWith([
      'architecturalAsset',
      'asset',
      'distributionAsset',
    ])

    expect(screen.getByText('Architectural Asset')).toBeInTheDocument()
    expect(screen.getByText('Distribution Asset')).toBeInTheDocument()
  })

  it('should show a count of selected child nodes on a parent node when it is collapsed', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    act(() => screen.getByText('Architectural Asset').click())

    toggleNode('Asset')
    expect(getBadgeLabel('Asset')).toBe('1')
  })

  it('should show not show the child count badge on an expanded parent node', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    act(() => screen.getByText('Architectural Asset').click())
    expect(getBadgeLabel('Asset')).toBe(undefined)
  })

  it('should show not show the child count badge on a parent with all children selected', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Asset')
    act(() => screen.getByText('Architectural Asset').click())
    act(() => screen.getByText('Distribution Asset').click())

    toggleNode('Asset')
    expect(getBadgeLabel('Asset')).toBe(undefined)
  })

  it('should include nested parents and children in the child count', () => {
    const onChangeIds = jest.fn()
    render(multiSelectTree(onChangeIds))
    waitFor(() => screen.getByText('All Categories'))

    toggleNode('Building Component')
    act(() => screen.getByText('Architectural Building Component').click())
    toggleNode('Building Component')

    expect(getBadgeLabel('Building Component')).toBe('5')
  })

  it('should only look at the name field when filtering items', async () => {
    render(
      <MultiSelectTree
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
      <MultiSelectTree
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
