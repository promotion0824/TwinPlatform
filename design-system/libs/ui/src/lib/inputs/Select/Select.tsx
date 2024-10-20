import {
  Select as SelectMantine,
  SelectProps as SelectPropsMantine,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  CommonInputProps,
  getCommonInputProps,
  getInputPaddings,
} from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { SelectItem } from './types'

export interface SelectProps
  extends WillowStyleProps,
    Omit<
      SelectPropsMantine,
      | keyof WillowStyleProps
      | 'prefix'
      | 'leftSection'
      | 'rightSection'
      | 'withAsterisk'
      | 'withCheckIcon'
    >,
    BaseProps {}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps
  extends Omit<CommonInputProps<string | null>, 'onChange'> {
  onChange?: (value: string | null, option: SelectItem) => void
  /**
   * acceptable type could be:
   * `string[]` |
   * `SelectItem[]` |
   * `{ group: string;
   * items: (SelectItem | string)[] }`
   */
  data?: SelectPropsMantine['data']
  /** @default false */
  placeholder?: SelectPropsMantine['placeholder']
  /** Adds icon on the left side of input */
  prefix?: SelectPropsMantine['leftSection']
  /** Adds icon on the right side of input */
  suffix?: SelectPropsMantine['rightSection']

  /**
   * Determines whether it should be possible to deselect value by clicking on the selected option.
   * @default false
   */
  allowDeselect?: SelectPropsMantine['allowDeselect']
  filter?: SelectPropsMantine['filter']
  initiallyOpened?: SelectPropsMantine['defaultDropdownOpened']
  limit?: SelectPropsMantine['limit']
  maxDropdownHeight?: SelectPropsMantine['maxDropdownHeight']
  nothingFound?: SelectPropsMantine['nothingFoundMessage']
  onDropdownOpen?: SelectPropsMantine['onDropdownOpen']
  onDropdownClose?: SelectPropsMantine['onDropdownClose']
  onSearchChange?: SelectPropsMantine['onSearchChange']
  searchable?: SelectPropsMantine['searchable']
  searchValue?: SelectPropsMantine['searchValue']
}

const StyledSelect = styled(SelectMantine)<Pick<SelectProps, 'readOnly'>>(
  ({ leftSection, readOnly, theme }) => css`
    .mantine-Select-input {
      padding: ${getInputPaddings({
        leftSection,
        rightSection: true /* always has right section */,
      })};

      /* .mantine-Select-input without searchable always has readonly attribute,
        so cannot apply style via :read-only CSS pseudo-class selector */
      ${readOnly &&
      css`
        background-color: ${theme.color.neutral.bg.accent.default};
      `}
    }
  `
)

/**
 * `Select` is a component built on top of `Combobox` component. It's a single select component.
 *
 * @see TODO: add link to storybook
 */
export const Select = forwardRef<HTMLInputElement, SelectProps>(
  (
    {
      allowDeselect = false,
      initiallyOpened,
      nothingFound,
      prefix,
      suffix,
      required,
      labelWidth,
      onChange,
      ...restProps
    },
    ref
  ) => {
    return (
      <StyledSelect
        allowDeselect={allowDeselect}
        withCheckIcon={false} // can enable this if desired in the future, but need restyle the icon
        onChange={onChange}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
        defaultDropdownOpened={initiallyOpened}
        nothingFoundMessage={nothingFound}
        leftSection={prefix}
        rightSection={suffix}
        required={required}
        withAsterisk={required}
        ref={ref}
        size="xs" /* this impacts prefix and suffix space */
      />
    )
  }
)

export default Select

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)
export const SelectItemDiv = forwardRef<HTMLDivElement, SelectItem>(() => (
  <div />
))
