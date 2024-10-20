import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'
import { storyContainerTestId } from '../../../storybookUtils'

import { Menu } from '.'

import { Button } from '../../buttons/Button'
import { Avatar } from '../../data-display/Avatar'
import { useHotkeys } from '../../hooks'
import { Checkbox } from '../../inputs/Checkbox'
import { CheckboxGroup } from '../../inputs/CheckboxGroup'
import { Radio } from '../../inputs/Radio'
import { RadioGroup } from '../../inputs/RadioGroup'
import { Icon } from '../../misc/Icon'

const meta: Meta<typeof Menu> = {
  title: 'Menu',
  component: Menu,
}
export default meta

type Story = StoryObj<typeof Menu>

export const Playground: Story = {
  render: () => (
    <Menu>
      <Menu.Target>
        <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
          Toggle Menu
        </Button>
      </Menu.Target>
      <Menu.Dropdown>
        <Menu.Item>Menu item 1</Menu.Item>
        <Menu.Item>Menu item 2</Menu.Item>
        <Menu.Item>Menu item 3</Menu.Item>
      </Menu.Dropdown>
    </Menu>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 130 }}>
        <Story />
      </div>
    ),
  ],
}

export const Controlled: Story = {
  render: () => {
    const [opened, setOpened] = useState(true)

    return (
      <Menu opened={opened} onChange={setOpened}>
        <Menu.Target>
          <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
            Toggle Menu
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item>Menu item 1</Menu.Item>
          <Menu.Item>Menu item 2</Menu.Item>
          <Menu.Item>Menu item 3</Menu.Item>
        </Menu.Dropdown>
      </Menu>
    )
  },
  decorators: [
    (Story) => (
      <div css={{ height: 130 }}>
        <Story />
      </div>
    ),
  ],
}

export const MenuItemStyles: Story = {
  render: () => (
    <Menu defaultOpened>
      <Menu.Target>
        <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
          Toggle Menu
        </Button>
      </Menu.Target>
      <Menu.Dropdown>
        <Menu.Item>Menu item</Menu.Item>
        <Menu.Item disabled>Menu item</Menu.Item>
        <Menu.Item intent="negative">Menu item</Menu.Item>
        <Menu.Item disabled intent="negative">
          Menu item
        </Menu.Item>
      </Menu.Dropdown>
    </Menu>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 160 }}>
        <Story />
      </div>
    ),
  ],
}

export const DividerAndLabel: Story = {
  render: () => (
    <Menu width={200} defaultOpened>
      <Menu.Target>
        <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
          Toggle Menu
        </Button>
      </Menu.Target>
      <Menu.Dropdown>
        <Menu.Label>GROUP LABEL</Menu.Label>
        <Menu.Item>Menu item</Menu.Item>
        <Menu.Item>Menu item</Menu.Item>

        <Menu.Divider />

        <Menu.Label>GROUP LABEL</Menu.Label>
        <Menu.Item>Menu item</Menu.Item>
        <Menu.Item>Menu item</Menu.Item>
      </Menu.Dropdown>
    </Menu>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 230 }}>
        <Story />
      </div>
    ),
  ],
}

/**
 * Please note that the `Menu.Item` for each `Radio` might need
 * `closeMenuOnClick={false}`, which means after making a selection, the menu is
 * still open.
 */
