import type { Meta, StoryObj } from '@storybook/react'

import { Pill } from '.'
import { Group } from '../../layout/Group'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Pill> = {
  title: 'Pill',
  component: Pill,
  args: {
    children: 'Label',
  },
}
export default meta

type Story = StoryObj<typeof Pill>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
    withRemoveButton: true,
  },
}

export const Removable: Story = {
  ...storybookAutoSourceParameters,
  args: {
    withRemoveButton: true,
  },
}

export const Size: Story = {
  render: () => (
    <Group>
      <Pill size="sm" withRemoveButton>
        Label
      </Pill>
      <Pill size="md" withRemoveButton>
        Label
      </Pill>
    </Group>
  ),
}
