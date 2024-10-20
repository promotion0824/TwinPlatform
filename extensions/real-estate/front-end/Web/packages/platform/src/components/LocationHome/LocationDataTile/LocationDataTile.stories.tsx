import type { Meta, StoryObj } from '@storybook/react'

import { LocationDataTile } from './LocationDataTile'

const meta: Meta<typeof LocationDataTile> = {
  args: {
    area: '21,000,000 sqft',
    location: 'New York, NY',
    status: 'operations',
    timeZone: 'America/New_York',
    yearOpened: 2019,
  },
  component: LocationDataTile,
}

export default meta

type Story = StoryObj<typeof LocationDataTile>

export const Playground: Story = {}

export const NonOperations: Story = {
  args: {
    status: 'construction',
  },
}