export const WithRadioGroup: Story = {
  render: () => {
    const [selected, setSelected] = useState('cost')

    return (
      <Menu defaultOpened>
        <Menu.Target>
          <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
            Toggle Menu
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Label>Single Radios</Menu.Label>
          <Menu.Item closeMenuOnClick={false} disabled>
            <Radio label="Single Radio" />
          </Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Radio label="Single Radio" />
          </Menu.Item>

          <Menu.Divider />

          <Menu.Label>Radio Group</Menu.Label>
          <RadioGroup value={selected} onChange={setSelected}>
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
  decorators: [
    (Story) => (
      <div css={{ height: 210 }}>
        <Story />
      </div>
    ),
  ],
}

/**
 * Please note that the `Menu.Item` for each `Checkbox` should have
 * `closeMenuOnClick={false}`, which means after making a selection, the menu is
 * still open.
 */
export const WithCheckboxGroup: Story = {
  render: () => {
    const [selected, setSelected] = useState(['cost', 'resources'])

    return (
      <Menu defaultOpened>
        <Menu.Target>
          <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
            Toggle Menu
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Label>Independent Checkboxes</Menu.Label>
          <Menu.Item closeMenuOnClick={false} disabled>
            <Checkbox label="Independent Checkbox" />
          </Menu.Item>
          <Menu.Item closeMenuOnClick={false}>
            <Checkbox label="Independent Checkbox" />
          </Menu.Item>

          <Menu.Divider />

          <Menu.Label>Checkbox Group</Menu.Label>
          <CheckboxGroup value={selected} onChange={setSelected}>
            <Menu.Item closeMenuOnClick={false}>
              <Checkbox label="Cost" value="cost" />
            </Menu.Item>
            <Menu.Item closeMenuOnClick={false}>
              <Checkbox label="Energy" value="energy" />
            </Menu.Item>
            <Menu.Item closeMenuOnClick={false}>
              <Checkbox label="Resources" value="resources" />
            </Menu.Item>
          </CheckboxGroup>
        </Menu.Dropdown>
      </Menu>
    )
  },
  decorators: [
    (Story) => (
      <div css={{ height: 240 }}>
        <Story />
      </div>
    ),
  ],
}

export const SubMenus: Story = {
  render: () => (
    <Menu position="bottom-start" defaultOpened>
      <Menu.Target>
        <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
          Toggle Menu
        </Button>
      </Menu.Target>
      <Menu.Dropdown>
        <Menu.Item>Item label</Menu.Item>
        <Menu.SubMenu
          menuProps={{
            defaultOpened: true,
          }}
        >
          <Menu.Target>
            <div
              css={{
                width: '100%',
                display: 'flex',
                justifyContent: 'space-between',
              }}
            >
              Submenu item label
              <Icon icon="chevron_right" />
            </div>
          </Menu.Target>
          <Menu.Dropdown>
            <Menu.Item>Item Label</Menu.Item>
            <Menu.SubMenu
              menuProps={{
                defaultOpened: true,
              }}
            >
              <Menu.Target>
                <div
                  css={{
                    width: '100%',
                    display: 'flex',
                    justifyContent: 'space-between',
                  }}
                >
                  Submenu item label
                  <Icon icon="chevron_right" />
                </div>
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Item>Item Label</Menu.Item>
                <Menu.Item>Item Label</Menu.Item>
              </Menu.Dropdown>
            </Menu.SubMenu>
          </Menu.Dropdown>
        </Menu.SubMenu>
      </Menu.Dropdown>
    </Menu>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 180, width: 460 }} data-testid={storyContainerTestId}>
        <Story />
      </div>
    ),
  ],
}

export const NavigationMenu: Story = {
  render: () => {
    return (
      <Menu position="bottom-end" width={200} defaultOpened>
        <Menu.Target>
          <Avatar>AA</Avatar>
        </Menu.Target>

        <Menu.Dropdown>
          <Menu.Item
            component="a"
            href="https://google.com"
            target="_blank"
            prefix={<Icon icon="open_in_new" />}
          >
            Help & Support
          </Menu.Item>
          <Menu.Item
            component="a"
            href="https://google.com"
            target="_blank"
            prefix={<Icon icon="open_in_new" />}
          >
            Privacy Policy
          </Menu.Item>
          <Menu.Item
            component="a"
            href="https://google.com"
            target="_blank"
            prefix={<Icon icon="open_in_new" />}
          >
            What’s new
          </Menu.Item>

          <Menu.Divider />

          <Menu.Item prefix={<Icon icon="info" />} intent="negative">
            Log Out
          </Menu.Item>
        </Menu.Dropdown>
      </Menu>
    )
  },
  decorators: [
    (Story) => (
      <div css={{ height: 190, display: 'flex', justifyContent: 'center' }}>
        <Story />
      </div>
    ),
  ],
}

export const WithShortcutKeys: Story = {
  render: () => {
    const handleSearch = () => alert('Trigger search')
    // ctrl + K or ⌘ + K to search
    useHotkeys([['mod+K', handleSearch]])
    return (
      <Menu width={200} defaultOpened>
        <Menu.Target>
          <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
            Toggle Menu
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item
            prefix={<Icon icon="search" />}
            suffix="⌘K"
            onClick={handleSearch}
          >
            Search
          </Menu.Item>
        </Menu.Dropdown>
      </Menu>
    )
  },
  decorators: [
    (Story) => (
      <div css={{ height: 90 }}>
        <Story />
      </div>
    ),
  ],
}
