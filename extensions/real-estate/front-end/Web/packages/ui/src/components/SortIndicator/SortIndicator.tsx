import { ReactNode } from 'react'
import styled from 'styled-components'
import Icon from '../Icon/Icon'

/**
 * display an icon either on top or bottom of some text
 * to indicate sorting order, by default, a chevron icon
 * pointing top is displayed.
 */
export default function SortIndicator({
  isSorted,
  $transform = 'translateY(-12px) rotate(-180deg)',
  icon = 'chevron',
  iconSize = 'medium',
  children,
}: {
  isSorted: boolean
  $transform?: string
  icon?: string
  iconSize?: string
  children?: ReactNode
}) {
  return (
    <IconContainer>
      {children}
      <div>
        {isSorted && (
          <TransformedIcon
            icon={icon}
            size={iconSize}
            $transform={$transform}
          />
        )}
      </div>
    </IconContainer>
  )
}

const IconContainer = styled.div({
  position: 'relative',
  width: '100%',
  height: '100%',
  maxWidth: 'fit-content',
  display: 'flex',
  alignItems: 'center',
  '& > div': {
    pointerEvents: 'none',
    position: 'absolute',
    width: '100%',
    display: 'flex',
    justifyContent: 'center',
  },
})

const TransformedIcon = styled(Icon)<{ $transform: string }>(
  ({ $transform }) => ({
    transition: 'translateY',
    transform: $transform,
  })
)
