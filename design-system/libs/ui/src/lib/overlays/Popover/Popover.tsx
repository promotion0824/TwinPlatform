import {
  Popover as MantinePopover,
  PopoverProps as MantinePopoverProps,
} from '@mantine/core'

import { ForwardRefWithStaticComponents } from '../../utils'
import { PopoverDropdown } from './PopoverDropdown'
import { PopoverTarget } from './PopoverTarget'
export interface PopoverProps extends Omit<MantinePopoverProps, 'arrowSize'> {
  children: MantinePopoverProps['children']
  disabled?: MantinePopoverProps['disabled']

  defaultOpened?: MantinePopoverProps['defaultOpened']
  opened?: MantinePopoverProps['opened']
  onChange?: MantinePopoverProps['onChange']
  onClose?: MantinePopoverProps['onClose']
  onOpen?: MantinePopoverProps['onOpen']
  /**
   * Initial position of Popover.Dropdown. However, regardless of the
   * set position, it will be automatically adjusted to ensure it fits
   * in the screen.
   *
   * @default 'bottom'
   */
  position?: MantinePopoverProps['position']
  /** @default true */
  closeOnClickOutside?: MantinePopoverProps['closeOnClickOutside']
  /** @default true */
  closeOnEscape?: MantinePopoverProps['closeOnEscape']

  /** @default false */
  withArrow?: MantinePopoverProps['withArrow']
  withinPortal?: MantinePopoverProps['withinPortal']
  portalProps?: MantinePopoverProps['portalProps']
}

type PopoverComponent = ForwardRefWithStaticComponents<
  PopoverProps,
  {
    Target: typeof PopoverTarget
    Dropdown: typeof PopoverDropdown
  }
>

/**
 * `Popover` contains a `Popover.Trigger` and a `Popover.Dropdown`.
 *
 * The position of `Popover.Dropdown` will be automatically adjusted to ensure
 * it fits in the screen, no matter what position is set.
 */
// MantinePopoverProps do not have ref prop
export const Popover: PopoverComponent = ({
  position = 'bottom',
  closeOnClickOutside = true,
  closeOnEscape = true,
  withArrow = false,
  ...restProps
}) => {
  return (
    <MantinePopover
      {...restProps}
      position={position}
      closeOnClickOutside={closeOnClickOutside}
      closeOnEscape={closeOnEscape}
      withArrow={withArrow}
      // Fix arrowSize to keep the design consistent
      arrowSize={8}
    />
  )
}

Popover.Target = PopoverTarget
Popover.Dropdown = PopoverDropdown
Popover.displayName = 'Popover'
