import { NavLink, NavLinkProps } from '@mantine/core'
import { forwardRef, useState } from 'react'
import styled from 'styled-components'
import { Icon, IconName } from '../../misc/Icon'
import { rem } from '../../utils'
import { WillowStyleProps } from '../../utils/willowStyleProps'

export const ERR_ACTIVE_WITH_CHILDREN =
  '"active" cannot be set on a "NavList.Item" with children.'
export const ERR_DEFAULT_OPENED_WITHOUT_CHILDREN =
  '"defaultOpened" cannot be set on a "NavList.Item" without children.'

function hasActiveChild(children: React.ReactNode): boolean {
  return (
    Array.isArray(children) &&
    children.some(({ props }) => props.active || hasActiveChild(props.children))
  )
}

export interface NavListItemProps
  extends Omit<NavLinkProps, keyof WillowStyleProps> {
  active?: NavLinkProps['active']
  /**
   * If a list item has nested items, set its default open state.
   * @default false
   */
  defaultOpened?: NavLinkProps['defaultOpened']
  /** Icon to be displayed to the left of the label. */
  icon?: IconName
  /** Label displayed for the item. */
  label: NavLinkProps['label']
  /** Called when the item is clicked. */
  onClick?: () => void
}

const ActiveIndicator = styled.div(({ theme }) => ({
  backgroundColor: theme.color.intent.primary.bg.bold.default,
  width: rem(3),
}))

const Container = styled.div<{ $opened: boolean }>(({ $opened, theme }) => ({
  display: 'flex',

  ...($opened
    ? {
        flexDirection: 'column',

        '& > :nth-child(2)': {
          display: 'flex',

          '& div': {
            flexGrow: 1,
          },
        },

        '& > :nth-child(2)::before': {
          backgroundColor: theme.color.neutral.border.default,
          content: '""',
          display: 'block',
          left: theme.spacing.s24,
          position: 'relative',
          width: rem(1),
        },
      }
    : {
        flexDirection: 'row',
      }),
}))

const StyledNavList = styled(NavLink<'button'>)<{
  $containerOpened: boolean
  $hasChildren: boolean
}>(({ active, $containerOpened, $hasChildren, theme }) => ({
  borderRadius: theme.radius.r2,
  color: theme.color.neutral.fg.default,
  width: $hasChildren && $containerOpened ? 'auto' : '100%',

  margin: active
    ? `0 ${rem(5)} 0 ${theme.spacing.s8}`
    : `0 ${theme.spacing.s8}`,

  padding: `${theme.spacing.s4} ${theme.spacing.s8}`,

  '&[data-active]': {
    backgroundColor: theme.color.neutral.bg.accent.default,
    color: theme.color.neutral.fg.default,

    '&:hover': {
      backgroundColor: theme.color.neutral.bg.accent.default,
    },
  },

  '&:focus-visible': {
    outline: `1px solid ${theme.color.state.focus.border}`,
    outlineOffset: '-1px',
  },

  '&:hover': {
    backgroundColor: theme.color.neutral.bg.panel.hovered,
  },

  '.mantine-NavLink-icon': {
    marginRight: theme.spacing.s8,
  },

  '.mantine-NavLink-label': {
    ...theme.font.heading.xs,
  },
}))

/**
 * `NavList.Item` is a link item in the side navigation.
 */
export const NavListItem = forwardRef<HTMLButtonElement, NavListItemProps>(
  ({ active, children, defaultOpened, icon, ...restProps }, ref) => {
    const [opened, setOpened] = useState(!!defaultOpened)
    const [containerOpened, setContainerOpened] = useState(opened)

    const hasChildren = !!children
    const isActive = hasChildren
      ? !containerOpened && hasActiveChild(children)
      : active

    if (hasChildren && active !== undefined) {
      throw new Error(ERR_ACTIVE_WITH_CHILDREN)
    }

    if (!hasChildren && defaultOpened !== undefined) {
      throw new Error(ERR_DEFAULT_OPENED_WITHOUT_CHILDREN)
    }

    return (
      <Container $opened={containerOpened}>
        <StyledNavList
          // If a child is active, display the activity indicator on the parent when it is collapsed.
          active={isActive}
          children={children}
          childrenOffset={30}
          $containerOpened={containerOpened}
          disableRightSectionRotation
          $hasChildren={hasChildren}
          leftSection={icon && <Icon icon={icon} />}
          onChange={() => {
            setOpened(!opened)

            // Closing the container is on a delay so that the switch to a row layout
            // doesn't happen too quicky and cause a flicker. Conversely when opening again,
            // this happens instantly so that the children are shown immediately.
            if (containerOpened) {
              setTimeout(() => setContainerOpened(false), 200)
            } else {
              setContainerOpened(true)
            }
          }}
          opened={opened}
          ref={ref}
          rightSection={
            hasChildren && (
              <Icon
                icon={opened ? 'keyboard_arrow_up' : 'keyboard_arrow_down'}
              />
            )
          }
          tabIndex={0} // enable focus with Tab key
          {...restProps}
        />
        {isActive && <ActiveIndicator />}
      </Container>
    )
  }
)
