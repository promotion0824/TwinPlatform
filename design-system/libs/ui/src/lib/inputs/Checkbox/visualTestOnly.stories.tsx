import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Checkbox } from './'

const defaultStory = {
  component: Checkbox,
  title: 'Checkbox',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Checkbox>

export const IndeterminateChecked: Story = {
  args: {
    label: 'Indeterminate Checked',
    indeterminate: true,
    checked: true,
  },
}

export const DisabledChecked: Story = {
  args: {
    label: 'Disabled Checked',
    disabled: true,
    checked: true,
  },
}

export const DisabledIndeterminate: Story = {
  args: {
    label: 'Disabled Indeterminate',
    disabled: true,
    indeterminate: true,
  },
}

export const Invalid: Story = {
  args: {
    label: 'Invalid',
    error: true,
  },
}
