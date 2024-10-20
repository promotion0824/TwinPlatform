import {
  Combobox as MantineCombobox,
  ComboboxTargetProps as MantineComboboxTargetProps,
} from '@mantine/core'

// those sub components do not have any tangible UI
const Target = MantineCombobox.Target
export { Target, type MantineComboboxTargetProps as TargetProps }

const EventsTarget = MantineCombobox.EventsTarget
export { EventsTarget, type MantineComboboxTargetProps as EventsTargetProps }

const DropdownTarget = MantineCombobox.DropdownTarget
export {
  DropdownTarget,
  type MantineComboboxTargetProps as DropdownTargetProps,
}

export { Chevron, type ChevronProps } from './Chevron'
export { ClearButton, type ClearButtonProps } from './ClearButton'
export { Dropdown, type DropdownProps } from './Dropdown'
export { Empty, type EmptyProps } from './Empty'
export { Footer, type FooterProps } from './Footer'
export { Group, type GroupProps } from './Group'
export { Header, type HeaderProps } from './Header'
export { InputBase, type InputBaseProps } from './InputBase'
export {
  InputPlaceholder,
  type InputPlaceholderProps,
} from './InputPlaceholder'
export { Option, type OptionProps } from './Option'
export { Options, type OptionsProps } from './Options'
export { Search, type SearchProps } from './Search'
// Combobox.HiddenInput is omitted because it is not used in any customize example
