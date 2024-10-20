import {
  Switch as MantineSwitch,
  SwitchProps as MantineSwitchProps,
} from '@mantine/core'
import { CSSProperties, forwardRef } from 'react'
import styled from 'styled-components'
import { CommonInputProps, rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps
  extends Omit<
    CommonInputProps<string | number | readonly string[]>,
    | 'defaultValue'
    | 'onChange'
    | 'required'
    | 'error'
    | 'errorProps'
    | 'description'
    | 'descriptionProps'
  > {
  /** Directly set the value of the switch. */
  checked?: boolean
  /** Position of the label next to the switch. */
  labelPosition?: MantineSwitchProps['labelPosition']
  /** Align the switch and label in the container. */
  justify?: CSSProperties['justifyContent']
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void
  /**
   * Only support boolean value at the moment. Reach out if you need
   * to display an error message.
   */
  error?: boolean
}

export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

export interface SwitchProps
  extends WillowStyleProps,
    Omit<
      MantineSwitchProps,
      | keyof WillowStyleProps
      | 'checked'
      | 'disabled'
      | 'error'
      | 'label'
      | 'labelPosition'
    >,
    BaseProps {}

const StyledSwitch = styled(MantineSwitch)<{ justify?: BaseProps['justify'] }>(
  ({ error, labelPosition, justify = 'flex-start', theme }) => ({
    alignItems: 'center',
    // Default state
    '.mantine-Switch-input+.mantine-Switch-track': {
      width: rem(36),
      minWidth: rem(36),
      backgroundColor: theme.color.neutral.bg.panel.default,
      borderColor: theme.color.neutral.border.default,
      cursor: 'pointer',
      outlineOffset: 0,

      ...(error && {
        outline: `1px solid ${theme.color.intent.negative.border.default}`,
      }),

      '.mantine-Switch-thumb': {
        backgroundColor: theme.color.neutral.fg.muted,
        border: 'none',
      },
    },

    // Checked
    '.mantine-Switch-input:checked+.mantine-Switch-track': {
      backgroundColor: theme.color.intent.primary.bg.bold.default,
      borderColor: theme.color.neutral.border.default,

      '.mantine-Switch-thumb': {
        backgroundColor: theme.color.neutral.fg.highlight,
      },
    },

    // Disabled
    '.mantine-Switch-input:disabled+.mantine-Switch-track': {
      cursor: 'not-allowed', // change to Mantine's cursor for disabled input

      backgroundColor: theme.color.neutral.bg.panel.default,
      borderColor: theme.color.state.disabled.border,

      '.mantine-Switch-thumb': {
        backgroundColor: theme.color.state.disabled.bg,
      },
    },

    // Disabled + checked
    '.mantine-Switch-input:checked:disabled+.mantine-Switch-track': {
      backgroundColor: theme.color.state.disabled.bg,

      '.mantine-Switch-thumb': {
        backgroundColor: theme.color.state.disabled.fg,
      },
    },

    // Label
    '.mantine-Switch-label': {
      color: theme.color.neutral.fg.default,
      ...theme.font.body.md.regular,
      paddingLeft: labelPosition !== 'left' ? rem(8) : 0,
      paddingRight: labelPosition === 'left' ? rem(8) : 0,

      '&[data-disabled]': {
        color: theme.color.state.disabled.fg,
      },
    },

    // Error message isn't used, and needs to be hidden so that the
    // element height doesn't change
    '.mantine-Switch-error': {
      display: 'none',
    },

    '.mantine-Switch-body': {
      display: 'flex',
      alignItems: 'start',
      justifyContent: justify,

      // Focused
      '.mantine-Switch-input[type="checkbox"]:focus-visible+.mantine-Switch-track':
        {
          outline: error
            ? `1px solid ${theme.color.intent.negative.border.default}`
            : `1px solid ${theme.color.state.focus.border}`,
          outlineOffset: 0,
        },
    },
  })
)

/**
 * `Switch` captures boolean input from the user
 */
export const Switch = forwardRef<HTMLInputElement, SwitchProps>(
  (props, ref) => {
    return <StyledSwitch ref={ref} {...props} {...useWillowStyleProps(props)} />
  }
)
