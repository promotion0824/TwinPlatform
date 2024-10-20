import {
  SwitchGroupProps as MantineSwitchGroupProps,
  Switch,
} from '@mantine/core'
import { forwardRef } from 'react'
import { Group } from '../../layout/Group'
import { Stack } from '../../layout/Stack'
import {
  CommonInputProps,
  getCommonInputProps,
  renderChildrenWithProps,
} from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export interface BaseProps
  extends Omit<CommonInputProps<string[]>, 'disabled'> {
  /** `Switch` components. */
  children: React.ReactNode
  /**
   * Display the switches horizontally inline.
   * @default false
   */
  inline?: boolean
}

export interface SwitchGroupProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineSwitchGroupProps, keyof WillowStyleProps> {}

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/** `SwitchGroup` is used to group a set of `Switch` components together. */
export const SwitchGroup = forwardRef<HTMLDivElement, SwitchGroupProps>(
  ({ children, labelWidth, error, inline = false, ...restProps }, ref) => {
    const childrenWithProps = renderChildrenWithProps(children, {
      error: !!error,
    })

    return (
      <Switch.Group
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
        ref={ref}
      >
        {inline ? (
          <Group>{childrenWithProps}</Group>
        ) : (
          <Stack>{childrenWithProps}</Stack>
        )}
      </Switch.Group>
    )
  }
)
