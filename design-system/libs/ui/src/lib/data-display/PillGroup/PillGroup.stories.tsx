import type { Meta, StoryObj } from '@storybook/react'

import { PillGroup } from '.'
import { Pill } from '../Pill/Pill'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof PillGroup> = {
  title: 'PillGroup',
  component: PillGroup,
  args: {
    children: [
      <Pill>Label 1</Pill>,
      <Pill>Label 2</Pill>,
      <Pill>Label 3</Pill>,
    ],
  },
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof PillGroup>

export const Playground: Story = {
  args: {},
}

export const Disabled: Story = {
  args: {
    disabled: true,
  },
}

export const CustomizedGap: Story = {
  args: {
    gap: 's16',
  },
}
