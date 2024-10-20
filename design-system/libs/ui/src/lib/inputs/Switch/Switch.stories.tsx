import { useState } from 'react'
import type { Meta, StoryObj } from '@storybook/react'

import { Switch } from '.'
import { Stack } from '../../layout/Stack'
import { Box } from '../../misc/Box'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Switch> = {
  title: 'Switch',
  component: Switch,
}
export default meta

type Story = StoryObj<typeof Switch>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const Label: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: 'Show twins',
  },
}

export const LabelPosition: Story = {
  render: () => (
    <Stack>
      <Switch label="Show twins" labelPosition="left" />
      <Switch label="Show twins" labelPosition="right" />
    </Stack>
  ),
}

export const Justify: Story = {
  render: () => (
    <Stack maw={400}>
      <Box data-testid="switch-input">
        <Switch label="Show twins" labelPosition="left" justify="flex-start" />
      </Box>
      <Box data-testid="switch-input">
        <Switch label="Show twins" labelPosition="left" justify="flex-end" />
      </Box>
      <Box data-testid="switch-input">
        <Switch
          label="Show twins"
          labelPosition="left"
          justify="space-between"
        />
      </Box>
    </Stack>
  ),
}

export const LongLabel: Story = {
  render: () => (
    <Stack w={200}>
      <Switch label="Long labels will wrap inside their parent elements as expected" />
    </Stack>
  ),
}

export const Disabled: Story = {
  render: () => (
    <Stack>
      <Switch disabled label="Show twins" />
      <Switch checked disabled label="Show twins" />
    </Stack>
  ),
}

export const Error: Story = {
  render: () => {
    const [checked, setChecked] = useState(false)

    return (
      <Switch
        error={!checked}
        label="Do you agree to the terms and conditions?"
        onChange={() => setChecked(!checked)}
      />
    )
  },
}
