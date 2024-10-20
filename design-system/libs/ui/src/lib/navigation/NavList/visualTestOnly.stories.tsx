import type { Meta, StoryObj } from '@storybook/react'

import { NavList } from '.'
import { NavListDecorator } from './NavList.stories'
import { FlexDecorator } from '../../../storybookUtils'

const meta: Meta<typeof NavList> = {
  title: 'NavList',
  component: NavList,
  decorators: [NavListDecorator, FlexDecorator],
}
export default meta

type Story = StoryObj<typeof NavList>

export const MultipleNestedNavLists: Story = {
  render: () => (
    <NavList>
      <NavList.Item icon="public" label="Portfolios" />
      <NavList.Item defaultOpened icon="group" label="Authorization">
        <NavList.Item defaultOpened label="Users">
          <NavList.Item active label="Roles" />
          <NavList.Item label="Permissions" />
        </NavList.Item>
        <NavList.Item label="Groups" />
      </NavList.Item>
      <NavList.Item icon="sort" label="Models of Interest" />
    </NavList>
  ),
}
