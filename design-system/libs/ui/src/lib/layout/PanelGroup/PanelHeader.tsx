import { rem } from '@mantine/core'
import { forwardRef, Fragment, HTMLProps } from 'react'
import styled from 'styled-components'
import { IconButton } from '../../buttons/Button'
import { usePanelGroupContext } from './PanelGroupContext'
import { usePanelContext } from './PanelContext'
import { Box } from '../../misc/Box'
import { WillowStyleProps } from '../../utils'

export const PANEL_HEADER_CONTROLS = 'panel-header-controls' as const

export interface PanelHeaderProps
  extends WillowStyleProps,
    Omit<HTMLProps<HTMLDivElement>, 'as'> {
  /** Optional controls to be displayed on the right side of the header. */
  headerControls?: React.ReactNode
  /**
   * Hide the header border on title variants.
   * Primarily useful if you want a collapse button but no other header content.
   * @default false
   */
  hideBorder?: boolean
  /**
   * Adjusts the styling of the header depending on the variant selected.
   * @default 'title'
   */
  variant?: 'title' | 'tabs'
}

const getCollapseIcon = (isVertical = false, isCollapsed = false) => {
  if (isVertical) {
    return isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up'
  }
  return isCollapsed ? 'last_page' : 'first_page'
}

export const CollapseButton = ({
  isShown = false,
  isCollapsed = false,
  isVertical,
  onClick,
}: {
  isShown?: boolean
  /**
   * Whether panel is current collapsed
   */
  isCollapsed?: boolean
  isVertical: boolean
  onClick: () => void
}) => {
  return isShown ? (
    <IconButton
      kind="secondary"
      background="transparent"
      icon={getCollapseIcon(isVertical, isCollapsed)}
      // need to match or contain content text for a11y
      aria-label={getCollapseIcon(isVertical, isCollapsed)}
      // temp solution for screen reader when aria-label doesn't indicate the purpose
      title={isCollapsed ? 'expand' : 'collapse'}
      data-testid={isCollapsed ? 'expand-panel' : 'collapse-panel'}
      onClick={onClick}
    />
  ) : null
}

const HeaderControlsOuterContainer = styled.div(({ theme }) => ({
  alignItems: 'center',
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  display: 'flex',
  paddingRight: theme.spacing.s8,
}))

const HeaderControlsInnerContainer = styled.div(({ theme }) => ({
  paddingRight: theme.spacing.s8,
}))

const PanelHeaderBox = styled(Box<'div'>)<{
  $hideBorder: PanelHeaderProps['hideBorder']
  $variant: PanelHeaderProps['variant']
}>(({ $hideBorder, $variant, theme }) => ({
  alignItems: $variant === 'tabs' ? 'stretch' : 'center',
  backgroundColor: theme.color.neutral.bg.panel.default,
  boxSizing: 'content-box',
  display: 'flex',

  borderBottom: $hideBorder
    ? 'none'
    : `1px solid ${theme.color.neutral.border.default}`,

  ...($variant === 'title'
    ? {
        minHeight: rem(28), // This doesn't play nicely with the tab variant, though the height is the same regardless
        padding: `${theme.spacing.s8} ${theme.spacing.s8} ${theme.spacing.s8} ${theme.spacing.s16}`,
      }
    : {
        // $variant === 'tabs'
        padding: 0,
        borderBottom: 'none',
      }),
}))

export const PanelHeaderContent = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,
  flex: '1',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  textWrap: 'nowrap',
}))

/**
 * `PanelHeader` is the header for the panel. This is used to display the heading
 * and the action buttons.
 */
const PanelHeader = forwardRef<HTMLDivElement, PanelHeaderProps>(
  (
    {
      children,
      headerControls,
      hideBorder = false,
      variant = 'title',
      ...restProps
    },
    ref
  ) => {
    const { collapsible, onCollapse } = usePanelContext()
    const { isVertical } = usePanelGroupContext()

    const HeaderControlsWrapper =
      variant === 'tabs' ? HeaderControlsOuterContainer : Fragment

    return (
      <PanelHeaderBox
        $hideBorder={hideBorder}
        $variant={variant}
        data-testid="panel-header"
        {...restProps}
        ref={ref}
      >
        <PanelHeaderContent>{children}</PanelHeaderContent>
        <HeaderControlsWrapper
          {...(variant === 'tabs' ? { className: PANEL_HEADER_CONTROLS } : {})}
        >
          {headerControls !== undefined && (
            <HeaderControlsInnerContainer>
              {headerControls}
            </HeaderControlsInnerContainer>
          )}

          <CollapseButton
            isShown={collapsible}
            isVertical={isVertical}
            onClick={() => onCollapse(true)}
          />
        </HeaderControlsWrapper>
      </PanelHeaderBox>
    )
  }
)

export default PanelHeader
