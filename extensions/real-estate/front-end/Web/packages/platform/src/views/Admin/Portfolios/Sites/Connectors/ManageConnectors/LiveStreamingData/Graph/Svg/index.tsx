import { useState } from 'react'
import styled, { keyframes, css } from 'styled-components'
import { useGraph } from '../index'
import Tooltip from '../Tooltip'
import { Column } from '../types'

// Svg will render svg element that will display columns in the graph
export default function Svg() {
  const { svgRef, columns } = useGraph()

  const [selected, setSelected] = useState<{ column: Column } | null>()

  return (
    <>
      <StyledSVG ref={svgRef}>
        {columns.map((column, i) => (
          <StyledG // Group both the dark green column and light green line at the top, together
            key={`${column.value}`}
            $columnIndex={i}
            $columnsLength={columns.length}
          >
            <rect // display dark green column
              x={column.x}
              y={column.y}
              width={column.width}
              height={column.height}
              fill="#29372a"
              onMouseEnter={() => {
                setSelected({ column })
              }}
              onMouseLeave={() => setSelected(null)}
            />

            <rect // display light green line that indicate the top of the column
              x={column.x}
              y={column.y + column.height - 2}
              width={column.width}
              height={1}
              fill="#35ea38"
            />
          </StyledG>
        ))}
        {
          // highlight column when hovered
          selected && (
            <SelectedRect
              x={selected.column.x}
              y={selected.column.y}
              width={selected.column.width}
              height={selected.column.height - 2}
            />
          )
        }
      </StyledSVG>
      {
        // display tooltip when hovered
        selected && <Tooltip selected={selected} />
      }
    </>
  )
}

const StyledSVG = styled.svg({
  border: 0,
  flex: 1,
  overflow: 'hidden',
  transform: ' scale(1, -1)',
})

const svgEnter = keyframes`
0% {
  transform: scale(1, 0);
}

100% {
  transform: scale(1);
}`

const StyledG = styled.g<{
  $columnIndex: number
  $columnsLength: number
}>`
  transform: scale(1, 0);
  animation: ${({ $columnIndex, $columnsLength }) => css`
    ${svgEnter} 0.2s ${$columnIndex * (0.2 / $columnsLength)}s ease forwards
  `};
`

const segmentEnter = keyframes`{
  0% {
    fill-opacity: 0;
  }
}`

const SelectedRect = styled.rect`
  animation: ${css`
    ${segmentEnter}
  `};
  fill: #eee;
  fill-opacity: 0.3;
  pointer-events: none;
`
