import {
  Textarea as MantineTextarea,
  TextareaProps as MantineTextareaProps,
} from '@mantine/core'
import { ChangeEventHandler, forwardRef } from 'react'
import styled from 'styled-components'
import { BaseProps as TextInputBaseProps } from '../TextInput/TextInput'
import { getCommonInputProps } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface TextareaProps
  extends WillowStyleProps,
    Omit<MantineTextareaProps, keyof WillowStyleProps>,
    BaseProps {}

interface BaseProps
  extends Omit<TextInputBaseProps, 'onChange' | 'clearable' | 'prefix'> {
  onChange?: ChangeEventHandler<HTMLTextAreaElement>

  /**
   * Determines whether the textarea height should grow with its content.
   * Constrained by minRows and maxRows
   * @default true
   */
  autosize?: MantineTextareaProps['autosize']
  /** Ignored if `autosize` is disabled. */
  maxRows?: MantineTextareaProps['maxRows']
  /**
   * Ignored if `autosize` is disabled.
   * @default 5
   */
  minRows?: MantineTextareaProps['minRows']

  /** Defines the maximum length of value allowed. */
  maxLength?: number
}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `Textarea` combines a textarea, an optional label and
 * an optional validation message.
 */
export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  (
    {
      required = false,
      readOnly = false,
      disabled = false,
      autosize = true,
      minRows = 5,
      labelWidth,
      onChange,
      ...restProps
    },
    ref
  ) => {
    return (
      <StyledTextarea
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
        withAsterisk={required}
        required={required}
        readOnly={readOnly}
        disabled={disabled}
        autosize={autosize}
        minRows={minRows}
        onChange={onChange}
        ref={ref}
      />
    )
  }
)

const StyledTextarea = styled(MantineTextarea)`
  &,
  * {
    box-sizing: border-box;
  }
`
