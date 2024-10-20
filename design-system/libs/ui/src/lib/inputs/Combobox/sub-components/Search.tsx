import {
  ComboboxSearch as MantineComboboxSearch,
  ComboboxSearchProps as MantineComboboxSearchProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface SearchProps
  extends WillowStyleProps,
    Omit<MantineComboboxSearchProps, keyof WillowStyleProps> {
  /** Determines whether the search input should have `aria-` attribute, `true` by default */
  withAriaAttributes?: boolean

  /** Determines whether the search input should handle keyboard navigation, `true` by default */
  withKeyboardNavigation?: boolean
}

/**
 * `Combobox.Search` is a styled input component which can be used
 * inside `Combobox`.
 */
export const Search = forwardRef<HTMLInputElement, SearchProps>(
  (props, ref) => (
    <MantineComboboxSearch
      {...props}
      {...useWillowStyleProps(props)}
      ref={ref}
      size="xs"
    />
  )
)
