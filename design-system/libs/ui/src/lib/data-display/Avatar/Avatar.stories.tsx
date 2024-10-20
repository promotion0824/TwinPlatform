import type { Meta, StoryObj } from '@storybook/react'
import { StoryFlexContainer, FlexDecorator } from '../../../storybookUtils'

import { Avatar } from '.'
import { Icon } from '../../misc/Icon'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Avatar> = {
  title: 'Avatar',
  component: Avatar,
}
export default meta

type Story = StoryObj<typeof Avatar>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

/**
 * If src prop is not set, equals to null or image cannot be loaded,
 * placeholder icon will be displayed instead. Any React node can be used
 * instead of placeholder icon. Usually icon or initials are used in this case.
 *
 * Avatar renders img html element. Do not forget to add alt text. If image fails
 * to load alt will be used as title for placeholder.
 */
export const Content: Story = {
  render: () => (
    <>
      {/* Placeholder avatar */}
      <Avatar />
      <Avatar src="example-avatar.jpeg" alt="dog" />
      <Avatar>AA</Avatar>
      <Avatar>
        <Icon icon="pets" />
      </Avatar>
    </>
  ),
  decorators: [FlexDecorator],
}

export const Size: Story = {
  render: () => (
    <>
      <StoryFlexContainer>
        <Avatar size="sm" />
        <Avatar size="md" />
        <Avatar size="lg" />
      </StoryFlexContainer>
      <StoryFlexContainer>
        <Avatar size="sm" shape="rectangle" />
        <Avatar size="md" shape="rectangle" />
        <Avatar size="lg" shape="rectangle" />
      </StoryFlexContainer>
    </>
  ),
  decorators: [
    (Story) => (
      <StoryFlexContainer css={{ flexDirection: 'column' }}>
        <Story />
      </StoryFlexContainer>
    ),
  ],
}

export const Variant: Story = {
  render: () => (
    <>
      <StoryFlexContainer>
        <Avatar variant="bold" color="red" />
        <Avatar variant="muted" color="red" />
        <Avatar variant="subtle" color="red" />
        <Avatar variant="outline" color="red" />
      </StoryFlexContainer>
      <StoryFlexContainer>
        <Avatar variant="bold" color="red" shape="rectangle" />
        <Avatar variant="muted" color="red" shape="rectangle" />
        <Avatar variant="subtle" color="red" shape="rectangle" />
        <Avatar variant="outline" color="red" shape="rectangle" />
      </StoryFlexContainer>
    </>
  ),
  decorators: [
    (Story) => (
      <StoryFlexContainer css={{ flexDirection: 'column' }}>
        <Story />
      </StoryFlexContainer>
    ),
  ],
}

export const Color: Story = {
  render: () => (
    <>
      <Avatar color="gray" />
      <Avatar color="red" />
      <Avatar color="orange" />
      <Avatar color="yellow" />
      <Avatar color="teal" />
      <Avatar color="green" />
      <Avatar color="cyan" />
      <Avatar color="blue" />
      <Avatar color="purple" />
      <Avatar color="pink" />
    </>
  ),
  decorators: [FlexDecorator],
}

export const Shape: Story = {
  render: () => (
    <>
      <Avatar shape="circle">AA</Avatar>
      <Avatar shape="rectangle">AA</Avatar>
    </>
  ),
  decorators: [FlexDecorator],
}

export const WithTooltip: Story = {
  render: () => <Avatar tooltip="Tooltip Content">AC</Avatar>,
  decorators: [FlexDecorator],
}
