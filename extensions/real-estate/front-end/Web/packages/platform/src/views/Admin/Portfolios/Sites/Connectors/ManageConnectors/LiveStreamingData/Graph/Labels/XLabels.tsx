import { Text, Time } from '@willow/ui'
import styled, { css, keyframes } from 'styled-components'
import { useGraph } from '../index'

/**
 * This component will display the x label points for the graph.
 * The x label's values are based off the graphData, which is derived from telemetry's timestamps,
 *  - an array of telemetry objects for the last 48 hour, where each object is grouped by 1 hour timestamps.
 * The x label points are equally spaced on the x axis.
 * Most of the x label points are not displayed to prevent overlapping. This is done by css styling.
 */
export default function XLabels() {
  const { columns } = useGraph()

  return (
    <>
      {columns.map((column) => (
        <StyledSpan key={`${column.timestamp}`} $left={column.left}>
          <Text size="tiny" color="green" whiteSpace="nowrap">
            <Time value={column.timestamp} format="agoDetail" />
          </Text>
        </StyledSpan>
      ))}
    </>
  )
}

const xEnter = keyframes`{
  0% {
    opacity: 0;
    transform: translate(-50%, 10px);
  }
}`
const StyledSpan = styled.span<{ $left: number }>`
  animation: ${css`
    ${xEnter} 0.2s ease
  `};
  bottom: -5px;
  pointer-events: none;
  position: absolute;
  transform: translate(-50%, -3px);
  transform-origin: left;
  left: ${({ $left }) => `
    ${$left}px
  `};
  // Hide x labels points to prevent them overlapping each other on the x-axis
  // Only display 5 x label points that will fill the x-axis equally.
  &:not(:nth-child(10n-3)) {
    display: none;
  }
`
