import { forwardRef } from 'react'
import styled from 'styled-components'

import { Icon } from '../../misc/Icon'
import {
  TextInput,
  BaseProps as TextInputBaseProps,
  TextInputProps,
} from '../TextInput/TextInput'

export interface SearchInputProps
  extends Omit<TextInputProps, keyof TextInputBaseProps>,
    BaseProps {}

interface BaseProps
  extends Omit<
    TextInputBaseProps,
    'clearable' | 'prefix' | 'suffix' | 'suffixProps'
  > {}

const SearchIcon = styled(Icon)<{ $disabled: boolean }>(
  ({ $disabled, theme }) => ({
    color: $disabled
      ? theme.color.state.disabled.fg
      : theme.color.neutral.fg.muted,
  })
)

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/** `SearchInput` is a wrapped `TextInput` with a restricted api and some pre-applied props, used for wiring up search functionality.*/
export const SearchInput = forwardRef<HTMLInputElement, SearchInputProps>(
  ({ disabled = false, ...restProps }, ref) => {
    return (
      <TextInput
        {...restProps}
        prefix={<SearchIcon $disabled={disabled} icon="search" />}
        clearable
        disabled={disabled}
        ref={ref}
      />
    )
  }
)
