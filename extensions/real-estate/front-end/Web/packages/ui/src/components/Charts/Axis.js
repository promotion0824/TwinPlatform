import tw, { styled } from 'twin.macro'
import { v4 as uuidv4 } from 'uuid'
import { useRef, useEffect } from 'react'

export default function Axis({
  axisGroupProperties,
  lineProperties,
  textProperties,
  data,
}) {
  const axisGroupRef = useRef()

  const maxTextWidth = 123.69

  useEffect(() => {
    const textElements = axisGroupRef.current?.querySelectorAll('text') || []

    textElements.forEach((textElement) => {
      const textLength = textElement.getComputedTextLength()

      if (textLength > maxTextWidth) {
        for (let i = textElement.innerHTML.length; ; i--) {
          if (textElement.getSubStringLength(0, i) < maxTextWidth) {
            // eslint-disable-next-line no-param-reassign
            textElement.innerHTML = textElement.innerHTML.slice(0, i)
            break
          }
        }
        // eslint-disable-next-line no-param-reassign
        textElement.innerHTML += textLength > maxTextWidth ? '...' : ''
      }
    })
  }, [data])

  return (
    <AxisGroup
      ref={axisGroupRef}
      textAnchor={axisGroupProperties.textAnchor}
      transform={`translate(${axisGroupProperties.xTranslate}, ${axisGroupProperties.yTranslate})`}
    >
      {data.map((d) => (
        <GridGroup
          key={uuidv4()}
          transform={`translate(${d.xTranslate}, ${d.yTranslate})`}
        >
          <Line
            x1={lineProperties.x1}
            y1={lineProperties.y1}
            x2={lineProperties.x2}
            y2={lineProperties.y2}
            stroke={lineProperties.stroke}
          />
          <StyledText
            fill={textProperties.fill}
            x={textProperties.x}
            y={textProperties.y}
            dy={textProperties.dy}
            fontSize={textProperties.fontSize}
          >
            {d.tickValue}
          </StyledText>
        </GridGroup>
      ))}
    </AxisGroup>
  )
}

const AxisGroup = tw.g`
`

const GridGroup = tw.g`
`

const Line = tw.line`
  opacity-20
  text-gray-350
`

const fontSizes = {
  0.8: tw`text-xxs2`, // 9px / 13px
  1: tw`text-sm1`, // 10px
  2: tw`text-sm2`, // 12px
}

const StyledText = styled.text(() => [
  tw`text-gray-450`,
  ({ fontSize = '1' }) => fontSizes[fontSize],
])
