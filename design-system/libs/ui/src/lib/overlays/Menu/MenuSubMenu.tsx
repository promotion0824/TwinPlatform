import { forwardRef } from 'react'

import { Menu, type MenuProps } from './Menu'
import { MenuItem, type MenuItemProps } from './MenuItem'

export interface MenuSubMenuProps extends MenuItemProps {
  /** Customizable props will pass to the Menu component. */
  menuProps?: MenuProps
}

/**
 * `Menu.SubMenu` is a menu container that used inside the dropdown of another
 * `Menu` as a `Menu.MenuItem`. Rest props will be passed to the outer `Menu.MenuItem`
 * component.
 */
export const MenuSubMenu = forwardRef<HTMLButtonElement, MenuSubMenuProps>(
  ({ children, menuProps, ...restProps }, ref) => {
    return (
      <MenuItem closeMenuOnClick={false} {...restProps} ref={ref}>
        <Menu
          position="right-start"
          offset={
            8 /* button padding of MenuItem */ +
            4 /* padding of dropdown */ +
            1 /* dropdown border width */ +
            3 /* designed gap between dropdowns */
          }
          withinPortal={false}
          {...menuProps}
        >
          {children}
        </Menu>
      </MenuItem>
    )
  }
)
