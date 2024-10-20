import { Number, Text } from '@willow/ui'
import styled, { css, keyframes } from 'styled-components'
import { useGraph } from '../index'

/**
 * This component will display the y label points for the graph.
 * The y label's values are based off the graphData, which is derived from telemetry's totalTelemetryCount,
 *  - an array of telemetry objects for the last 48 hour, where each object is grouped by 1 hour timestamps.
 * The y label point values range is from the max value of totalTelemetryCount in telemetry, to 0.
 * The y label points are equally spaced on the y axis.
 */
export default function YLabels() {
  const { maxValue } = useGraph()

  // Determine the number of y labels and positioning of y labels
  const size = 3 // number of y label points, the last y label point is 0 and is removed by the css styling
  const yOffset = 5
  const values = Array.from(Array(size)).map((_, i) => {
    const topPosition = (i / (size - 1)) * 100 - yOffset
    return {
      value: ((size - 1 - i) / (size - 1)) * maxValue, // y label value
      top: `${topPosition > 0 ? topPosition : 0}%`, // y labels is equally spaced out. The first point is < 0%, so we set it to 0%
      index: i, // index is used for key
    }
  })

  return (
    <div>
      {values.map((value) => (
        <StyledSpan key={value.index} style={{ top: value.top }}>
          <Text size="tiny" color="green" whiteSpace="nowrap">
            <Number value={value.value} format="," />
          </Text>
        </StyledSpan>
      ))}
    </div>
  )
}

const yEnter = keyframes`{
  0% {
    opacity: 0;
    transform: translate(-10px, 0);
  }
}`

const StyledSpan = styled.span`
  animation: ${css`
    ${yEnter} 0.2s ease
  `};
  left: -4px;
  pointer-events: none;
  position: absolute;
  transform: translate(3px, 0);

  // remove the y label point where the value is 0
  &:last-child {
    display: none;
  }
`
