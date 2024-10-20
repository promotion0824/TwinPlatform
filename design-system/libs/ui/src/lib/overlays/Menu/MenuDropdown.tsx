import {
  Menu as MantineMenu,
  MenuDropdownProps as MantineMenuDropdownProps,
} from '@mantine/core'
import { ForwardedRef, forwardRef } from 'react'
import styled, { css } from 'styled-components'

export interface MenuDropdownProps extends MantineMenuDropdownProps {
  children?: MantineMenuDropdownProps['children']
}

/**
 * `MenuDropdown` is the dropdown that opens when toggle `MenuTrigger`.
 *
 * A `MenuDropdown` can contain any number of `Menu.Item` or `Menu.SubMenu`.
 * Any number of optional `Menu.Label` and `Menu.Divider` can be placed
 * between `Menu.Item`.
 */
export const MenuDropdown = forwardRef<HTMLDivElement, MenuDropdownProps>(
  ({ ...restProps }, ref) => {
    return <StyledMenuDropdown {...restProps} ref={ref} />
  }
)

const StyledMenuDropdown = styled(MantineMenu.Dropdown)<
  MantineMenuDropdownProps & { ref: ForwardedRef<HTMLDivElement> }
>(
  ({ theme }) => css`
    border-radius: ${theme.radius.r2};
    background-color: ${theme.color.neutral.bg.panel.default};
    border: 1px solid ${theme.color.neutral.border.default};

    display: flex;
    padding: ${theme.spacing.s4};
    flex-direction: column;

    > [data-menu-dropdown] {
      width: 100%;
    }

    /* RadioGroup or CheckboxGroup */
    .mantine-InputWrapper-root, // v6, might be able to remove
    .mantine-Radio-root,
    .mantine-Checkbox-root {
      gap: 0;

      .mantine-Radio-body,
      .mantine-Radio-body .mantine-Radio-labelWrapper,
      .mantine-Radio-labelWrapper .mantine-Radio-label, // for upgrade v7 fix
      .mantine-Checkbox-body,
      .mantine-Checkbox-body .mantine-Checkbox-labelWrapper {
        width: 100%;
      }
    }

    /* Radio with/without RadioGroup */
    .mantine-Radio-radio,
    .mantine-Radio-label,
    /* Checkbox with/without CheckboxGroup */
    .mantine-Checkbox-input,
    .mantine-Checkbox-label {
      cursor: pointer;
    }
  `
)
