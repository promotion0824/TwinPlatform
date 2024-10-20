import { MouseEvent, ReactNode } from 'react'

import {
  ButtonProps,
  IconButton,
  IconName,
  Menu,
  PopoverProps,
} from '@willowinc/ui'

/** Support all props from @willowinc/ui/Menu.Item  */
const MoreButtonDropdownOption = Menu.Item

/** Support all props from @willowinc/ui/Menu.Divider  */
const MoreButtonDropdownOptionDivider = Menu.Divider

type MoreButtonDropdownProps = Partial<PopoverProps> & {
  /**
   * To replace the icon in target button.
   * @default 'more_vert'
   */
  targetButtonIcon?: IconName
  targetButtonProps?: Partial<ButtonProps>
  children: ReactNode[]
}

/**
 * Use with more than 1 `MoreButtonDropdownOption`.
 * @example
 * <MoreButtonDropdown>
 *  <MoreButtonDropdownOption onClick={} prefix={<Icon />}>
 *    Preview
 *  </MoreButtonDropdownOption>
 *  <MoreButtonDropdownOption onClick={} prefix={<Icon />}>
 *    Edit
 *  </MoreButtonDropdownOption>
 * </MoreButtonDropdown>
 */
const MoreButtonDropdown = ({
  targetButtonIcon = 'more_vert',
  children,
  targetButtonProps,
  ...restProps
}: MoreButtonDropdownProps) => (
  <Menu
    withinPortal={false} // so that we don't need to worry about z-index with modal for example
    {...restProps}
  >
    <Menu.Target>
      <IconButton
        kind="secondary"
        icon={targetButtonIcon}
        onClick={(e: MouseEvent<HTMLButtonElement>) => {
          // so that when the dropdown button is clicked inside another
          // clickable content, it won't trigger the parent's onClick.
          e.preventDefault()
          e.stopPropagation()

          targetButtonProps?.onClick?.(e)
        }}
        {...targetButtonProps}
      />
    </Menu.Target>
    <Menu.Dropdown>{children}</Menu.Dropdown>
  </Menu>
)

export {
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  MoreButtonDropdownOptionDivider,
}
