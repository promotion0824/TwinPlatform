import type { StoryObj } from '@storybook/react'

import { Combobox, useCombobox } from '.'
import { useState } from 'react'

const defaultStory = {
  component: Combobox,
  title: 'Combobox',
}

export default defaultStory

type Story = StoryObj<typeof Combobox>
const groceries = ['Apples', 'Bananas', 'Broccoli', 'Carrots', 'Chocolate']

export const ComboboxSimpleExample: Story = {
  render: () => {
    const combobox = useCombobox({
      opened: true,
      onDropdownClose: () => combobox.resetSelectedOption(),
    })
    const [value, setValue] = useState<string | undefined>()

    return (
      <Combobox
        store={combobox}
        onOptionSubmit={(val) => {
          setValue(val)
          combobox.closeDropdown()
        }}
      >
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={<Combobox.Chevron />}
            suffixPointerEvents="none"
            onClick={() => combobox.toggleDropdown()}
          >
            {value || (
              <Combobox.InputPlaceholder>Pick value</Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown>
          <Combobox.Header>Test Header</Combobox.Header>
          <Combobox.Options>
            {groceries.slice(0, 1).map((item) => (
              <Combobox.Option value={item} key={item}>
                {item}
              </Combobox.Option>
            ))}
            <Combobox.Footer>Test Footer</Combobox.Footer>
          </Combobox.Options>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}

export const ComboboxWithSelectedValue: Story = {
  render: () => {
    const combobox = useCombobox({
      opened: true,
      onDropdownClose: () => combobox.resetSelectedOption(),
    })
    const [value, setValue] = useState<string | undefined>('Bananas')

    return (
      <Combobox
        store={combobox}
        onOptionSubmit={(val) => {
          setValue(val)
          combobox.closeDropdown()
        }}
      >
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={
              <Combobox.ClearButton onClear={() => setValue(undefined)} />
            }
            suffixPointerEvents="none"
          >
            {value || (
              <Combobox.InputPlaceholder>Pick value</Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown>
          <Combobox.Options>
            <Combobox.Search value="Test search" />
            {groceries.map((item) => (
              <Combobox.Option
                value={item}
                key={item}
                selected={item === value}
                disabled={item === 'Chocolate'}
              >
                {item}
              </Combobox.Option>
            ))}
          </Combobox.Options>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}

export const ComboboxWithEmptyOption: Story = {
  render: () => {
    const combobox = useCombobox({
      opened: true,
      onDropdownClose: () => combobox.resetSelectedOption(),
    })
    const [value, setValue] = useState<string | undefined>('Bananas')

    return (
      <Combobox
        store={combobox}
        onOptionSubmit={(val) => {
          setValue(val)
          combobox.closeDropdown()
        }}
      >
        <Combobox.Target>
          <Combobox.InputBase
            component="button"
            type="button"
            pointer
            suffix={<Combobox.Chevron />}
            suffixPointerEvents="none"
          >
            {value || (
              <Combobox.InputPlaceholder>Pick value</Combobox.InputPlaceholder>
            )}
          </Combobox.InputBase>
        </Combobox.Target>

        <Combobox.Dropdown>
          <Combobox.Empty>No options</Combobox.Empty>
        </Combobox.Dropdown>
      </Combobox>
    )
  },
}
