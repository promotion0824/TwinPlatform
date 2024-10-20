import { dropTargetForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { useEffect, useRef } from 'react'

import { invariant } from '@willow/ui'
import { Stack, StackProps } from '@willowinc/ui'
import DraggableWrapper from './DraggableWrapper'
import { ComponentMap, DraggableItem } from './types'

interface DroppableColumnProps extends StackProps {
  items: DraggableItem[]
  columnId: string
  canDrop?: boolean
  componentMap: ComponentMap
  isEditingMode: boolean
}

const DroppableColumn = ({
  items,
  columnId,
  canDrop = true,
  componentMap,
  isEditingMode,
  ...rest
}: DroppableColumnProps) => {
  const droppableRef = useRef(null)

  useEffect(() => {
    const droppableElement = droppableRef.current
    invariant(droppableElement, 'droppableRef not exist')

    return dropTargetForElements({
      element: droppableElement,
      getData: () => ({ columnId }),
      canDrop: () => canDrop,
    })
  }, [canDrop, columnId])

  return (
    <Stack ref={droppableRef} {...rest}>
      {items.map((item: DraggableItem, index: number) => (
        <DraggableWrapper
          key={item.id}
          index={index}
          id={item.id}
          columnId={columnId}
          canDrag={canDrop}
          isEditingMode={isEditingMode}
          content={componentMap[item.id].component}
        />
      ))}
    </Stack>
  )
}

export default DroppableColumn
