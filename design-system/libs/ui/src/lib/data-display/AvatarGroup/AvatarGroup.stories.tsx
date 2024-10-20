import type { Meta, StoryObj } from '@storybook/react'

import { AvatarGroup } from '.'
import { Avatar } from '../Avatar/Avatar'
import { Icon } from '../../misc/Icon'
import { FlexDecorator } from '../../../storybookUtils'

const meta: Meta<typeof AvatarGroup> = {
  title: 'AvatarGroup',
  component: AvatarGroup,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof AvatarGroup>

export const Playground: Story = {
  render: () => (
    <AvatarGroup>
      <Avatar />
      <Avatar src="example-avatar.jpeg" alt="dog" />
      <Avatar color="green">AA</Avatar>
      <Avatar>AA</Avatar>
      <Avatar>AA</Avatar>
      <Avatar>
        <Icon icon="pets" />
      </Avatar>
    </AvatarGroup>
  ),
}

export const HasOverflow: Story = {
  render: () => (
    <AvatarGroup maxItems={3}>
      <Avatar />
      <Avatar src="example-avatar.jpeg" alt="dog" />
      <Avatar color="green">AA</Avatar>
      <Avatar>AA</Avatar>
      <Avatar>AA</Avatar>
      <Avatar>
        <Icon icon="pets" />
      </Avatar>
    </AvatarGroup>
  ),
}

export const WithTooltip: Story = {
  render: () => (
    <AvatarGroup maxItems={3}>
      <Avatar tooltip="Tooltip 1" />
      <Avatar src="example-avatar.jpeg" alt="dog" tooltip="Tooltip 2" />
      <Avatar color="green" tooltip="Tooltip 3">
        AX
      </Avatar>
      <Avatar tooltip="Tooltip 4">AA</Avatar>
      <Avatar tooltip="Tooltip 5">AA</Avatar>
      <Avatar tooltip="Tooltip 6">
        <Icon icon="pets" />
      </Avatar>
    </AvatarGroup>
  ),
}
