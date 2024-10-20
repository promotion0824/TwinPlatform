import type { Meta, StoryObj } from '@storybook/react'

import { Stack } from '.'
import { Button } from '../../buttons/Button'

const meta: Meta<typeof Stack> = {
  title: 'Stack',
  component: Stack,
}
export default meta

type Story = StoryObj<typeof Stack>

export const Playground: Story = {
  render: () => (
    <Stack>
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Stack>
  ),
}

export const Gap: Story = {
  render: () => (
    <Stack gap="s32">
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Stack>
  ),
}

export const Justify: Story = {
  render: () => (
    <Stack justify="flex-end" style={{ height: '200px' }}>
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Stack>
  ),
}

export const Align: Story = {
  render: () => (
    <Stack align="self-end">
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Stack>
  ),
}
