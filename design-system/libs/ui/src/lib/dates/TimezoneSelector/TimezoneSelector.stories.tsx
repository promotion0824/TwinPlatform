import type { Meta, StoryObj } from '@storybook/react'

import { TimezoneSelector } from './TimezoneSelector'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof TimezoneSelector> = {
  title: 'TimezoneSelector',
  component: TimezoneSelector,
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof TimezoneSelector>

export const Playground: Story = {
  args: {},
}
