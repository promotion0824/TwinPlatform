import { Icon } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { FieldSet, LabelText } from './shared'

type Props = {
  onChange: (color: string) => void
  selectedColor?: string
}

/**
 * Input field that allows users to choose the color for the TwinChip's icon.
 * It will display an array of 16 colors for users to choose from.
 */
export default function ColorPreferencesSection({
  onChange,
  selectedColor,
}: Props) {
  const { t } = useTranslation()
  return (
    <FieldSet label={t('plainText.colorPreferences')}>
      <LabelText>{t('plainText.chooseColor')}</LabelText>
      <ColorPalette onChange={onChange} selectedColor={selectedColor} />
    </FieldSet>
  )
}

const Tile = styled.div<{ $color: string }>(({ $color }) => ({
  width: '25px',
  height: '25px',
  borderRadius: '3px',
  backgroundColor: $color,
  marginRight: '8px',
  '&:hover': {
    cursor: 'pointer !important',
  },
}))

function ColorPalette({ onChange, selectedColor }: Props) {
  const blackCheckMarkColor = '#171717'
  const whiteCheckMarkColor = '#FFFFFF'
  const colorTiles = [
    { color: '#33CA36', checkMarkColor: blackCheckMarkColor },
    { color: '#70DA72', checkMarkColor: blackCheckMarkColor },
    { color: '#417CBF', checkMarkColor: whiteCheckMarkColor },
    { color: '#63A9E3', checkMarkColor: whiteCheckMarkColor },
    { color: '#9CCCEF', checkMarkColor: blackCheckMarkColor },
    { color: '#55FFD1', checkMarkColor: blackCheckMarkColor },
    { color: '#9B3E9D', checkMarkColor: whiteCheckMarkColor },
    { color: '#DD4FC1', checkMarkColor: whiteCheckMarkColor },
    { color: '#DFB9E4', checkMarkColor: blackCheckMarkColor },
    { color: '#E81B29', checkMarkColor: whiteCheckMarkColor },
    { color: '#FF3B48', checkMarkColor: whiteCheckMarkColor },
    { color: '#FD6C76', checkMarkColor: whiteCheckMarkColor },
    { color: '#E57936', checkMarkColor: whiteCheckMarkColor },
    { color: '#FFC11A', checkMarkColor: blackCheckMarkColor },
    { color: '#78949F', checkMarkColor: whiteCheckMarkColor },
    { color: '#D9D9D9', checkMarkColor: blackCheckMarkColor },
  ]

  return (
    <ColorTilesContainer>
      {colorTiles.map(({ color, checkMarkColor }) => (
        <Tile
          key={color}
          $color={color}
          onClick={() => onChange(color)}
          data-testid="color-tile"
        >
          {selectedColor === color && (
            <ColorIcon
              icon="ok"
              $checkMarkColor={checkMarkColor}
              data-testid="selected-color-tile-svg"
            />
          )}
        </Tile>
      ))}
    </ColorTilesContainer>
  )
}

const ColorTilesContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  marginTop: '15px',
})

const ColorIcon = styled(Icon)(({ $checkMarkColor }) => ({
  color: $checkMarkColor,
  marginLeft: '3.7px',
  marginTop: '3px',
}))
