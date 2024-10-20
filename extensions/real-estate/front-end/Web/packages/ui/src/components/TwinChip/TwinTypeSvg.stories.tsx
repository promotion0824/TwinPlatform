import type { Meta, StoryObj } from '@storybook/react'
import React from 'react'
import TwinTypeSvg from './TwinTypeSvg'

const meta: Meta<typeof TwinTypeSvg> = {
  component: TwinTypeSvg,
}

export default meta
type Story = StoryObj<typeof TwinTypeSvg>

export const Default: Story = {
  render: () => (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'start',
        gap: '4px',
      }}
    >
      <TwinTypeSvg type="asset" />
      <TwinTypeSvg type="building" />
      <TwinTypeSvg type="building_component" />
      <TwinTypeSvg type="equipment_group" />
      <TwinTypeSvg type="level" />
      <TwinTypeSvg type="room" />
      <TwinTypeSvg type="system" />
      <TwinTypeSvg type="tenancy_unit" />
      <TwinTypeSvg type="zone" />
      <TwinTypeSvg type="unknown" />
      <TwinTypeSvg />
    </div>
  ),
}
