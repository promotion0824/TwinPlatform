import { useState } from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Checkbox } from './'
import { Stack } from '../../layout/Stack'
import { Box } from '../../misc/Box'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Checkbox> = {
  title: 'Checkbox',
  component: Checkbox,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof Checkbox>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
}

export const WithLabel: Story = {
  render: () => <Checkbox label="Label" />,
}

export const LeftLabel: Story = {
  render: () => <Checkbox label="left label" labelPosition="left" />,
}

export const Justify: Story = {
  render: () => (
    <Stack>
      <Box data-testid="willow-checkbox">
        <Checkbox
          w={400}
          label="label"
          labelPosition="left"
          justify="flex-start"
        />
      </Box>
      <Box data-testid="willow-checkbox">
        <Checkbox
          w={400}
          label="label"
          labelPosition="left"
          justify="flex-end"
        />
      </Box>
      <Box data-testid="willow-checkbox">
        <Checkbox
          w={400}
          label="label"
          labelPosition="left"
          justify="space-between"
        />
      </Box>
    </Stack>
  ),
}

export const WithLongLabel: Story = {
  render: () => (
    // show user how to define container width
    <Stack w={200}>
      <Checkbox label="Long long long long long long long long long label" />
    </Stack>
  ),
}

export const Checked: Story = {
  render: () => {
    const [checked, setChecked] = useState(true)

    return (
      <Checkbox
        checked={checked}
        onChange={(event) => {
          setChecked(event.target.checked)
        }}
        label="Label"
      />
    )
  },
}

/**
 * Indeterminate is typically only used when a parent checkbox can control
 * a group of children.
 */
export const Indeterminate: Story = {
  render: () => <Checkbox label="Indeterminate" indeterminate />,
}

export const Disabled: Story = {
  render: () => <Checkbox label="Disabled" disabled />,
}

export const Error: Story = {
  render: () => <Checkbox label="Invalid checkbox" error />,
}
