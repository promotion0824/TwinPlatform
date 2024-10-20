import type { StoryObj } from '@storybook/react'

import { Select } from '.'
import { Icon } from '../../misc/Icon'

const defaultStory = {
  component: Select,
  title: 'Select',
  args: {
    data: [
      { value: 'react', label: 'React' },
      { value: 'ng', label: 'Angular' },
      { value: 'svelte', label: 'Svelte' },
      { value: 'vue', label: 'Vue' },
    ],
  },
}

export default defaultStory

type Story = StoryObj<typeof Select>

export const LongValue: Story = {
  decorators: [
    (Story) => (
      <div css={{ width: 210 }}>
        <Story />
      </div>
    ),
  ],
  args: {
    data: [
      {
        value: 'react',
        label: 'React long long long long long long label',
      },
    ],
    value: 'react',
  },
}

export const PrefixAndSuffix: Story = {
  args: {
    prefix: <Icon icon="info" />,
    suffix: <Icon icon="search" />,
  },
}

export const WithScrollbar: Story = {
  decorators: [
    (Story) => (
      <div css={{ height: 110, width: 220 }}>
        <Story />
      </div>
    ),
  ],
  args: {
    data: [
      { value: 'react', label: 'React' },
      { value: 'ng', label: 'Angular' },
      { value: 'svelte', label: 'Svelte' },
      { value: 'vue', label: 'Vue' },
      {
        value: 'long',
        label: 'React long long long long long long label',
      },
    ],
    maxDropdownHeight: 50,
    initiallyOpened: true,
  },
}
