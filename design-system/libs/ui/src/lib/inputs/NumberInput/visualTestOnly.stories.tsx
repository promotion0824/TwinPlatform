import type { StoryObj } from '@storybook/react'

import { NumberInput } from '.'
import { Icon } from '../../misc/Icon'

const defaultStory = {
  component: NumberInput,
  title: 'NumberInput',
}

export default defaultStory

type Story = StoryObj<typeof NumberInput>

export const PlaceholderInvalid: Story = {
  args: {
    error: true,
    placeholder: 'Please enter a number',
  },
}

export const PlaceholderDisabled: Story = {
  args: {
    disabled: true,
    placeholder: 'Please enter a number',
  },
}

export const PlaceholderReadOnly: Story = {
  args: {
    placeholder: 'Please enter a number',
    readOnly: true,
  },
}

export const PrefixInvalid: Story = {
  args: {
    error: true,
    prefix: <Icon icon="info" />,
    value: 0,
  },
}

export const PrefixDisabled: Story = {
  args: {
    disabled: true,
    prefix: <Icon icon="info" />,
    value: 0,
  },
}

export const PrefixReadOnly: Story = {
  args: {
    prefix: <Icon icon="info" />,
    readOnly: true,
    value: 0,
  },
}
