import {
  UnstyledButton as MantineUnstyledButton,
  UnstyledButtonProps as MantineUnstyledButtonProps,
  createPolymorphicComponent,
} from '@mantine/core'
import { ReactElement, forwardRef } from 'react'
import styled from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

const StyledMantineUnstyledButton = styled(MantineUnstyledButton<'button'>)({
  '&:focus-visible': {
    outline: 'none',
  },
})

// The `component` and `renderRoot` properties cannot be applied to
// `UnstyledButtonProps` which will cause type errors. To inform users
// about these properties, we have defined this `PropsForDocuments`
// specifically for documentation purposes.
interface PropsForDocuments extends WillowStyleProps {
  /**
   * Change the root element with component prop.
   * @default 'button'
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component?: any // Mantine's is any
  /**
   * `renderRoot` is an alternative to the `component` prop, which accepts a function
   * that should return a React element. It is useful in cases when `component` prop
   * cannot be used, for example, when the component that you want to pass to the
   * `component` is generic (accepts type or infers it from props, for example `<Link<'/'> />`).
   */
  renderRoot?: (props: Record<string, unknown>) => ReactElement
}

export interface UnstyledButtonProps
  extends Omit<MantineUnstyledButtonProps, keyof WillowStyleProps>,
    WillowStyleProps {}

/**
 * `UnstyledButton` is a polymorphic, unstyled button component.
 */
export const UnstyledButton = createPolymorphicComponent<
  'button',
  UnstyledButtonProps
>(
  forwardRef<HTMLButtonElement, UnstyledButtonProps>((props, ref) => (
    <StyledMantineUnstyledButton
      {...props}
      {...useWillowStyleProps(props)}
      ref={ref}
    />
  ))
)

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, PropsForDocuments>(
  () => <div />
)
