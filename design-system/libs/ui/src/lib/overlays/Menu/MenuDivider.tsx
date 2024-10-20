import {
  Menu as MantineMenu,
  MenuDividerProps as MantineMenuDividerProps,
} from '@mantine/core'
import { ForwardedRef, forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface MenuDividerProps
  extends WillowStyleProps,
    Omit<MantineMenuDividerProps, keyof WillowStyleProps> {}

/**
 * `Menu.Divider` is to visually separate groups of related menu items in
 * a `Menu.Dropdown`.
 */
export const MenuDivider = forwardRef<HTMLDivElement, MenuDividerProps>(
  (props, ref) => {
    return (
      <StyledMenuDivider {...props} {...useWillowStyleProps(props)} ref={ref} />
    )
  }
)

const StyledMenuDivider = styled(MantineMenu.Divider)<
  MantineMenuDividerProps & { ref: ForwardedRef<HTMLDivElement> }
>(
  ({ theme }) => css`
    border-color: ${theme.color.neutral.border.default};
    margin: ${theme.spacing.s8};
    border-top-width: 1px;
  `
)
