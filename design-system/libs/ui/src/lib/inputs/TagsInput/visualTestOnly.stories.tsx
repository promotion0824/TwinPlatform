import type { StoryObj } from '@storybook/react'

import { TagsInput } from '.'

const defaultStory = {
  component: TagsInput,
  title: 'TagsInput',
}

export default defaultStory

type Story = StoryObj<typeof TagsInput>

export const WithValues: Story = {
  args: {
    value: ['tag 1', 'tag 2', 'tag 3'],
    placeholder: 'Enter tag',
  },
}

export const WithPlaceholder: Story = {
  args: {
    placeholder: 'Enter tag',
  },
}

export const ReadonlyWithValue: Story = {
  args: {
    readOnly: true,
    value: ['tag'],
  },
}

export const DisabledWithValue: Story = {
  args: {
    disabled: true,
    value: ['tag'],
  },
}

export const ReadonlyWithPlaceholder: Story = {
  args: {
    readOnly: true,
    placeholder: 'Enter tag',
  },
}

export const DisabledWithPlaceholder: Story = {
  args: {
    disabled: true,
    placeholder: 'Enter tag',
  },
}

export const WithSuggestions: Story = {
  args: {
    data: ['Apple', 'Banana', 'Cherry'],
    dropdownOpened: true,
  },
}
