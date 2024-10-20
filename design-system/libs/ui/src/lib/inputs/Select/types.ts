import { ComboboxItem } from '@mantine/core'

export {
  ComboboxData as SelectData,
  OptionsFilter as SelectOptionsFilter,
} from '@mantine/core'

// redefined just for storybook props documentation
export interface SelectItem extends ComboboxItem {
  value: string
  label: string
  /** @default false */
  disabled?: boolean
}
