import type { StoryObj } from '@storybook/react'

import { Menu } from '.'
import { Checkbox } from '../../inputs/Checkbox'
import { CheckboxGroup } from '../../inputs/CheckboxGroup'
import { Radio } from '../../inputs/Radio'
import { RadioGroup } from '../../inputs/RadioGroup'

const defaultStory = {
  component: Menu,
  title: 'Menu',
}

export default defaultStory

type Story = StoryObj<typeof Menu>

export const Radios: Story = {
  render: () => {
    return (
      <Menu width={200} defaultOpened>
        <Menu.Target>
          <button>Toggle Menu</button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item>Menu item</Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Radio
              label="Long long long long long long long long label"
              value="longLabel"
            />
          </Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Radio label="Single radio" value="singleRadio" />
          </Menu.Item>

          <RadioGroup>
            <Menu.Item closeMenuOnClick={false}>
              <Radio label="Cost" value="cost" />
            </Menu.Item>
            <Menu.Item closeMenuOnClick={false}>
              <Radio label="Energy" value="energy" />
            </Menu.Item>
          </RadioGroup>
        </Menu.Dropdown>
      </Menu>
    )
  },
}

export const Checkboxes: Story = {
  render: () => {
    return (
      <Menu width={200} defaultOpened>
        <Menu.Target>
          <button>Toggle Menu</button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item>Menu item</Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Checkbox
              label="Long long long long long long long long label"
              value="longLabel"
            />
          </Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Checkbox label="Single radio" value="singleCheckbox" />
          </Menu.Item>

          <CheckboxGroup>
            <Menu.Item closeMenuOnClick={false}>
              <Checkbox label="Cost" value="cost" />
            </Menu.Item>
            <Menu.Item closeMenuOnClick={false}>
              <Checkbox label="Energy" value="energy" />
            </Menu.Item>
          </CheckboxGroup>
        </Menu.Dropdown>
      </Menu>
    )
  },
}

export const FreeWidthMenu: Story = {
  render: () => {
    return (
      <Menu defaultOpened>
        <Menu.Target>
          <button>Toggle Menu</button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item>menu item</Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Checkbox
              label="Long long long long long long long long label"
              value="longLabelCheckbox"
            />
          </Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Radio
              label="Long long long long long long long long label"
              value="longLabelCheckboxGroup"
            />
          </Menu.Item>

          <CheckboxGroup>
            <Menu.Item closeMenuOnClick={false}>
              <Checkbox
                label="Long long long long long long long long label"
                value="longLabelRadio"
              />
            </Menu.Item>
          </CheckboxGroup>
          <RadioGroup>
            <Menu.Item closeMenuOnClick={false}>
              <Radio
                label="Long long long long long long long long label"
                value="longLabelRadioGroup"
              />
            </Menu.Item>
          </RadioGroup>
        </Menu.Dropdown>
      </Menu>
    )
  },
}
