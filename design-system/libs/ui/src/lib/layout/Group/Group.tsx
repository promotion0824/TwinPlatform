import {
  Group as MantineGroup,
  GroupProps as MantineGroupProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import { SpacingValue, getSpacing } from '../../utils/themeSpacing'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export interface BaseProps {
  /**
   * Controls `align-items` CSS property.
   * @default 'center'
   */
  align?: MantineGroupProps['align']
  /**
   * Accepts "spacing" properties from the Willow theme (s2, s4, etc.), any valid CSS value,
   * or numbers. Numbers are converted to rem.
   * @default 's8'
   */
  gap?: SpacingValue
  /**
   * Determines whether each child element should have `flex-grow: 1` style.
   * @default false
   */
  grow?: MantineGroupProps['grow']
  /**
   * Controls `justify-content` CSS property.
   * @default 'flex-start'
   */
  justify?: MantineGroupProps['justify']
  /**
   * Allows you to control how `Group` children should behave when there is not enough space to fit them all on one line.
   * By default, children are not allowed to take more space than `(1 / children.length) * 100%` of parent width.
   * To change this behavior, set `preventGrowOverflow` to `false` and children will be allowed to grow and take as much space as they need.
   * @default true
   */
  preventGrowOverflow?: MantineGroupProps['preventGrowOverflow']
  /**
   * Controls `flex-wrap` CSS property.
   * @default 'wrap'
   */
  wrap?: MantineGroupProps['wrap']
}

export interface GroupProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineGroupProps, keyof WillowStyleProps | 'gap'> {}

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `Group` is used to group elements and components in a horizontal flex container.
 */
export const Group = forwardRef<HTMLDivElement, GroupProps>(
  ({ gap = 's8', ...restProps }, ref) => (
    <MantineGroup
      gap={getSpacing(gap)}
      ref={ref}
      {...restProps}
      {...useWillowStyleProps(restProps)}
    />
  )
)
