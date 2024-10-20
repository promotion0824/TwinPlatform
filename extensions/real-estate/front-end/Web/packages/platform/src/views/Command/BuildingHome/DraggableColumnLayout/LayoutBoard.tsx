import { extractClosestEdge } from '@atlaskit/pragmatic-drag-and-drop-hitbox/closest-edge'
import { monitorForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { keys, mapValues } from 'lodash'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import styled from 'styled-components'

import { invariant, useResizeObserver } from '@willow/ui'
import DroppableColumn from './DroppableColumn'
import { Cols, ComponentMap, DraggableItem, Layout } from './types'
import {
  arrayToLayoutColumns,
  getBreakpoint,
  layoutColumnsToArray,
  mergeColumnsBasedOnHeight,
  moveItem,
} from './utils'

export interface DraggableLayoutBoardProps {
  /**
   * Pair of breakpoint number and column number.
   *
   * Currently only allows 2 column numbers in total,
   * a single column and a multiple columns (doesn't matter how many).
   * @example
   * {  768: 1, 1024: 3 , 1280: 3}
   */
  cols: Cols
  data: DraggableItem[][]
  setData: (data: DraggableItem[][]) => void
  prefixId?: string
  isEditingMode?: boolean
  /** component id to component and default height map */
  componentMap: ComponentMap
}

const COLUMN_GAP_SIZE = 8 // can use theme tokens later

/**
 * Currently only allows maximum of 2 set of column configurations in total:
 * a single column and a multiple columns (doesn't matter how many)
 */
const DraggableLayoutBoard = ({
  cols,
  data,
  setData,
  isEditingMode = false,
  prefixId = 'home-page-board',
  componentMap,
}: DraggableLayoutBoardProps) => {
  const windowRef = useRef(document.body)
  const [colNum, setColNum] = useState<number | undefined>()

  const itemsHeight = useMemo(
    () => mapValues(componentMap, 'defaultHeight'),
    [componentMap]
  )

  const handleResize = useCallback(
    (entries: ResizeObserverEntry[]) => {
      const { width } = entries[0].contentRect
      const breakpoint = getBreakpoint(width, keys(cols).map(Number))
      const newColNum = cols[breakpoint]

      if (newColNum === colNum) {
        return
      }

      setColNum(newColNum)
    },
    [colNum, cols]
  )
  useResizeObserver(windowRef.current, handleResize)

  // update layout when number of column changes
  const layout: Layout = useMemo(
    () =>
      colNum === 1
        ? // calculate the new order by considering the Y position
          // of the top border for each item
          mergeColumnsBasedOnHeight(
            arrayToLayoutColumns(data, prefixId),
            itemsHeight,
            prefixId,
            COLUMN_GAP_SIZE
          )
        : arrayToLayoutColumns(data, prefixId),
    [colNum, data, itemsHeight, prefixId]
  )

  useEffect(
    () =>
      monitorForElements({
        onDrop({ source, location }) {
          const destination = location.current.dropTargets[0]
          if (!destination) {
            return
          }
          const destinationColumnId = destination.data.columnId
          const destinationItemId = destination.data.id
          const itemId = source.data.id
          const edge = extractClosestEdge(location.current.dropTargets[0].data)

          invariant(typeof itemId === 'string', 'itemId must be a string')
          invariant(
            typeof destinationColumnId === 'string',
            'columnId must be a string'
          )
          invariant(
            typeof destinationItemId === 'string' ||
              destinationItemId === undefined,
            'itemId must be a string or undefined'
          )
          invariant(
            edge === 'top' || edge === 'bottom' || edge === null,
            'edge must be top, bottom or null'
          )

          invariant(layout, 'layout should be valid when dragging')

          const newLayout = moveItem(
            layout,
            itemId,
            destinationColumnId,
            destinationItemId,
            edge
          )

          setData(layoutColumnsToArray(newLayout))
        },
      }),
    [layout, setData]
  )

  return (
    <BoardContainer $columnNum={colNum ?? 0}>
      {layout?.map((column) => (
        <DroppableColumn
          items={column.items}
          columnId={column.id}
          key={column.id}
          canDrop={
            isEditingMode &&
            // no drag and drop for single column layout because it's hard to decide draggable item
            // orders if start drag and drop from single column, then switch back to multiple columns
            Boolean(colNum && colNum > 1)
          }
          isEditingMode={isEditingMode}
          css={{
            flex: 1,
          }}
          componentMap={componentMap}
          gap={`s${COLUMN_GAP_SIZE}`}
        />
      ))}
    </BoardContainer>
  )
}

const BoardContainer = styled.div<{ $columnNum: number }>(
  ({ theme, $columnNum }) => `
  display: grid;
  width: 100%;
  height: 100%;
  grid-template-columns: repeat(${$columnNum}, minmax(0, 1fr));
  gap: ${theme.spacing.s8};
`
)

export default DraggableLayoutBoard
