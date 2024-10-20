import tw, { styled } from 'twin.macro'
import Icon from '../Icon/Icon'

const ChipVariants = {
  yellow: tw`bg-yellow-500 bg-opacity-10 text-yellow-500 h-6 leading-4`,
  orange: tw`bg-orange-450 bg-opacity-10 text-orange-450 h-6 leading-4`,
  pink: tw`bg-pink-450 bg-opacity-10 text-pink-350 h-6 leading-4`,
  blue: tw`bg-blue-550 bg-opacity-10 text-blue-350 h-6 leading-4`,
  gray: tw`text-gray-450 bg-gray-350 bg-opacity-10 h-6 leading-4`,
  purple: tw`bg-purple-450 bg-opacity-10 text-purple-450 h-6 leading-4`,
}

const fontSizes = {
  1: tw`text-sm1`,
  2: tw`text-sm2`,
}

// default spacing: py-1 px-2
// flexiFilter: py-1 px-2 mt-0.5
const spacings = {
  flexi: tw`py-1 px-2`,
  flexiFilter: tw`py-1 px-2 mt-2`,
}

const StyledChip = styled.div(({ hover = false }) => [
  tw`text-sm1
    flex
    text-center
    rounded-sm
    border-2
    min-w-min
    whitespace-nowrap
    overflow-hidden
    hover:cursor-pointer
    gap-x-2
  `,
  hover ? tw`hover:text-gray-500` : tw`hover:cursor-default`,
  ({ fontSize = '1' }) => fontSizes[fontSize],
  ({ spacing = 'flexi' }) => spacings[spacing],
  ({ variant = 'orange' }) => ChipVariants[variant],
])

export default function Chip({
  variant,
  content,
  fontSize,
  spacing,
  icon,
  hover,
  onClick,
  ...rest
}) {
  return (
    <StyledChip
      variant={variant}
      fontSize={fontSize}
      spacing={spacing}
      hover={hover}
      onClick={onClick}
      {...rest}
    >
      {content}
      {icon && <Icon icon={icon} size="small" />}
    </StyledChip>
  )
}
