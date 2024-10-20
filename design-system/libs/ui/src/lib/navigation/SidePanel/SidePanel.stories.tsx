import type { Meta, StoryObj } from '@storybook/react'
import { css } from 'styled-components'

import { SidePanel } from '.'
import { NavList } from '../NavList'

const meta: Meta<typeof SidePanel> = {
  title: 'SidePanel',
  component: SidePanel,
  decorators: [
    (Story) => (
      <div
        css={css(({ theme }) => ({
          padding: theme.spacing.s16,
          height: 400,
          background: theme.color.neutral.bg.base.default,
          color: theme.color.neutral.fg.default,
        }))}
      >
        <Story />
      </div>
    ),
  ],
}
export default meta

type Story = StoryObj<typeof SidePanel>

export const Playground: Story = {
  render: () => (
    <SidePanel title="Reports">
      <NavList>
        <NavList.Item active label="Asset Knowledge" />
        <NavList.Item label="Chilled Water Thermal Energy Consumption" />
        <NavList.Item label="Chiller Operations" />
        <NavList.Item label="Electricity Consumption" />
        <NavList.Item label="HVAC Systems" />
        <NavList.Item label="Hot Water Thermal Energy Consumption" />
        <NavList.Item label="Occupancy IoT Sensors" />
        <NavList.Item label="Terminal Units Occupancy Detection" />
        <NavList.Item label="Twins Summary" />
        <NavList.Item label="Wider Consumption" />
      </NavList>
    </SidePanel>
  ),
}

export const CustomizedWidth: Story = {
  render: () => (
    <SidePanel css={{ width: 400 }} title="Reports">
      <NavList>
        <NavList.Item active label="Asset Knowledge" />
        <NavList.Item label="Chilled Water Thermal Energy Consumption" />
        <NavList.Item label="Chiller Operations" />
        <NavList.Item label="Electricity Consumption" />
        <NavList.Item label="HVAC Systems" />
        <NavList.Item label="Hot Water Thermal Energy Consumption" />
        <NavList.Item label="Occupancy IoT Sensors" />
        <NavList.Item label="Terminal Units Occupancy Detection" />
        <NavList.Item label="Twins Summary" />
        <NavList.Item label="Wider Consumption" />
      </NavList>
    </SidePanel>
  ),
}
