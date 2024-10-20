import type { Meta, StoryObj } from '@storybook/react'
import { Badge, Group } from '@willowinc/ui'
import React from 'react'
import { DataTile } from './DataTile'

const meta: Meta<typeof DataTile> = {
  args: {
    fields: [
      {
        icon: 'construction',
        label: 'Status',
        value: (
          <Badge color="green" size="sm" variant="dot">
            Operations
          </Badge>
        ),
      },
      {
        icon: 'my_location',
        label: 'Location',
        value: 'New York, NY',
      },
      {
        icon: 'paid',
        label: 'Active Avoidable Cost',
        value: (
          <Group gap="s4">
            <div style={{ fontWeight: 600 }}>14K</div>
            <div>USD/year</div>
          </Group>
        ),
      },
    ],
    title: 'Information',
  },
  component: DataTile,
}

export default meta

type Story = StoryObj<typeof DataTile>

export const Playground: Story = {}
