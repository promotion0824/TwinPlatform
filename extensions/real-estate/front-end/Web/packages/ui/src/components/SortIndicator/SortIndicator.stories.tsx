import type { Meta, StoryObj } from '@storybook/react'
import SortIndicator from './SortIndicator'

const meta: Meta<typeof SortIndicator> = {
  component: SortIndicator,
}

export default meta
type Story = StoryObj<typeof SortIndicator>

export const BaseSortIndicator: Story = {
  args: {
    isSorted: true,
    children: 'some random text',
    $transform: 'translateY(-12px) rotate(-180deg)',
  },
}

export const DescendingSortIndicator: Story = {
  args: {
    isSorted: true,
    children: 'some random text',
    $transform: 'translateY(-12px) rotate(-180deg)',
  },
}
