import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import GroupedSensors from './GroupedSensors'
import 'twin.macro'
import {
  pointsWithHosted,
  pointsWithoutHosted,
  liveDataPoints,
} from './samplePoints'

const meta: Meta<typeof GroupedSensors> = {
  component: GroupedSensors,
  render: (args) => (
    <GroupedSensors
      {...args}
      onTogglePoint={() => {
        // do nothing.
      }}
    />
  ),
}

export default meta
type Story = StoryObj<typeof GroupedSensors>

export const GroupedSensorsWithHostedByDevice: Story = {
  args: {
    hostedBy: { name: 'BACnet Device 13257' },
    connector: { name: 'CUS-CENTRALTOWER-BMS-BACNET' },
    points: pointsWithHosted,
    liveDataPoints,
  },
}
export const GroupedSensorsWithoutHostedByDevice: Story = {
  args: {
    connector: { name: 'CUS-CENTRALTOWER-VIBRATION' },
    points: pointsWithoutHosted,
    liveDataPoints,
  },
}
export const GroupedSensorsWithoutConnector: Story = {
  args: {
    points: pointsWithoutHosted,
    liveDataPoints,
  },
}
