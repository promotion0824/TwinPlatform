import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Radio } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Radio> = {
  title: 'Radio',
  component: Radio,
  args: {
    label: 'Twins',
    value: 'twins',
  },
  decorators: [FlexDecorator],
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof Radio>

export const Playground: Story = {}

export const Checked: Story = {
  args: {
    defaultChecked: true,
  },
}

export const Disabled: Story = {
  args: {
    checked: true,
    disabled: true,
  },
}

export const Invalid: Story = {
  args: {
    defaultChecked: true,
    error: true,
  },
}
