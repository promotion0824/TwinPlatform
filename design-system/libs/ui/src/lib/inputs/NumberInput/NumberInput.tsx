import {
  NumberInput as MantineNumberInput,
  NumberInputProps as MantineNumberInputProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled from 'styled-components'
import {
  CommonInputProps,
  getCommonInputProps,
  leftSectionSize,
  rem,
  rightSectionSize,
} from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

const StyledNumberInput = styled(MantineNumberInput)(
  ({ leftSection, rightSection, theme }) => {
    const leftPadding = leftSection ? leftSectionSize : theme.spacing.s8
    const rightPadding = rightSection ? rightSectionSize : theme.spacing.s8

    return {
      '.mantine-NumberInput-control': {
        backgroundColor: theme.color.neutral.bg.panel.default,
        borderColor: theme.color.neutral.border.default,
        color: theme.color.neutral.fg.default,
        height: rem(13),

        '&[data-direction="up"]': {
          borderBottom: `0.05px solid ${theme.color.neutral.border.default}`,
        },
        '&[data-direction="down"]': {
          borderTop: `0.05px solid ${theme.color.neutral.border.default}`,
        },

        svg: {
          // Important needs to be used here as these SVGs have inline styles applied to them
          // that need to be overwritten.
          width: `${theme.spacing.s8} !important`,
        },

        '&:hover': {
          backgroundColor: theme.color.neutral.bg.panel.hovered,
        },
      },

      '.mantine-NumberInput-controls': {
        height: rem(26),
      },

      '.mantine-NumberInput-input': {
        padding: `${theme.spacing.s4} ${rightPadding} ${theme.spacing.s4} ${leftPadding}`,
      },

      '.mantine-NumberInput-section': {
        color: theme.color.neutral.fg.subtle,

        '&[data-position="right"]': {
          // only apply when we have customized suffix passed in,
          // which will override the default up and down buttons.
          ...(rightSection && { paddingRight: theme.spacing.s8 }),
        },
      },

      '.mantine-NumberInput-wrapper': {
        '&[data-error="true"]': {
          '.mantine-NumberInput-control': {
            backgroundColor: theme.color.intent.negative.bg.subtle.default,
            borderColor: theme.color.intent.negative.border.default,
          },
        },

        '&[data-disabled="true"]': {
          '.mantine-NumberInput-control,  .mantine-NumberInput-section': {
            backgroundColor: theme.color.state.disabled.bg,
            color: theme.color.state.disabled.fg,
            opacity: 1,
          },
        },
      },
    }
  }
)

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export interface BaseProps extends CommonInputProps<number | string> {
  /**
   * Determines whether decimal values are allowed.
   * @default true
   */
  allowDecimal?: MantineNumberInputProps['allowDecimal']
  /**
   * Determines whether negative values are allowed.
   * @default true
   */
  allowNegative?: MantineNumberInputProps['allowNegative']
  /**
   * Controls how value is clamped.
   * `strict`: user is not allowed to enter values that are not in [min, max] range
   * `blur`: user is allowed to enter any values, but the value is clamped when the input loses focus
   * `none`: lifts all restrictions, [min, max] range is applied only for controls and up/down keys
   * @default blur
   */
  clampBehavior?: MantineNumberInputProps['clampBehavior']
  /** Limits the number of digits that can be entered after the decimal point. */
  decimalScale?: MantineNumberInputProps['decimalScale']
  /**
   * If set, the provided `decimalScale` is always used, with zeroes added as necessary.
   * @default false
   */
  fixedDecimalScale?: MantineNumberInputProps['fixedDecimalScale']
  /** Maximum possible value. */
  max?: MantineNumberInputProps['max']
  /** Minimum possible value. */
  min?: MantineNumberInputProps['min']
  /** Content rendered to the left of the input. */
  prefix?: MantineNumberInputProps['leftSection']
  /** Content rendered to the right of the input. */
  suffix?: MantineNumberInputProps['rightSection']
  /** Prefix added before the input value. */
  textPrefix?: MantineNumberInputProps['prefix']
  /** Suffix added after the input value. */
  textSuffix?: MantineNumberInputProps['suffix']
}

export interface NumberInputProps
  extends WillowStyleProps,
    BaseProps,
    Omit<
      MantineNumberInputProps,
      keyof WillowStyleProps | 'prefix' | 'suffix'
    > {}

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `NumberInput` is an input that only allows numbers to be entered.
 */
export const NumberInput = forwardRef<HTMLDivElement, NumberInputProps>(
  (
    { labelWidth, prefix, suffix, textPrefix, textSuffix, ...restProps },
    ref
  ) => {
    return (
      <StyledNumberInput
        leftSection={prefix}
        prefix={textPrefix}
        ref={ref}
        rightSection={suffix}
        suffix={textSuffix}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
      />
    )
  }
)
