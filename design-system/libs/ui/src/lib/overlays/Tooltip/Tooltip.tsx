import {
  Tooltip as MantineTooltip,
  TooltipProps as MantineTooltipProps,
} from '@mantine/core'
import { ForwardedRef, MutableRefObject, RefObject, forwardRef } from 'react'
import styled from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface TooltipProps
  extends WillowStyleProps,
    Omit<MantineTooltipProps, keyof WillowStyleProps | 'arrowSize'> {
  label: MantineTooltipProps['label']
  children: MantineTooltipProps['children']
  disabled?: MantineTooltipProps['disabled']

  opened?: MantineTooltipProps['opened']
  /**
   * Initial position of tooltip content. However, regardless of the
   * set position, it will be automatically adjusted to ensure it fits
   * on the screen.
   *
   * @default 'top'
   */
  position?: MantineTooltipProps['position']
  /**
   * Open delay in ms.
   * @default 500
   */
  openDelay?: MantineTooltipProps['openDelay']
  closeDelay?: MantineTooltipProps['closeDelay']
  /** @default false */
  inline?: MantineTooltipProps['inline']
  /**
   * Determines which events will be used to show tooltip,
   * default to be triggered by hover
   */
  events?: MantineTooltipProps['events']
  /** @default false */
  multiline?: MantineTooltipProps['multiline']
  /** Tooltip popover width */
  width?: MantineTooltipProps['w']
  /**
   * Determines whether tooltip should have an arrow pointing to the trigger element
   * @default false
   */
  withArrow?: MantineTooltipProps['withArrow']

  /**
   * Determines whether tooltip should be rendered within Portal,
   * default to be false which will be rendered above the trigger element.
   * This will be helpful if the tooltip is hidden behind other elements when
   * overflowed.
   *
   * @default false
   */
  withinPortal?: MantineTooltipProps['withinPortal']
}

/**
 *  A `Tooltip` is a popup that displays additional information related to an element when the element receives
 * keyboard focus or the mouse hovers over it. The information included should be contextual, helpful, and
 * nonessential while providing that extra ability to communicate and give clarity to a user.
 *
 * `<Tooltip/>` requires an element or a component as a single child – strings,
 * fragments, numbers and multiple elements/components are not supported
 * and will throw error. Custom components must provide a prop to get root
 * element ref.
 */
export const Tooltip = forwardRef<HTMLDivElement | HTMLElement, TooltipProps>(
  (
    {
      width,
      inline = false,
      multiline = false,
      openDelay = 500,
      withinPortal = false,
      withArrow = false,
      ...restProps
    },
    ref
  ) => {
    return (
      <StyledTooltip
        w={width}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        inline={inline}
        multiline={multiline}
        openDelay={openDelay}
        withinPortal={withinPortal}
        withArrow={withArrow}
        // Fix arrowSize to keep the design consistent
        arrowSize={8}
        ref={
          ref as RefObject<HTMLDivElement> &
            MutableRefObject<HTMLElement | null>
        }
      />
    )
  }
)

// Need this styled component type trick to apply ref to MantineTooltipProps,
// otherwise we will get a type error when pass ref to MantineTooltip.
// `styled(MantineTooltip)` cannot access the classnames of tooltip content,
// so the styles are defined in globalStyles which has higher level and can
// apply to the classnames.
const StyledTooltip = styled(MantineTooltip)<
  MantineTooltipProps & { ref: ForwardedRef<HTMLElement> }
>``
