import { Children, forwardRef } from 'react'
import styled from 'styled-components'

import { Field, FieldProps } from '../Field'
import { renderChildrenWithProps } from '../../utils'

export interface InputGroupProps extends FieldProps {
  /** Should be more than one child in a group. */
  children: React.ReactNode
  /** Disable all the inputs in the same group. */
  disabled?: boolean
}
export const inputGroupChildrenError =
  'InputGroup requires more than one child to group with.'

/**
 * `InputGroup` is a filed wrapper that used to combine more than one input
 * components as a group.
 */
export const InputGroup = forwardRef<HTMLDivElement, InputGroupProps>(
  ({ disabled, readOnly, children, ...restProps }, ref) => {
    if (Children.count(children) === 1) {
      throw new Error(inputGroupChildrenError)
    }

    return (
      <Field {...restProps} ref={ref}>
        <Wrapper>
          {renderChildrenWithProps(children, (child, index) => {
            return {
              className: `${child.props.className || ''} ${getClassNameByIndex(
                index,
                index ===
                  Children.count(children) - 1 /* whether is last child */
              )}`,
              ...(disabled ? { disabled: true } : {}),
            }
          })}
        </Wrapper>
      </Field>
    )
  }
)

const FIRST = 'first-in-input-group'
const MIDDLE = 'middle-in-input-group'
const LAST = 'last-in-input-group'

const getClassNameByIndex = (index: number, last = false) =>
  last ? LAST : index === 0 ? FIRST : MIDDLE

const Wrapper = styled.div`
  display: flex;
  flex-direction: row;
  width: 100%;

  .${FIRST} {
    margin-right: -1px;

    input,
    &.mantine-Button-root {
      border-bottom-right-radius: unset;
      border-top-right-radius: unset;
    }
  }

  .${MIDDLE} {
    margin-right: -1px;

    input,
    &.mantine-Button-root {
      border-radius: unset;
    }
  }
  button.${MIDDLE} {
    border-radius: unset;
  }

  .${LAST} {
    input,
    &.mantine-Button-root {
      border-bottom-left-radius: unset;
      border-top-left-radius: unset;
    }
  }

  .${FIRST}, .${MIDDLE}, .${LAST} {
    &:focus-visible,
    &:focus-within,
    &:focus,
    &:active {
      z-index: 1;
    }
  }
`
