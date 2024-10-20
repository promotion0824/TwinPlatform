import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Avatar } from '.'
import { Icon } from '../../misc/Icon'

const defaultStory = {
  component: Avatar,
  title: 'Avatar',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Avatar>

export const SizesWithCirclePlaceholder: Story = {
  render: () => (
    <>
      <Avatar size="sm" />
      <Avatar size="md" />
      <Avatar size="lg" />
    </>
  ),
}

export const SizesWithRectanglePlaceholder: Story = {
  render: () => (
    <>
      <Avatar size="sm" shape="rectangle" />
      <Avatar size="md" shape="rectangle" />
      <Avatar size="lg" shape="rectangle" />
    </>
  ),
}

export const SizesWithCircleImage: Story = {
  render: () => (
    <>
      <Avatar size="sm" src="example-avatar.jpeg" alt="dog" />
      <Avatar size="md" src="example-avatar.jpeg" alt="dog" />
      <Avatar size="lg" src="example-avatar.jpeg" alt="dog" />
    </>
  ),
}

export const SizesWithCircleInitials: Story = {
  render: () => (
    <>
      <Avatar size="sm">AB</Avatar>
      <Avatar size="md">AB</Avatar>
      <Avatar size="lg">AB</Avatar>
    </>
  ),
}

export const SizesWithCircleIcon: Story = {
  render: () => (
    <>
      <Avatar size="sm">
        <Icon icon="star" />
      </Avatar>
      <Avatar size="md">
        <Icon icon="star" />
      </Avatar>
      <Avatar size="lg">
        <Icon icon="star" />
      </Avatar>
    </>
  ),
}

export const VariantsWithCircle: Story = {
  render: () => (
    <>
      <Avatar variant="bold" color="red" />
      <Avatar variant="muted" color="red" />
      <Avatar variant="subtle" color="red" />
      <Avatar variant="outline" color="red" />

      <Avatar variant="bold" color="red" shape="rectangle" />
      <Avatar variant="muted" color="red" shape="rectangle" />
      <Avatar variant="subtle" color="red" shape="rectangle" />
      <Avatar variant="outline" color="red" shape="rectangle">
        <Icon icon="star"></Icon>
      </Avatar>
    </>
  ),
}
