import { useState } from 'react'
import type { Meta, StoryObj } from '@storybook/react'

import { NavList } from '.'
import { FlexDecorator } from '../../../storybookUtils'

export const NavListDecorator = (Story: React.ComponentType) => (
  <div style={{ width: '300px' }}>
    <Story />
  </div>
)

const meta: Meta<typeof NavList> = {
  title: 'NavList',
  component: NavList,
  decorators: [NavListDecorator, FlexDecorator],
  excludeStories: ['NavListDecorator'],
}
export default meta

type Story = StoryObj<typeof NavList>

export const Playground: Story = {
  render: () => (
    <NavList>
      <NavList.Item active label="Users" />
      <NavList.Item label="Roles" />
      <NavList.Item label="Permissions" />
    </NavList>
  ),
}

export const Icons: Story = {
  render: () => (
    <NavList>
      <NavList.Item active icon="group" label="Users" />
      <NavList.Item icon="settings" label="Roles" />
      <NavList.Item icon="notifications" label="Permissions" />
    </NavList>
  ),
}

export const SelectingItems: Story = {
  render: () => {
    const items = [
      { icon: 'group', label: 'Users' },
      { icon: 'settings', label: 'Roles' },
      { icon: 'notifications', label: 'Permissions' },
    ] as const

    const [active, setActive] = useState<string>(items[0].label)

    return (
      <NavList>
        {items.map((item) => (
          <NavList.Item
            active={active === item.label}
            icon={item.icon}
            key={item.label}
            label={item.label}
            onClick={() => setActive(item.label)}
          />
        ))}
      </NavList>
    )
  },
}

export const NestedNavLists: Story = {
  render: () => (
    <NavList>
      <NavList.Item icon="public" label="Portfolios" />
      <NavList.Item defaultOpened icon="group" label="Authorization">
        <NavList.Item label="Users" />
        <NavList.Item active label="Roles" />
        <NavList.Item label="Permissions" />
      </NavList.Item>
      <NavList.Item icon="sort" label="Models of Interest" />
    </NavList>
  ),
}

export const Groups: Story = {
  render: () => (
    <NavList>
      <NavList.Group title="User Settings">
        <NavList.Item icon="group" label="Profile" />
        <NavList.Item icon="settings" label="Preferences" />
        <NavList.Item icon="notifications" label="Notifications" />
      </NavList.Group>
      <NavList.Group title="Admin">
        <NavList.Item icon="public" label="Portfolios" />
        <NavList.Item defaultOpened icon="group" label="Authorization">
          <NavList.Item active label="Users" />
          <NavList.Item label="Roles" />
          <NavList.Item label="Permissions" />
        </NavList.Item>
        <NavList.Item icon="sort" label="Models of Interest" />
      </NavList.Group>
    </NavList>
  ),
}
