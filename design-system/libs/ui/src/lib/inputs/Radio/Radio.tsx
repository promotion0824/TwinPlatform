import {
  Radio as MantineRadio,
  type RadioProps as MantineRadioProps,
} from '@mantine/core'
import { ChangeEvent, forwardRef, isValidElement } from 'react'
import styled, { css } from 'styled-components'
import { CommonInputProps, rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

const StyledRadio = styled(MantineRadio)(
  ({ theme, error }) => css`
    .mantine-Radio-icon {
      color: ${theme.color.intent.primary.bg.bold.default};
      transform: unset;
      transition: unset;
      width: ${theme.spacing.s8};
      height: ${theme.spacing.s8};
      top: ${theme.spacing.s4}; /* 16px/2 - 8px/2 */
      left: ${theme.spacing.s4};
    }

    .mantine-Radio-label {
      ${theme.font.body.md.regular};
      line-height: ${rem(16)};
      color: ${theme.color.neutral.fg.default};
      padding-left: ${theme.spacing.s8};

      &[data-disabled='true'] {
        color: ${theme.color.state.disabled.fg};
      }
    }

    .mantine-Radio-labelWrapper {
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .mantine-Radio-radio {
      width: ${theme.spacing.s16};
      height: ${theme.spacing.s16};
      background-color: transparent;
      border-color: ${theme.color.neutral.fg.muted};
      outline-offset: 0;

      &[aria-invalid='true'] {
        outline: 1px solid ${theme.color.intent.negative.border.default};
      }

      &:checked {
        background-color: transparent;
        border-color: ${theme.color.intent.primary.bg.bold.default};
      }

      &:disabled {
        border-color: ${theme.color.state.disabled.fg};

        + svg {
          color: ${theme.color.state.disabled.fg};
        }
      }

      &:focus-visible {
        outline: 1px solid ${theme.color.state.focus.border};
      }
    }

    .mantine-Radio-inner {
      width: ${theme.spacing.s16};
      height: ${theme.spacing.s16};

      display: flex;
      align-items: center;
      justify-content: center;
    }

    /* Mantine will add an empty error element as long as error is true,
      which has a margin top of 8px and will impact the height of Radio.
      Which will be a problem when we use an invalid style without error message,
      for example in a RadioGroup which has error=true for Radios. */

    .mantine-Radio-error {
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

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps
  extends Pick<
    CommonInputProps<string | number | readonly string[]>,
    'value' | 'label' | 'disabled' | 'readOnly'
  > {
  defaultChecked?: boolean
  /** Whether the radio button is checked. */
  checked?: boolean
  onChange?: (event: ChangeEvent<HTMLInputElement>) => void
  error?: boolean
}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

export interface RadioProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineRadioProps, keyof WillowStyleProps | 'error'> {}

export const Radio = forwardRef<HTMLInputElement, RadioProps>(
  ({ error, ...props }, ref) => {
    return (
      <StyledRadio
        aria-invalid={Boolean(error)}
        ref={ref}
        {...props}
        {...useWillowStyleProps(props)}
      />
    )
  }
)
