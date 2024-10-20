import { Layout } from './types'
import { getBreakpoint, mergeColumnsBasedOnHeight, moveItem } from './utils'

describe('mergeColumnsBasedOnHeight', () => {
  const prefixId = 'newColumn'

  it('should merge 2 items based on height', () => {
    const layout: Layout = [
      {
        id: 'column1',
        items: [{ id: 'item2' }],
      },
      {
        id: 'column2',
        items: [{ id: 'item1' }],
      },
    ]

    const itemsHeight = {
      item1: 100,
      item2: 50,
    }

    const expectedOutput: Layout = [
      {
        id: 'newColumn-0',
        items: [{ id: 'item2' }, { id: 'item1' }],
      },
    ]

    const result = mergeColumnsBasedOnHeight(layout, itemsHeight, prefixId)
    expect(result).toEqual(expectedOutput)
  })

  it('should merge columns correctly based on item heights', () => {
    const layout: Layout = [
      {
        id: 'column1',
        items: [{ id: 'item1' }, { id: 'item2' }],
      },
      {
        id: 'column2',
        items: [{ id: 'item3' }, { id: 'item4' }],
      },
    ]

    const itemsHeight = {
      item1: 50,
      item2: 60,
      item3: 40,
      item4: 70,
    }

    const expectedOutput: Layout = [
      {
        id: 'newColumn-0',
        items: [
          { id: 'item1' },
          { id: 'item3' },
          { id: 'item4' },
          { id: 'item2' },
        ],
      },
    ]

    const result = mergeColumnsBasedOnHeight(layout, itemsHeight, prefixId)
    expect(result).toEqual(expectedOutput)
  })

  it('should handle an empty layout', () => {
    const layout: Layout = []

    const itemsHeight = {}

    const expectedOutput: Layout = [
      {
        id: 'newColumn-0',
        items: [],
      },
    ]

    const result = mergeColumnsBasedOnHeight(layout, itemsHeight, prefixId)
    expect(result).toEqual(expectedOutput)
  })

  it('should correctly merge columns when all items have the same height', () => {
    const layout: Layout = [
      {
        id: 'column1',
        items: [{ id: 'item1' }, { id: 'item3' }],
      },
      {
        id: 'column2',
        items: [{ id: 'item2' }, { id: 'item4' }],
      },
    ]

    const itemsHeight = {
      item1: 50,
      item2: 50,
      item3: 50,
      item4: 50,
    }

    const expectedOutput: Layout = [
      {
        id: 'newColumn-0',
        items: [
          { id: 'item1' },
          { id: 'item2' },
          { id: 'item3' },
          { id: 'item4' },
        ],
      },
    ]

    const result = mergeColumnsBasedOnHeight(layout, itemsHeight, prefixId)
    expect(result).toEqual(expectedOutput)
  })

  it('should return the same layout if all items are already in the same column', () => {
    const layout: Layout = [
      {
        id: 'column1',
        items: [{ id: 'item1' }, { id: 'item2' }],
      },
    ]

    const itemsHeight = {
      item1: 50,
      item2: 60,
    }

    const expectedOutput: Layout = [
      {
        id: 'newColumn-0',
        items: [{ id: 'item1' }, { id: 'item2' }],
      },
    ]

    const result = mergeColumnsBasedOnHeight(layout, itemsHeight, prefixId)
    expect(result).toEqual(expectedOutput)
  })
})

describe('moveItem', () => {
  const columns = [
    { id: 'col-1', items: [{ id: 'item-1' }, { id: 'item-2' }] },
    { id: 'col-2', items: [{ id: 'item-3' }] },
  ]

  test('moves item to the top of another column', () => {
    const result = moveItem(columns, 'item-1', 'col-2', 'item-3', 'top')
    expect(result[1].items.map((item) => item.id)).toEqual(['item-1', 'item-3'])
  })

  test('moves item to the bottom of another column', () => {
    const result = moveItem(columns, 'item-1', 'col-2', 'item-3', 'bottom')
    expect(result[1].items.map((item) => item.id)).toEqual(['item-3', 'item-1'])
  })

  test('does nothing if item does not exist', () => {
    const result = moveItem(columns, 'item-999', 'col-2', 'item-3', 'top')
    expect(result).toEqual(columns)
  })

  test('does nothing if target column does not exist', () => {
    const result = moveItem(columns, 'item-1', 'col-999', 'item-3', 'top')
    expect(result).toEqual(columns)
  })

  test('does not mutate the original columns array', () => {
    const original = columns
    moveItem(columns, 'item-1', 'col-2', 'item-3', 'top')
    expect(columns).toEqual(original)
  })

  test('can drop to an empty colum', () => {
    const result = moveItem(
      [
        { id: 'col-1', items: [{ id: 'item-1' }, { id: 'item-2' }] },
        { id: 'col-2', items: [] },
      ],
      'item-1',
      'col-2',
      undefined,
      null
    )
    expect(result).toEqual([
      { id: 'col-1', items: [{ id: 'item-2' }] },
      { id: 'col-2', items: [{ id: 'item-1' }] },
    ])
  })

  test('can drop to the empty bottom area of colum', () => {
    const result = moveItem(
      [
        { id: 'col-1', items: [{ id: 'item-1' }, { id: 'item-2' }] },
        { id: 'col-2', items: [{ id: 'item-3' }] },
      ],
      'item-1',
      'col-2',
      undefined,
      null
    )
    expect(result).toEqual([
      { id: 'col-1', items: [{ id: 'item-2' }] },
      { id: 'col-2', items: [{ id: 'item-3' }, { id: 'item-1' }] },
    ])
  })
})

describe('getBreakpoint', () => {
  const BREAKPOINTS = [600, 1200, 1800, 900]

  const testCases = [
    { width: 500, expected: 600 },
    { width: 600, expected: 600 },
    { width: 700, expected: 900 },
    { width: 900, expected: 900 },
    { width: 1100, expected: 1200 },
    { width: 1200, expected: 1200 },
    { width: 1600, expected: 1800 },
    { width: 1800, expected: 1800 },
    { width: 1900, expected: 1800 },
  ]

  testCases.forEach(({ width, expected }) => {
    test(`returns correct breakpoint for width ${width}`, () => {
      expect(getBreakpoint(width, BREAKPOINTS)).toBe(expected)
    })
  })
})
