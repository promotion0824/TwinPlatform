import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Badge } from '.'
import { Icon } from '../../misc/Icon'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Badge> = {
  title: 'Badge',
  component: Badge,
  args: {
    children: 'Badge',
  },
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof Badge>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const Size: Story = {
  render: () => (
    <>
      <Badge color="pink" size="xs">
        Badge
      </Badge>
      <Badge color="pink" size="sm">
        Badge
      </Badge>
      <Badge color="pink" size="md">
        Badge
      </Badge>
      <Badge color="pink" size="lg">
        Badge
      </Badge>
    </>
  ),
}

export const Variant: Story = {
  render: () => (
    <>
      <Badge variant="dot" color="teal">
        Badge
      </Badge>
      <Badge variant="bold" color="teal">
        Badge
      </Badge>
      <Badge variant="muted" color="teal">
        Badge
      </Badge>
      <Badge variant="subtle" color="teal">
        Badge
      </Badge>
      <Badge variant="outline" color="teal">
        Badge
      </Badge>
    </>
  ),
}

export const Color: Story = {
  render: () => (
    <>
      <Badge color="gray">Badge</Badge>
      <Badge color="red">Badge</Badge>
      <Badge color="orange">Badge</Badge>
      <Badge color="yellow">Badge</Badge>
      <Badge color="teal">Badge</Badge>
      <Badge color="green">Badge</Badge>
      <Badge color="cyan">Badge</Badge>
      <Badge color="blue">Badge</Badge>
      <Badge color="purple">Badge</Badge>
      <Badge color="pink">Badge</Badge>
    </>
  ),
}

export const PrefixAndSuffix: Story = {
  render: () => (
    <>
      <Badge
        size="xs"
        prefix={<Icon icon="info" size={16} />}
        suffix={<Icon icon="close" size={16} />}
      >
        Badge
      </Badge>
      <Badge
        size="sm"
        prefix={<Icon icon="info" />}
        suffix={<Icon icon="close" />}
      >
        Badge
      </Badge>
      <Badge
        size="md"
        prefix={<Icon icon="info" />}
        suffix={<Icon icon="close" />}
      >
        Badge
      </Badge>
      <Badge
        size="lg"
        prefix={<Icon icon="info" />}
        suffix={<Icon icon="close" />}
      >
        Badge
      </Badge>
    </>
  ),
}
