import {
  TagsInput as MantineTagsInput,
  TagsInputProps as MantineTagsInputProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled from 'styled-components'

import { CloseButton } from '../../common'
import { CommonInputProps, getCommonInputProps, rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

interface BaseProps extends CommonInputProps<string[]>, WillowStyleProps {
  /** Data displayed in the dropdown */
  data?: MantineTagsInputProps['data']

  /** Default search value */
  defaultSearchValue?: MantineTagsInputProps['defaultSearchValue']
  searchValue?: MantineTagsInputProps['searchValue']
  onSearchChange?: MantineTagsInputProps['onSearchChange']

  /** Called when tag is removed */
  onRemove?: MantineTagsInputProps['onRemove']

  /**
   * Determines whether the clear button should be displayed in the right section
   * when the component has value
   *
   * @default false
   */
  clearable?: MantineTagsInputProps['clearable']
  /** Props passed down to the clear button */
  clearButtonProps?: MantineTagsInputProps['clearButtonProps']
  /** Called whe the clear button is clicked */
  onClear?: () => void

  /** Maximum number of tags, `Infinity` by default */
  maxTags?: MantineTagsInputProps['maxTags']
  /** Maximum number of options displayed at a time, `Infinity` by default */
  limit?: MantineTagsInputProps['limit']

  /** Characters that should trigger tags split, [','] by default */
  splitChars?: MantineTagsInputProps['splitChars']
  /** A function to render content of the option, replaces the default content of the option */
  renderOption?: MantineTagsInputProps['renderOption']
  /**
   * `max-height` of the dropdown, only applicable when `withScrollArea` prop
   * is `true`, `250` by default
   */
  maxDropdownHeight?: MantineTagsInputProps['maxDropdownHeight']
  /** Props passed down to `Combobox` component */
  comboboxProps?: MantineTagsInputProps['comboboxProps']
  /** Uncontrolled dropdown initial opened state */
  defaultDropdownOpened?: MantineTagsInputProps['defaultDropdownOpened']
  /** Controlled dropdown opened state */
  dropdownOpened?: MantineTagsInputProps['dropdownOpened']
}

export interface TagsInputProps
  extends BaseProps,
    Omit<MantineTagsInputProps, keyof WillowStyleProps> {}

/**
 * `TagsInput` is a component built on top of `Combobox` component. It's a multi select allows entering custom values.
 */
export const TagsInput = forwardRef<HTMLInputElement, TagsInputProps>(
  (
    {
      clearable = false,
      clearButtonProps,
      labelWidth,

      ...restProps
    },
    ref
  ) => {
    return (
      <StyledTagsInput
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
        clearable={clearable}
        clearButtonProps={{
          // @ts-expect-error // it works
          component: CloseButton,
          ...clearButtonProps,
        }}
        ref={ref}
      />
    )
  }
)

const StyledTagsInput = styled(MantineTagsInput)`
  .mantine-TagsInput-wrapper {
    .mantine-TagsInput-input {
      min-height: ${rem(28)};
      height: auto;

      .mantine-TagsInput-inputField {
        height: auto;
      }
    }
  }
`

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)
