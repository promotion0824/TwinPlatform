import {
  Collapse as MantineCollapse,
  CollapseProps as MantineCollapseProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

interface BaseProps {
  /** Opened state */
  opened: MantineCollapseProps['in']
  /** Called each time transition ends */
  onTransitionEnd?: MantineCollapseProps['onTransitionEnd']
}

export interface CollapseProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineCollapseProps, keyof WillowStyleProps | 'in'> {}

/**
 * `Collapse` is a wrapper component that animates its children to collapse or expand.
 */
export const Collapse = forwardRef<HTMLDivElement, CollapseProps>(
  ({ opened, ...restProps }, ref) => {
    return (
      <MantineCollapse
        {...restProps}
        {...useWillowStyleProps(restProps)}
        in={opened}
        ref={ref}
      />
    )
  }
)
/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)
