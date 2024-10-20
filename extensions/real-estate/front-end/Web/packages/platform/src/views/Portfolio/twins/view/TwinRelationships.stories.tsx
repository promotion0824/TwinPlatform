import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import TwinRelationships from './TwinRelationships'

const meta: Meta<typeof TwinRelationships> = {
  component: TwinRelationships,
  render: (args) => <TwinRelationships {...args} />,
}

export default meta
type Story = StoryObj<typeof TwinRelationships>

export const MultipleRelationshipsAndLevels: Story = {
  args: {
    relationships: [
      {
        id: 'INV-151CS-LGC',
        siteId: 'e729ac18-192b-4174-91db-b3a624f1f1a4',
        modelOfInterest: {
          type: 'level',
        },
        name: 'Upper Ground Floor',
      },
      {
        id: 'INV-151CS-LGD',
        siteId: 'e719ac18-192b-4174-91db-b3a624f1f1a4',
        modelOfInterest: {
          type: 'level',
        },
        name: 'Lower Ground Floor',
      },
      {
        id: 'INV-151CS-ELS',
        siteId: 'e719ac18-192b-4174-91db-b3a624f1f1a4',
        modelOfInterest: {
          type: 'system',
        },
        name: 'Electrical System',
      },
    ],
  },
}

export const OneLevel: Story = {
  args: {
    relationships: MultipleRelationshipsAndLevels.args.relationships.slice(
      0,
      2
    ),
  },
}

export const OneRelationship: Story = {
  args: {
    relationships: MultipleRelationshipsAndLevels.args.relationships.slice(
      0,
      1
    ),
  },
}

export const NoRelationship: Story = {
  args: {
    relationships: [],
  },
}
