import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Switch } from '.'

const defaultStory = {
  component: Switch,
  title: 'Switch',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Switch>

export const NoLabel: Story = {
  args: {},
}

export const LabelRight: Story = {
  args: {
    label: 'Show twins',
  },
}

export const LabelLeft: Story = {
  args: {
    label: 'Show twins',
    labelPosition: 'left',
  },
}

export const LabelLong: Story = {
  render: () => (
    <div style={{ width: '200px' }}>
      <Switch label="Long labels will wrap inside their parent elements as expected" />
    </div>
  ),
}

export const DisabledChecked: Story = {
  args: {
    checked: true,
    disabled: true,
  },
}

export const DisabledUnchecked: Story = {
  args: {
    disabled: true,
  },
}

export const ErrorChecked: Story = {
  args: {
    checked: true,
    error: true,
  },
}

export const ErrorUnchecked: Story = {
  args: {
    error: true,
  },
}
