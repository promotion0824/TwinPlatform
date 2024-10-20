import {
  Stack as MantineStack,
  StackProps as MantineStackProps,
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
   * @default 'stretch'
   */
  align?: MantineStackProps['align']
  /**
   * Accepts "spacing" properties from the Willow theme (s2, s4, etc.), any valid CSS value,
   * or numbers. Numbers are converted to rem.
   * @default 's8'
   */
  gap?: SpacingValue
  /**
   * Controls `justify-content` CSS property.
   * @default 'flex-start'
   */
  justify?: MantineStackProps['justify']
}

export interface StackProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineStackProps, keyof WillowStyleProps | 'gap'> {}

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `Stack` is used to group elements and components in a vertical flex container.
 */
export const Stack = forwardRef<HTMLDivElement, StackProps>(
  ({ gap = 's8', ...restProps }, ref) => (
    <MantineStack
      gap={getSpacing(gap)}
      ref={ref}
      {...restProps}
      {...useWillowStyleProps(restProps)}
    />
  )
)
