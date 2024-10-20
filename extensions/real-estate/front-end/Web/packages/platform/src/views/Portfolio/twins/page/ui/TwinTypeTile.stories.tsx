import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import TwinTypeTile from './TwinTypeTile'

const meta: Meta<typeof TwinTypeTile> = {
  component: TwinTypeTile,
}

export default meta
type Story = StoryObj<typeof TwinTypeTile>

export const Default: Story = {
  render: () => {
    const props = { onClick: () => null }

    return (
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'start',
          gap: '4px',
        }}
      >
        <TwinTypeTile type="asset" {...props} />
        <TwinTypeTile type="building" {...props} />
        <TwinTypeTile type="building_component" {...props} />
        <TwinTypeTile type="equipment_group" {...props} />
        <TwinTypeTile type="level" {...props} />
        <TwinTypeTile type="room" {...props} />
        <TwinTypeTile type="system" {...props} />
        <TwinTypeTile type="tenancy_unit" {...props} />
        <TwinTypeTile type="zone" {...props} />
      </div>
    )
  },
}
