import {
  TextInput as MantineTextInput,
  TextInputProps as MantineTextInputProps,
} from '@mantine/core'
import {
  ChangeEventHandler,
  MutableRefObject,
  forwardRef,
  useImperativeHandle,
  useRef,
  useState,
} from 'react'
import styled, { css } from 'styled-components'
import { IconButton } from '../../buttons/Button'
import { Icon } from '../../misc/Icon'
import {
  CommonInputProps,
  getCommonInputProps,
  getInputPaddings,
} from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface TextInputProps
  extends WillowStyleProps,
    Omit<
      MantineTextInputProps,
      | keyof WillowStyleProps
      | 'leftSection'
      | 'prefix'
      | 'rightSectionProps'
      | 'rightSection'
    >,
    BaseProps {}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps
  extends Omit<
    CommonInputProps<string | number | readonly string[]>,
    'onChange'
  > {
  placeholder?: string
  onChange?: ChangeEventHandler<HTMLInputElement>

  /** Adds icon on the left side of input */
  // Mantine do not support iconProps
  prefix?: MantineTextInputProps['leftSection']
  /** Adds icon on the right side of input */
  suffix?: MantineTextInputProps['rightSection']
  /** Props spread to suffix div element */
  suffixProps?: MantineTextInputProps['rightSectionProps']

  /**
   * Displays a clear button on the right when text has been entered.
   * Overrides any suffix if one is being displayed.
   * @default false
   */
  clearable?: boolean
}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/** `TextInput` combines a text input, an optional label and an optional validation message. */
export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(
  (
    {
      onChange,
      labelWidth,
      prefix,
      suffix,
      required = false,
      readOnly = false,
      clearable = false,
      disabled = false,
      suffixProps,
      ...restProps
    },
    externalRef
  ) => {
    // This is used as an internal reference to the current value, accessible
    // in both controlled and uncontrolled components.
    const [currentValue, setCurrentValue] = useState(
      restProps.defaultValue || restProps.value || ''
    )

    const ref = useRef<HTMLInputElement>(null)

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    useImperativeHandle(externalRef, () => ref.current!, [ref])

    return (
      <StyledTextInput
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
        leftSection={prefix}
        onChange={(event) => {
          setCurrentValue(event.target.value)
          onChange?.(event)
        }}
        rightSection={
          clearable && currentValue.toString()?.length ? (
            <ClearButton textInputRef={ref} />
          ) : (
            suffix
          )
        }
        rightSectionProps={suffixProps}
        required={required}
        readOnly={readOnly}
        disabled={disabled}
        ref={ref}
        size="xs" /* this impacts prefix and suffix space */
      />
    )
  }
)

const StyledIconButton = styled(IconButton)(({ theme }) => ({
  width: theme.spacing.s20,
  height: theme.spacing.s20,
  padding: 0,
}))

function ClearButton({
  textInputRef,
}: {
  textInputRef: MutableRefObject<HTMLInputElement | null>
}) {
  return (
    <StyledIconButton
      background="transparent"
      kind="secondary"
      onClick={() => {
        if (textInputRef.current) {
          // Calls the native setter for the input to ensure
          // capability with controlled components.
          Object.getOwnPropertyDescriptor(
            window.HTMLInputElement.prototype,
            'value'
          )?.set?.call(textInputRef.current, '')

          textInputRef.current.dispatchEvent(
            new Event('change', { bubbles: true })
          )
        }
      }}
    >
      <Icon icon="close" />
    </StyledIconButton>
  )
}

const StyledTextInput = styled(MantineTextInput)(
  ({ leftSection, rightSection }) => css`
    .mantine-Input-input {
      padding: ${getInputPaddings({ leftSection, rightSection })};
    }
  `
)
