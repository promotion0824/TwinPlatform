import {
  Popover as MantinePopover,
  PopoverDropdownProps as MantinePopoverDropdownProps,
} from '@mantine/core'
import { ReactNode, forwardRef } from 'react'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface PopoverDropdownProps
  extends WillowStyleProps,
    Omit<MantinePopoverDropdownProps, keyof WillowStyleProps> {
  /** The popover content */
  children: ReactNode
}

export const PopoverDropdown = forwardRef<HTMLDivElement, PopoverDropdownProps>(
  (props, ref) => {
    return (
      <MantinePopover.Dropdown
        ref={ref}
        {...props}
        {...useWillowStyleProps(props)}
      />
    )
  }
)
