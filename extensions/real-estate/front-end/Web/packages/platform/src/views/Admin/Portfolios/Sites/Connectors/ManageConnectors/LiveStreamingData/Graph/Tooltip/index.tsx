import { useLayoutEffect, useRef, useState } from 'react'
import { Flex, Number, Portal, Text } from '@willow/ui'
import styled, { keyframes, css } from 'styled-components'
import { useGraph } from '../index'
import Name from './Name'
import { Column } from '../types'

/**
 * Tooltip is displayed whenever we hover on a column in the graph.
 * It will display timestamp and value of the hovered column.
 */
export default function Tooltip({
  selected,
}: {
  selected: { column: Column }
}) {
  const { svgRef, liveStreamingDataRef } = useGraph()

  const tooltipRef = useRef<HTMLDivElement>(null)
  const [style, setStyle] = useState<
    { left: number; top: number } | undefined
  >()

  useLayoutEffect(() => {
    if (svgRef.current != null && liveStreamingDataRef.current != null) {
      const graphRect = svgRef.current.getBoundingClientRect()
      const containerWidth = liveStreamingDataRef.current.offsetWidth
      const offset = containerWidth > 1100 ? containerWidth / 75 : 0
      const left =
        selected.column.left + selected.column.width / 2 + 320 - offset

      if (tooltipRef.current != null) {
        const top =
          graphRect.top +
          graphRect.height -
          selected.column.y -
          selected.column.height / 2 -
          tooltipRef.current.offsetHeight / 2

        setStyle({ left, top })
      }
    }
  }, [selected])

  return (
    <Portal>
      <Container
        data-testid="connectivity-graph-tooltip"
        ref={tooltipRef}
        style={{ top: style?.top || 0, left: style?.left || 0 }}
      >
        <Flex size="medium" padding="large">
          <Name timestamp={selected.column.timestamp} />
          <Text size="large" color="white">
            <Number value={selected.column.value} />
          </Text>
        </Flex>
      </Container>
    </Portal>
  )
}

const enterRight = keyframes`{
  0% {
    transform: translate(20px, 0);
  }
}`

const Container = styled.div(
  ({ theme }) => css`
    background-color: var(--tooltip);
    border: 1px solid ${theme.color.neutral.border.default};
    border-radius: var(--border-radius);
    box-shadow: var(--shadow);
    pointer-events: none;
    position: absolute;
    white-space: nowrap;
    z-index: var(--z-tooltip);
    margin-left: 8px;
    animation: ${enterRight} 0.2s ease;
  `
)
