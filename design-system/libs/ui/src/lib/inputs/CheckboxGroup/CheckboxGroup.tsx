import {
  Checkbox as MantineCheckbox,
  CheckboxGroupProps as MantineCheckboxGroupProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  CommonInputProps,
  getCommonInputProps,
  renderChildrenWithProps,
} from '../../utils'
import {
  useWillowStyleProps,
  WillowStyleProps,
} from '../../utils/willowStyleProps'
import { Checkbox } from '../Checkbox'

// The following types enable the component to determine whether `value` and `onChange`
// should be of type `string[]` or `number[]`, based on the `type` prop. This avoids using
// a union type (`string[] | number[]`) for both `value` and `onChange`.
type ValueType = 'string' | 'number'
type Value<T extends ValueType> = T extends 'string' ? string : number
type CheckboxGroupValue<T extends ValueType> = Array<Value<T>>
type CheckboxGroupData<T extends ValueType> = Array<{
  label: string
  value: Value<T>
  disabled?: boolean
}>
type CheckboxGroupOnChange<T extends ValueType> = (
  value: Array<Value<T>>
) => void

export interface CheckboxGroupProps<Type extends ValueType = 'string'>
  extends WillowStyleProps,
    Omit<
      MantineCheckboxGroupProps,
      | keyof WillowStyleProps
      | 'defaultValue'
      | 'value'
      | 'onChange'
      | 'children'
    >,
    BaseProps<Type> {}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps<T extends ValueType = 'string'>
  extends Omit<
    CommonInputProps<CheckboxGroupValue<T>>,
    'value' | 'defaultValue' | 'onChange'
  > {
  /** <Checkbox /> components */
  children?: React.ReactNode

  /**
   * Display checkboxes inline.
   * @default false
   */
  inline?: boolean

  /**
   * Determine the Checkboxes' value type. Change to 'number' will support
   * Array<number> as value.
   * @default 'string'
   */
  type?: ValueType | T
  /** Value of input for controlled components. */
  value?: CheckboxGroupValue<T>
  /** Initial value for uncontrolled components. Overridden by value prop. */
  defaultValue?: CheckboxGroupValue<T>
  /** Called when value changes. */
  onChange?: CheckboxGroupOnChange<T>
  /**
   * Data used to generate Checkbox options. Can be used
   * instead of passing <Checkbox /> components as children.
   */
  data?: CheckboxGroupValue<T> | CheckboxGroupData<T>
}
export const BasePropsDiv = forwardRef<
  HTMLDivElement,
  Omit<BaseProps, 'value' | 'defaultValue' | 'onChange' | 'data'> & {
    // for documentation purposes
    value?: string[] | number[]
    defaultValue?: string[] | number[]
    onChange?: (value: string[] | number[]) => void
    data?:
      | string[]
      | number[]
      | Array<{ label: string; value: string; disabled?: boolean }>
      | Array<{ label: string; value: number; disabled?: boolean }>
  }
>(() => <div />)

interface CheckboxGroupComponent
  extends React.ForwardRefExoticComponent<CheckboxGroupProps<ValueType>> {
  <Type extends ValueType = 'string'>(
    props: CheckboxGroupProps<Type>
  ): JSX.Element
}

/**
 * `CheckboxGroup` is a fieldset the allows a user to select multiple items
 * from a group of Checkboxes.
 */
export const CheckboxGroup = forwardRef<HTMLDivElement, CheckboxGroupProps>(
  (
    {
      // Since React.forwardRef doesn't support generic types, the resulting
      // types are not accurately reflecting the expected types, particularly
      // when it comes to handling number types. But this implementation allows
      // our consumers to use a single CheckboxGroup and seamlessly switch between
      // number and string types. This approach aligns with our API preferences,
      // ensuring consistency across the board while providing consumers with the
      // correct type definitions on their end. Thus we adopt this implementation.
      defaultValue,
      value,
      onChange,
      data,
      type = 'string',

      inline = false,
      labelWidth,
      error,
      children,
      ...restProps
    },
    ref
  ) => {
    const isNumberValues = type === 'number'

    return (
      <MantineCheckbox.Group
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, error, labelWidth })}
        ref={ref}
        value={convertToStringArray(value)}
        defaultValue={convertToStringArray(defaultValue)}
        onChange={(values) => {
          onChange?.(
            // @ts-expect-error // As mentioned above, the type for onChange is not accurate
            isNumberValues ? convertToNumberArray(values) ?? [] : values
          )
        }}
      >
        <CheckboxContainer
          $inline={inline}
          className="mantine-InputWrapper-root"
        >
          {data?.length
            ? data?.map((item) => {
                if (typeof item === 'object') {
                  const value = String(item.value)

                  return (
                    <Checkbox
                      key={value}
                      value={value}
                      disabled={item.disabled}
                      label={item.label}
                      error={Boolean(error)}
                    />
                  )
                }

                const value = String(item)
                return (
                  <Checkbox
                    key={value}
                    value={value}
                    label={value}
                    error={Boolean(error)}
                  />
                )
              })
            : renderChildrenWithProps(children, (child) => ({
                error: Boolean(error),
                value: String(child?.props?.value),
              }))}
        </CheckboxContainer>
      </MantineCheckbox.Group>
    )
  }
) as CheckboxGroupComponent

const CheckboxContainer = styled.div<{
  $inline: boolean
}>(
  ({ $inline }) =>
    css`
      &.mantine-InputWrapper-root {
        ${$inline
          ? css`
              flex-direction: row;
              flex-wrap: wrap;
            `
          : css`
              flex-direction: column;
            `}
      }
    `
)

const convertToStringArray = (value?: string[] | number[]) =>
  value === undefined ? value : value.map((v) => String(v))

const convertToNumberArray = (value?: string[] | number[]) =>
  value === undefined ? value : value.map((v) => Number(v))
