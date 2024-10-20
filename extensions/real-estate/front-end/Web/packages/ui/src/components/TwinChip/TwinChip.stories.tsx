import type { Meta, StoryObj } from '@storybook/react'
import React from 'react'
import CountSummary from '../../../../platform/src/views/Portfolio/LocationCard/CountSummary'
import Icon from '../Icon/Icon'
import TwinChip from './TwinChip'

const meta: Meta<typeof TwinChip> = {
  component: TwinChip,
  render: (args) => (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'start',
        gap: '4px',
      }}
    >
      <TwinChip type="asset" {...args} />
      <TwinChip type="building" {...args} />
      <TwinChip type="building_component" {...args} />
      <TwinChip type="equipment_group" {...args} />
      <TwinChip type="level" {...args} />
      <TwinChip type="room" {...args} />
      <TwinChip type="system" {...args} />
      <TwinChip type="tenancy_unit" {...args} />
      <TwinChip type="zone" {...args} />
      <TwinChip type="unknown" {...args} />
      <TwinChip {...args} />
    </div>
  ),
}

export default meta
type Story = StoryObj<typeof TwinChip>

export const Basic: Story = {}

export const WithTextString: Story = {
  args: {
    text: 'Custom category',
  },
}

export const WithHighlight: Story = {
  args: {
    highlightOnHover: true,
  },
}

export const WithTextArray: Story = {
  args: {
    text: ['Asset', 'Equipment', 'HVAC', 'Air handling unit'],
  },
}

export const WithGappedText: Story = {
  args: {
    text: 'Text before',
    gappedText: 'Text after',
  },
}
export const WithIcon: Story = {
  args: {
    icon: <Icon size="tiny" icon="chevron" />,
  },
}

export const WithCount: Story = {
  args: {
    count: 23,
  },
}

export const Instance: Story = {
  args: {
    variant: 'instance',
    text: 'My cool twin',
  },
}

export const InstanceWithAdditionalInfo: Story = {
  args: {
    variant: 'instance',
    text: 'My cool twin',
    additionalInfo: [
      <CountSummary
        count={5}
        label="5"
        summaryType="insights"
        intent="negative"
      />,
      <CountSummary count={10} label="10" summaryType="insights" />,
      <CountSummary count={15} label="15" summaryType="tickets" />,
    ],
  },
}

export const InstanceWithIcon: Story = {
  args: {
    variant: 'instance',
    text: 'My cool twin',
    icon: <Icon size="tiny" icon="cross" />,
  },
}

export const VeryLongText: Story = {
  render: () => (
    <div style={{ width: '300px' }}>
      <TwinChip
        type="asset"
        text="This is a very very very very very looooooong text"
        icon={<Icon size="tiny" icon="more" onClick={() => {}} />}
      />
      <TwinChip
        type="asset"
        text="My asset"
        gappedText="This is a very very very looooooong text"
        icon={<Icon size="tiny" icon="more" onClick={() => {}} />}
      />
      <TwinChip
        type="asset"
        text={['Asset', 'Equipment', 'HVAC', 'Air handling unit']}
        gappedText="This is a very very very long text"
        icon={<Icon size="tiny" icon="more" onClick={() => {}} />}
      />
    </div>
  ),
}
