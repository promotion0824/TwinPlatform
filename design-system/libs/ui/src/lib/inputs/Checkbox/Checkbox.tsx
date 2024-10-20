import {
  Checkbox as MantineCheckbox,
  CheckboxProps as MantineCheckboxProps,
} from '@mantine/core'
import { CSSProperties, forwardRef, isValidElement } from 'react'
import styled, { css } from 'styled-components'
import { CommonInputProps, rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface CheckboxProps
  extends WillowStyleProps,
    Omit<MantineCheckboxProps, keyof WillowStyleProps>,
    BaseProps {}

export interface BaseProps
  extends Omit<
    CommonInputProps<string | number | readonly string[]>,
    'onChange' | 'defaultValue' | 'description' | 'descriptionProps'
  > {
  defaultChecked?: boolean
  checked?: boolean
  /** Indeterminate state of checkbox, if set, `checked` prop is ignored */
  indeterminate?: boolean
  /** Checkbox label position */
  labelPosition?: 'left' | 'right'
  /** Align the switch and label in the container. */
  justify?: CSSProperties['justifyContent']
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void
}

export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 *
 * A `Checkbox` is a customizable input control that allows users to toggle between two options - checked or unchecked.
 *
 * The `Checkbox` component combines a checkbox input, label, and validation message.
 */

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ error, ...restProps }, ref) => {
    return (
      <StyledCheckbox
        aria-invalid={error ? true : undefined}
        radius="xs"
        ref={ref}
        error={error}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      />
    )
  }
)

const StyledCheckbox = styled(MantineCheckbox)<{
  justify?: BaseProps['justify']
}>(
  ({ theme, indeterminate, justify = 'flex-start', error }) => css`
    .mantine-Checkbox-body {
      justify-content: ${justify};
      align-items: center;
    }

    .mantine-Checkbox-input {
      width: ${theme.spacing.s16};
      height: ${theme.spacing.s16};
    }

    .mantine-Checkbox-input {
      background-color: ${theme.color.intent.secondary.bg.subtle.default};
      border: 1px solid ${theme.color.neutral.border.default};
      outline-offset: 0;

      ${indeterminate &&
      css`
        background-color: ${theme.color.neutral.fg.muted};
        border-color: ${theme.color.neutral.fg.muted};
      `}

      &:checked {
        background-color: ${theme.color.intent.primary.bg.bold.default};
        border-color: ${theme.color.intent.primary.bg.bold.default};
      }

      &[aria-invalid='true'] {
        outline: ${rem(1)} solid ${theme.color.intent.negative.border.default};
      }

      &:focus-visible {
        outline: ${rem(1)} solid ${theme.color.state.focus.border};
      }

      &:disabled {
        background-color: ${theme.color.state.disabled.fg};
        border-color: ${theme.color.state.disabled.border};
      }
    }

    .mantine-Checkbox-label {
      ${theme.font.body.md.regular};
      line-height: ${rem(16)};
      color: ${theme.color.neutral.fg.default};
      padding-left: ${rem(8)};

      &[data-disabled] {
        color: ${theme.color.state.disabled.fg};
      }
    }

    .mantine-Checkbox-description {
      margin: ${rem(8)} 0 0 0;
      padding: 0 0 0 ${rem(8)};
    }

    .mantine-Checkbox-error {
      ${theme.font.body.md.regular};
      line-height: ${rem(16)};
      color: ${theme.color.intent.negative.fg.default};
      padding-left: ${rem(8)};
    }

    .mantine-Checkbox-icon {
      color: ${theme.color.neutral.bg.panel.default} !important;
    }

    .mantine-Checkbox-inner {
      width: ${rem(16)};
      height: ${rem(16)};
    }

    /* Mantine will add an empty error element as long as error is true,
      which has a margin top of 8px and will impact the height of checkbox.
      Which will be a problem when we use an invalid style without error message,
      for example in a CheckboxGroup which has error=true for Checkboxes. */

    .mantine-Checkbox-error {
      /* Make the style more like our design pattern. Even though we don't have
       design or requirement for such case yet. */
      margin-top: ${theme.spacing.s8};

      ${Boolean(error) &&
      !(
        isValidElement(error) ||
        typeof error === 'string' ||
        typeof error === 'number'
      ) &&
      css`
        margin-top: 0; /* remove margin top from the empty error element */
      `}
    }
  `
)
