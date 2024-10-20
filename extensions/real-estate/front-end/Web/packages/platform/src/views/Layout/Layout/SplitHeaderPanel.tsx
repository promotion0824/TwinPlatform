import { Portal } from '@willow/ui'
import { styled } from 'twin.macro'

import { useLayout } from './LayoutContext'

const HeaderPanel = styled.div(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  minHeight: '58px',
  paddingLeft: theme.spacing.s16,
  paddingRight: theme.spacing.s16,
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
}))

/**
 * A header panel that renders optional elements on its left and right sides.
 * Renders inside the headerPanelRef of the LayoutContext using a Portal.
 */
export default function SplitHeaderPanel({
  leftElement = <div />,
  rightElement = <div />,
  ...rest
}: {
  leftElement?: React.ReactNode
  rightElement?: React.ReactNode
}) {
  const { headerPanelRef } = useLayout()

  return (
    <Portal target={headerPanelRef}>
      <HeaderPanel {...rest}>
        {leftElement}
        {rightElement}
      </HeaderPanel>
    </Portal>
  )
}
