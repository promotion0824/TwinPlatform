import {
  Combobox as MantineCombobox,
  ComboboxProps as MantineComboboxProps,
} from '@mantine/core'

import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import {
  Chevron,
  ClearButton,
  Dropdown,
  DropdownTarget,
  Empty,
  EventsTarget,
  Footer,
  Group,
  Header,
  InputBase,
  InputPlaceholder,
  Option,
  Options,
  Search,
  Target,
  OptionProps,
} from './sub-components'
import { ComboboxStore } from './useCombobox'

export interface ComboboxProps
  extends WillowStyleProps,
    Omit<MantineComboboxProps, keyof WillowStyleProps | 'onOptionSubmit'> {
  /** Combobox content */
  children?: React.ReactNode

  /** Combobox store, can be used to control combobox state */
  store?: ComboboxStore

  /** Called when item is selected with `Enter` key or by clicking it */
  onOptionSubmit?: (value: string, optionProps: OptionProps) => void

  /** Determines whether selection should be reset when option is hovered, `false` by default */
  resetSelectionOnOptionHover?: boolean

  /** Determines whether Combobox value can be changed */
  readOnly?: boolean
}

/**
 * `Combobox` is the base component which could be used for create
 * custom select, autocomplete or multiselect inputs.
 */
// Mantine v7.11.2 Combobox do not support ref
export const Combobox = ({ onOptionSubmit, ...restProps }: ComboboxProps) => (
  <MantineCombobox
    {...restProps}
    {...useWillowStyleProps(restProps)}
    onOptionSubmit={onOptionSubmit as MantineComboboxProps['onOptionSubmit']}
  />
)

Combobox.Target = Target
Combobox.EventsTarget = EventsTarget
Combobox.DropdownTarget = DropdownTarget
Combobox.InputBase = InputBase
Combobox.InputPlaceholder = InputPlaceholder
Combobox.Options = Options
Combobox.Option = Option
Combobox.Dropdown = Dropdown
Combobox.Search = Search
Combobox.Group = Group
Combobox.Footer = Footer
Combobox.Header = Header
Combobox.Empty = Empty
Combobox.Chevron = Chevron
Combobox.ClearButton = ClearButton
