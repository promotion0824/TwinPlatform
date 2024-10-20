import {
  Menu as MantineMenu,
  MenuProps as MantineMenuProps,
} from '@mantine/core'

import { ForwardRefWithStaticComponents } from '../../utils'
import { MenuDivider } from './MenuDivider'
import { MenuDropdown } from './MenuDropdown'
import { MenuItem } from './MenuItem'
import { MenuLabel } from './MenuLabel'
import { MenuSubMenu } from './MenuSubMenu'
import { MenuTarget } from './MenuTarget'

export interface MenuProps extends MantineMenuProps {
  defaultOpened?: MantineMenuProps['defaultOpened']
  opened?: MantineMenuProps['opened']
  children?: MantineMenuProps['children']
  disabled?: MantineMenuProps['disabled']

  /**
   * Event which should open menu.
   * @default 'click'
   * @types 'click' |'hover'
   */
  trigger?: MantineMenuProps['trigger']
  onChange?: MantineMenuProps['onChange']
  onClose?: MantineMenuProps['onClose']
  onOpen?: MantineMenuProps['onOpen']
  onPositionChange?: MantineMenuProps['onPositionChange']

  closeOnClickOutside?: MantineMenuProps['closeOnClickOutside']
  closeOnEscape?: MantineMenuProps['closeOnEscape']
  closeOnItemClick?: MantineMenuProps['closeOnItemClick']

  width?: MantineMenuProps['width']
  /** @default 'bottom-start' */
  position?: MantineMenuProps['position']
  /** @default 3 */
  offset?: MantineMenuProps['offset']
  /** @default true */
  withinPortal?: MantineMenuProps['withinPortal']
  zIndex?: MantineMenuProps['zIndex']
}

type MenuComponent = ForwardRefWithStaticComponents<
  MenuProps,
  {
    Item: typeof MenuItem
    Label: typeof MenuLabel
    Dropdown: typeof MenuDropdown
    Target: typeof MenuTarget
    Divider: typeof MenuDivider
    SubMenu: typeof MenuSubMenu
  }
>
/**
 * A `Menu` displays a list of actions. The Menu component handles the state
 * management of the passed in list of actions.
 *
 * Inside each `Menu`, it will need one `Menu.Target` and one `Menu.Dropdown`.
 */
// MantineMenu do not have ref
export const Menu: MenuComponent = ({
  offset = 3,
  trigger = 'click',
  position = 'bottom-start',
  withinPortal = true,
  ...restProps
}: MenuProps) => {
  return (
    <MantineMenu
      {...restProps}
      offset={offset}
      trigger={trigger}
      position={position}
      withinPortal={withinPortal}
    />
  )
}

Menu.Item = MenuItem
Menu.Label = MenuLabel
Menu.Dropdown = MenuDropdown
Menu.Target = MenuTarget
Menu.Divider = MenuDivider
Menu.SubMenu = MenuSubMenu
Menu.displayName = 'Menu'
