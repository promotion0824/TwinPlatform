import { cloneDeep, find, map, omit, remove, sortBy } from 'lodash'
import { Column, DraggableItem, Layout } from './types'

export function getBreakpoint(width: number, breakpoints: number[]): number {
  const sortedBreakpoints = breakpoints.sort((a, b) => a - b)

  return (
    find(sortedBreakpoints, (value) => width <= value) ||
    // If no matching breakpoint is found and the width is larger than the largest breakpoint,
    // return the category with the largest value
    sortedBreakpoints[sortedBreakpoints.length - 1]
  )
}

/** update layout columns when drop an item */
export function moveItem(
  columns: Column[],
  sourceItemId: string,
  targetColumnId: string,
  targetItemId: string | undefined, // if not provided, it will add to the end of the empty target column
  edgeOfTargetItem: 'top' | 'bottom' | null // if not provided, it will add to the end of the empty target column
) {
  let sourceColumnIndex = -1
  let targetColumnIndex = -1
  let targetItemIndex = -1
  let itemToMove: DraggableItem | undefined

  // Find the source and target indices and item to move
  columns.forEach((column, index) => {
    if (column.id === targetColumnId) {
      targetColumnIndex = index
      if (targetItemId) {
        targetItemIndex = column.items.findIndex(
          (item) => item.id === targetItemId
        )
      } else {
        targetItemIndex = column.items.length
      }
    }

    const findItemToMove = column.items.find((item) => item.id === sourceItemId)
    if (findItemToMove) {
      sourceColumnIndex = index
      itemToMove = findItemToMove
    }
  })

  // If item or column wasn't found, do nothing
  if (sourceColumnIndex === -1 || targetColumnIndex === -1 || !itemToMove) {
    return columns
  }

  const clonedColumns = cloneDeep(columns)

  // Remove the item from its original place
  remove(clonedColumns[sourceColumnIndex].items, {
    id: sourceItemId,
  })

  // Insert the item in the new position
  clonedColumns[targetColumnIndex].items.splice(
    edgeOfTargetItem === 'top' ? targetItemIndex : targetItemIndex + 1,
    0,
    itemToMove
  )

  return clonedColumns
}

export function layoutColumnsToArray(columns: Column[]): DraggableItem[][] {
  return map(columns, 'items')
}

export function arrayToLayoutColumns(
  arrays: DraggableItem[][],
  prefixId: string
): Column[] {
  return arrays.map((items, index) => ({
    id: `${prefixId}-${index}`,
    items,
  }))
}

/**
 * merge columns into a single column based on 2 rules:
 * 1. based on the Y position order of the items in each column
 * 2. from left to right column if they have the same Y position
 */
export const mergeColumnsBasedOnHeight = (
  layout: Layout,
  itemsHeight: Record<string, number>,
  prefixId: string,
  gapSize = 8
): Column[] => {
  const itemsWithOffsetTop = layout.flatMap((column) => {
    let offsetTop = 0
    return column.items.map((item) => {
      const itemWithOffsetPosition = {
        ...item,
        offsetPosition: offsetTop,
      }
      offsetTop += itemsHeight[item.id] + gapSize

      return itemWithOffsetPosition
    })
  })

  const sortedItemsByOffset: DraggableItem[] = sortBy(
    itemsWithOffsetTop,
    'offsetPosition'
  ).map((item) => omit(item, 'offsetPosition'))

  return arrayToLayoutColumns([sortedItemsByOffset], prefixId)
}
