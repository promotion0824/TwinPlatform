import { HTMLProps } from 'react'
import styled from 'styled-components'
import { CollapseButton, PanelHeaderContent } from './PanelHeader'
import { Box } from '../../misc/Box'

const CollapsedPanelBox = styled(Box<'div'>)<{
  $isVertical: boolean
}>(({ $isVertical, theme }) => ({
  display: 'flex',
  justifyContent: 'right',

  ...($isVertical
    ? {
        alignItems: 'center',
        padding: `${theme.spacing.s8} ${theme.spacing.s8} ${theme.spacing.s8} ${theme.spacing.s16}`,
      }
    : {
        padding: theme.spacing.s8,
      }),
}))

const CollapsedPanel = ({
  isVertical,
  onExpand,
  title,
  ...restProps
}: Omit<HTMLProps<HTMLDivElement>, 'as' | 'children' | 'ref' | 'title'> & {
  isVertical: boolean
  onExpand: () => void
  title?: React.ReactNode
}) => {
  return (
    <CollapsedPanelBox $isVertical={isVertical} {...restProps}>
      {title && isVertical && <PanelHeaderContent>{title}</PanelHeaderContent>}

      <CollapseButton
        isShown
        onClick={onExpand}
        isVertical={isVertical}
        isCollapsed
      />
    </CollapsedPanelBox>
  )
}

export default CollapsedPanel
