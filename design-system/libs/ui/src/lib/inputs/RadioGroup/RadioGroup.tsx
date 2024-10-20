import { RadioGroupProps as MantineRadioGroupProps, Radio } from '@mantine/core'
import { HTMLAttributes, ReactNode, forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  CommonInputProps,
  getCommonInputProps,
  renderChildrenWithProps,
} from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps
  extends Omit<CommonInputProps<string | null>, 'onChange'> {
  /** \<Radio /> components. */
  children: ReactNode

  /**
   * Display the radio buttons horizontally inline.
   * @default false
   */
  inline?: boolean
  onChange?: (value: string) => void
}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

export interface RadioGroupProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineRadioGroupProps, keyof WillowStyleProps | 'style'>,
    Omit<
      HTMLAttributes<HTMLDivElement>,
      'children' | 'defaultValue' | 'onChange'
    > {}

export const RadioGroup = forwardRef<HTMLDivElement, RadioGroupProps>(
  (
    { children, labelWidth, error, inline = false, onChange, ...restProps },
    ref
  ) => {
    return (
      <StyledRadioGroup
        ref={ref}
        onChange={onChange}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps<string | null>({
          ...restProps,
          error,
          labelWidth,
        })}
      >
        <RadioContainer $inline={inline} className="mantine-InputWrapper-root">
          {renderChildrenWithProps(children, { error: Boolean(error) })}
        </RadioContainer>
      </StyledRadioGroup>
    )
  }
)

const RadioContainer = styled.div<{
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

const StyledRadioGroup = styled(Radio.Group)(({ theme }) => ({
  // Those styles are migrated when upgrade to Mantine v7,
  // they might be able to be replace by the input styles GlobalStyles.
  '.mantine-RadioGroup-error': {
    ...theme.font.body.sm.regular,
    color: theme.color.intent.negative.fg.default,
  },

  '.mantine-RadioGroup-label': {
    '.-RadioGroup-required': {
      color: theme.color.intent.negative.fg.default,
    },
  },

  '.mantine-RadioGroup-root': {
    display: 'flex',
    flexDirection: 'column',
    gap: theme.spacing.s8,

    'input:disabled': {
      backgroundColor: 'transparent',
    },
  },
}))
