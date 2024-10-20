import pluralize from 'pluralize'

import { Text } from '@willow/ui'
import tw, { styled } from 'twin.macro'
import TwinTypeSvg from '@willow/ui/components/TwinChip/TwinTypeSvg'

const Tile = styled.div`
  ${tw`
    flex
    flex-col
    border
    border-solid
    border-gray-550
  `}

  color: inherit;
  text-decoration: none;
  max-width: 80px;
  border-radius: 2px;
  &:hover {
    box-shadow: 0 0 6px rgba(255, 255, 255, 0.2);
    cursor: pointer;
  }
`

const IconPane = tw.div`
  h-[78px]
  w-[78px]
  flex
  justify-center
  items-center
  bg-gray-550
`

const LabelPane = tw(Text)`
  flex
  justify-center
  items-center
  h-7
  bg-gray-252525
  text-center
`

const EllipsizedText = styled(Text)({
  overflow: 'hidden',
  textOverflow: 'ellipsis',

  // Ellipsized text on second line.
  display: '-webkit-box', // Despite having "webkit" in its name, the following properties will still work as expected in firefox.
  '-webkit-box-orient': 'vertical',
  '-webkit-line-clamp': '2',
})

export default function TwinTypeTile({ modelOfInterest, onClick }) {
  return (
    <Tile data-testid={pluralize(modelOfInterest.name)} onClick={onClick}>
      <IconPane>
        <TwinTypeSvg
          modelOfInterest={modelOfInterest}
          style={{ width: '64px', height: '64px' }}
        />
      </IconPane>

      <LabelPane title={modelOfInterest.name}>
        <EllipsizedText size="extraTiny">
          {pluralize(modelOfInterest.name)}
        </EllipsizedText>
      </LabelPane>
    </Tile>
  )
}
