import {
  Menu as MantineMenu,
  MenuItemProps as MantineMenuItemProps,
  createPolymorphicComponent,
} from '@mantine/core'
import { ForwardedRef, forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface MenuItemProps
  extends WillowStyleProps,
    Omit<
      MantineMenuItemProps,
      keyof WillowStyleProps | 'leftSection' | 'rightSection'
    > {
  children: MantineMenuItemProps['children']
  /** Content rendered on the starting of the label */
  prefix?: MantineMenuItemProps['leftSection']
  /** Content rendered on the end of the label */
  suffix?: MantineMenuItemProps['rightSection']
  /** @default 'secondary' */
  intent?: 'secondary' | 'negative'
  /**
   * Determines whether menu should be closed when item is clicked, overrides
   * `closeOnItemClick` prop on Menu component.
   *
   * By default, clicking a `Menu.Item` will close the dropdown.
   */
  closeMenuOnClick?: MantineMenuItemProps['closeMenuOnClick']
}

/**
 * `MenuItem` is the container for each menu item.
 */
const _MenuItem = forwardRef<HTMLButtonElement, MenuItemProps>(
  ({ prefix, suffix, intent = 'secondary', ...restProps }, ref) => {
    return (
      <StyledMenuItem
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
        leftSection={prefix}
        rightSection={suffix}
        $intent={intent}
      />
    )
  }
)
export const MenuItem = createPolymorphicComponent<'button', MenuItemProps>(
  _MenuItem
)

const StyledMenuItem = styled(MantineMenu.Item)<
  MantineMenuItemProps & { ref: ForwardedRef<HTMLButtonElement> } & {
    $intent: Exclude<MenuItemProps['intent'], undefined>
  }
>(({ theme, $intent }) => {
  const colors = {
    secondary: {
      fg: theme.color.neutral.fg.default,
      hoveredBg: theme.color.intent.secondary.bg.subtle.default,
    },
    negative: {
      fg: theme.color.intent.negative.fg.default,
      hoveredBg: theme.color.intent.negative.bg.subtle.default,
    },
  } as const

  return css`
    border-radius: ${theme.radius.r2};
    background-color: ${theme.color.neutral.bg.panel.default};
    ${theme.font.body.md.regular};
    color: ${colors[$intent].fg};
    padding: ${theme.spacing.s4} ${theme.spacing.s8};
    opacity: 1; // remove Mantine's

    &[data-hovered] {
      background-color: ${colors[$intent].hoveredBg};
    }

    &:focus:focus-visible {
      outline: none;
    }

    &:disabled {
      div,
      label {
        color: ${theme.color.state.disabled.fg};
      }
    }

    .mantine-Button-section {
      &[data-position='left'] {
        margin-right: ${theme.spacing.s4};
      }
      &[data-position='right'] {
        color: ${theme.color.neutral.fg.subtle};
      }
    }
  `
})
