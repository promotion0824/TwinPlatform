import type { Meta, StoryObj } from '@storybook/react'

import { Illustration } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Illustration> = {
  title: 'Illustration',
  component: Illustration,
}
export default meta

type Story = StoryObj<typeof Illustration>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const NoData: Story = {
  ...storybookAutoSourceParameters,
  args: {
    illustration: 'no-data',
  },
}

export const NoResults: Story = {
  ...storybookAutoSourceParameters,
  args: {
    illustration: 'no-results',
  },
}

export const NotFound: Story = {
  ...storybookAutoSourceParameters,
  args: {
    illustration: 'not-found',
  },
}

export const Light: Story = {
  ...storybookAutoSourceParameters,
  args: {
    themeName: 'light',
  },
}

export const CustomSize: Story = {
  ...storybookAutoSourceParameters,
  args: {
    w: 's32',
  },
}
