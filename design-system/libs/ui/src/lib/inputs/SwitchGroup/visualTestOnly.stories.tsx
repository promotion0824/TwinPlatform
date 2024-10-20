import type { StoryObj } from '@storybook/react'
import { SwitchGroup } from '.'
import { FlexDecorator } from '../../../storybookUtils'
import { Switch } from '../Switch/Switch'

const defaultStory = {
  component: SwitchGroup,
  decorators: [FlexDecorator],
  title: 'SwitchGroup',
}

export default defaultStory

type Story = StoryObj<typeof SwitchGroup>

export const InlineDescription: Story = {
  render: () => (
    <SwitchGroup
      description="Select all twins you would like to enable"
      inline
      label="Twins"
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}

export const InlineError: Story = {
  render: () => (
    <SwitchGroup
      description="Select all twins you would like to enable"
      error="You must enable at least one twin"
      inline
      label="Twins"
    >
      <Switch label="Twin 1" value="twin1" />
      <Switch label="Twin 2" value="twin2" />
      <Switch label="Twin 3" value="twin3" />
      <Switch label="Twin 4" value="twin4" />
    </SwitchGroup>
  ),
}
