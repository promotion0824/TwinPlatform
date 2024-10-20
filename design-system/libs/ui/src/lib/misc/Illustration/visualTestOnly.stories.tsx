import type { StoryObj } from '@storybook/react'

import { Illustration } from '.'

const defaultStory = {
  component: Illustration,
  title: 'Illustration',
}

export default defaultStory

type Story = StoryObj<typeof Illustration>

export const NoPermissions: Story = {
  args: {
    w: 's20',
  },
}

export const NoData: Story = {
  args: {
    illustration: 'no-data',
    w: 's20',
  },
}

export const NoResults: Story = {
  args: {
    illustration: 'no-results',
    w: 's20',
  },
}

export const NotFound: Story = {
  args: {
    illustration: 'not-found',
    w: 's20',
  },
}
