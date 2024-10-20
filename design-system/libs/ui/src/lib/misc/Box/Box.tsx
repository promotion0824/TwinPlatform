import {
  Box as MantineBox,
  BoxProps as MantineBoxProps,
  createPolymorphicComponent,
} from '@mantine/core'
import { baseTheme } from '@willowinc/theme'
import React, { ForwardedRef, ReactElement, forwardRef } from 'react'
import styled from 'styled-components'

import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

// The `component` property cannot be applied to
// `BoxProps` which will cause type errors. To inform users
// about these properties, we have defined this `PropsForDocuments`
// specifically for documentation purposes.
interface PropsForDocuments {
  /**
   * Change the root element with component prop.
   * @default 'div'
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component?: any // Mantine's component prop is any
}

export interface BoxProps
  extends WillowStyleProps,
    Omit<MantineBoxProps, keyof WillowStyleProps | 'hiddenFrom'> {
  /** Breakpoint above which the component is hidden with `display: none` */
  hiddenFrom?: keyof typeof baseTheme.breakpoints
  /**
   * `renderRoot` is an alternative to the `component` prop, which accepts a function
   * that should return a React element. It is useful in cases when `component` prop
   * cannot be used, for example, when the component that you want to pass to the
   * `component` is generic (accepts type or infers it from props, for example `<Link<'/'> />`).
   */
  renderRoot?: (props: Record<string, unknown>) => ReactElement
}

/**
 * `Box` is a polymorphic component which can be used as a replacement for HTML elements.
 */
export const Box = createPolymorphicComponent<'div', BoxProps>(
  forwardRef<HTMLDivElement, BoxProps>(({ hiddenFrom, ...restProps }, ref) => {
    return (
      <StyledMantineBox
        $hiddenFrom={hiddenFrom}
        ref={ref}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      />
    )
  })
)

const StyledMantineBox = styled(MantineBox)<
  {
    ref: ForwardedRef<HTMLDivElement>
  } & {
    $hiddenFrom: BoxProps['hiddenFrom']
  }
>(({ $hiddenFrom, theme }) => {
  if (!$hiddenFrom) return {}

  const hiddenFromBreakpoint = `@media (min-width: ${theme.breakpoints[$hiddenFrom]})`
  return {
    [hiddenFromBreakpoint]: {
      display: 'none',
    },
  }
})

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<
  HTMLDivElement,
  PropsForDocuments & BoxProps
>(() => <div />)
