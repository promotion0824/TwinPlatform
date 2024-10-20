import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'
import { SwitchGroup } from '.'
import { FlexDecorator } from '../../../storybookUtils'
import { Switch } from '../Switch/Switch'

const meta: Meta<typeof SwitchGroup> = {
  title: 'SwitchGroup',
  component: SwitchGroup,
  decorators: [FlexDecorator],
}

export default meta

type Story = StoryObj<typeof SwitchGroup>

export const Playground: Story = {
  render: () => (
    <SwitchGroup label="Twins">
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const Controlled: Story = {
  render: () => {
    const [value, setValue] = useState<string[]>(['twin1'])

    return (
      <SwitchGroup onChange={setValue} value={value}>
        <Switch label="Twin 1" value="twin1" />
        <Switch label="Twin 2" value="twin2" />
        <Switch label="Twin 3" value="twin3" />
        <Switch label="Twin 4" value="twin4" />
      </SwitchGroup>
    )
  },
}

export const NoLabel: Story = {
  render: () => (
    <SwitchGroup>
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const Description: Story = {
  render: () => (
    <SwitchGroup
      description="Select all twins you would like to enable"
      label="Twins"
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const HorizontalLayout: Story = {
  render: () => (
    <SwitchGroup
      layout="horizontal"
      description="Select all twins you would like to enable"
      label="Twins"
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const HorizontalLayoutWithLabelWidth: Story = {
  render: () => (
    <SwitchGroup
      layout="horizontal"
      description="Select all twins you would like to enable"
      label="Twins"
      labelWidth={300}
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const Invalid: Story = {
  render: () => (
    <SwitchGroup
      description="Select all twins you would like to enable"
      error
      label="Twins"
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const Error: Story = {
  render: () => (
    <SwitchGroup
      description="Select all twins you would like to enable"
      error="You must enable at least one twin"
      label="Twins"
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const Required: Story = {
  render: () => (
    <SwitchGroup label="Twins" required>
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

/**
 * The `SwichGroup` itself can't be disabled, but you can still disable any/all of its
 * child `Switch` components as you normally would.
 */
export const Disabled: Story = {
  render: () => (
    <SwitchGroup label="Twins">
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch disabled label="Twin 3" value="twin3" />
      <Switch disabled label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const Inline: Story = {
  render: () => (
    <SwitchGroup inline label="Twins">
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}
