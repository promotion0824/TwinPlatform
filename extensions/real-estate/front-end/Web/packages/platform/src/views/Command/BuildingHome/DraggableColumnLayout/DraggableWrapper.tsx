import {
  Edge,
  attachClosestEdge,
  extractClosestEdge,
} from '@atlaskit/pragmatic-drag-and-drop-hitbox/closest-edge'
import { DropIndicator } from '@atlaskit/pragmatic-drag-and-drop-react-drop-indicator/box'
import { combine } from '@atlaskit/pragmatic-drag-and-drop/combine'
import {
  draggable,
  dropTargetForElements,
} from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { useEffect, useRef, useState } from 'react'
import styled, { css } from 'styled-components'

import { invariant } from '@willow/ui'
import { DraggableContent } from './types'

interface DraggableItemProps {
  id: string
  index: number
  columnId: string
  canDrag?: boolean
  content: DraggableContent
  isEditingMode: boolean
}

const DraggableWrapper = ({
  id,
  index,
  columnId,
  canDrag = true,
  content: ContentComponent,
  isEditingMode,
}: DraggableItemProps) => {
  const draggableHandleRef = useRef<HTMLButtonElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)

  const [dragging, setDragging] = useState<boolean>(false)
  const [closestEdge, setClosestEdge] = useState<Edge | null>(null)

  useEffect(() => {
    const containerElement = containerRef.current
    const handleElement = draggableHandleRef.current

    if (
      // might not exist when not in editing mode,
      // depends on the component implementation
      !handleElement
    ) {
      return undefined
    }

    invariant(containerElement, 'containerRef not exist')
    const data = { id, columnId }

    return combine(
      draggable({
        element: containerElement,
        dragHandle: handleElement,
        onDragStart: () => setDragging(true),
        onDrop: () => setDragging(false),
        getInitialData: () => data,
        canDrag: () => canDrag,
      }),
      dropTargetForElements({
        element: containerElement,
        getData({ input }) {
          return attachClosestEdge(data, {
            element: containerElement,
            input,
            allowedEdges: ['top', 'bottom'],
          })
        },
        onDragEnter: ({ self, source }) => {
          if (source.element !== containerElement) {
            setClosestEdge(extractClosestEdge(self.data))
          }
        },
        onDrag({ self, source }) {
          const isSource = source.element === containerElement
          if (isSource) {
            setClosestEdge(null)
            return
          }

          setClosestEdge(extractClosestEdge(self.data))
        },
        onDragLeave: () => {
          setClosestEdge(null)
        },
        onDrop: () => {
          setClosestEdge(null)
        },
      })
    )
  }, [canDrag, columnId, id])

  if (!ContentComponent) {
    // In case no matching component can be found for id
    return null
  }

  return (
    <Wrapper $dragging={dragging}>
      <ContentComponent
        key={id}
        canDrag={canDrag}
        isEditingMode={isEditingMode}
        draggableRef={draggableHandleRef}
        ref={containerRef}
        id={id}
      />

      {closestEdge && (
        <DropIndicator
          edge={closestEdge}
          gap={index === 0 && closestEdge === 'top' ? undefined : '8px'}
        />
      )}
    </Wrapper>
  )
}

const Wrapper = styled.div<{
  $dragging: boolean
}>(
  ({ theme, $dragging }) => css`
    position: relative; /* for positioning DropIndicator */
    // DropIndicator do not support css prop, style nor className
    --ds-border-selected: ${theme.color.intent.primary.bg.bold.default};

    > * {
      ${$dragging && 'opacity: 0.4;'}
    }
  `
)

export default DraggableWrapper
