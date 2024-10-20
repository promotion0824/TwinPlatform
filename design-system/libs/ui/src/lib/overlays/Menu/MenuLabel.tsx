import {
  Menu as MantineMenu,
  MenuLabelProps as MantineMenuLabelProps,
} from '@mantine/core'
import { ForwardedRef, forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface MenuLabelProps
  extends WillowStyleProps,
    Omit<MantineMenuLabelProps, keyof WillowStyleProps> {
  children?: MantineMenuLabelProps['children']
}

/**
 * `Menu.Label` a category headings for a group of menu items. It aids in
 * organizing and grouping related items, and usually used with `<Menu.Divider>`.
 */
export const MenuLabel = forwardRef<HTMLDivElement, MenuLabelProps>(
  (props, ref) => {
    return (
      <StyledMenuLabel {...props} {...useWillowStyleProps(props)} ref={ref} />
    )
  }
)

const StyledMenuLabel = styled(MantineMenu.Label)<
  MantineMenuLabelProps & { ref: ForwardedRef<HTMLDivElement> }
>(
  ({ theme }) => css`
    ${theme.font.heading.group};
    background-color: ${theme.color.neutral.bg.panel.default};
    color: ${theme.color.neutral.fg.muted};
    padding: ${theme.spacing.s6} ${theme.spacing.s8};
  `
)
