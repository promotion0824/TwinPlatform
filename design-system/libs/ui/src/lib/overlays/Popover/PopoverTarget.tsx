import {
  Popover as MantinePopover,
  PopoverTargetProps as MantinePopoverTargetProps,
} from '@mantine/core'
import { mergeRefs } from '@mantine/hooks'
import { forwardRef } from 'react'
import { useHiddenElementForAccessibility } from '../../utils'

export interface PopoverTargetProps extends MantinePopoverTargetProps {
  children: MantinePopoverTargetProps['children']
  popupType?: MantinePopoverTargetProps['popupType']
  refProp?: MantinePopoverTargetProps['refProp']
}

/**
 * `<Popover.Target/>` requires an element or a component as a single child – strings,
 * fragments, numbers and multiple elements/components are not supported
 * and will throw error. Custom components must provide a prop to get root
 * element ref.
 *
 * @example
 * <Popover.Target>
 *  <button>Native button – ok</button>
 * </Popover.Target>
 *
 * <Popover.Target>
 *   <Button>Our Button – ok</Button>
 * </Popover.Target>
 *
 * <Popover.Target>Raw string – will throw error</Popover.Target>
 *
 * <Popover.Target>{2} – will throw error</Popover.Target>
 *
 * <Popover.Target>
 *   <>Fragment, NOT OK, will throw error</>
 * </Popover.Target>
 *
 * <Popover.Target>
 *   // Multiple nodes, NOT OK – will throw error
 *   <div>More that one node</div>
 *   <div>NOT OK, will throw error</div>
 * </Popover.Target>
 */
export const PopoverTarget = forwardRef<HTMLElement, PopoverTargetProps>(
  ({ ...restProps }, ref) => {
    const { targetRef, hiddenComponent } = useHiddenElementForAccessibility()

    return (
      <>
        <MantinePopover.Target {...restProps} ref={mergeRefs(ref, targetRef)} />
        {hiddenComponent}
      </>
    )
  }
)
