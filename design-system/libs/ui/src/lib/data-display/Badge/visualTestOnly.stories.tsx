import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Badge, BadgeProps } from '.'

const defaultStory = {
  component: Badge,
  title: 'Badge',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Badge>

const Variants = (args: Pick<BadgeProps, 'variant'>) => (
  <>
    <Badge color="gray" {...args}>
      Badge
    </Badge>
    <Badge color="red" {...args}>
      Badge
    </Badge>
    <Badge color="orange" {...args}>
      Badge
    </Badge>
    <Badge color="yellow" {...args}>
      Badge
    </Badge>
    <Badge color="teal" {...args}>
      Badge
    </Badge>
    <Badge color="green" {...args}>
      Badge
    </Badge>
    <Badge color="cyan" {...args}>
      Badge
    </Badge>
    <Badge color="blue" {...args}>
      Badge
    </Badge>
    <Badge color="purple" {...args}>
      Badge
    </Badge>
    <Badge color="pink" {...args}>
      Badge
    </Badge>
  </>
)

export const DotColors: Story = {
  render: () => <Variants variant="dot" />,
}

export const MutedColors: Story = {
  render: () => <Variants variant="muted" />,
}

export const SubtleColors: Story = {
  render: () => <Variants variant="subtle" />,
}

export const OutlineColors: Story = {
  render: () => <Variants variant="outline" />,
}

export const SquishedDot: Story = {
  render: () => (
    <div style={{ display: 'flex', width: '80px' }}>
      <Badge color="purple" variant="dot">
        Badge Label
      </Badge>
    </div>
  ),
}
