import type { Meta, StoryObj } from '@storybook/react'

import { Group } from '.'
import { Button } from '../../buttons/Button'

const meta: Meta<typeof Group> = {
  title: 'Group',
  component: Group,
}
export default meta

type Story = StoryObj<typeof Group>

export const Playground: Story = {
  render: () => (
    <Group>
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Group>
  ),
}

export const Gap: Story = {
  render: () => (
    <Group gap="s32">
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Group>
  ),
}

export const Justify: Story = {
  render: () => (
    <Group justify="flex-end">
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Group>
  ),
}

export const Align: Story = {
  render: () => (
    <Group align="self-end" style={{ height: '100px' }}>
      <Button>First</Button>
      <Button>Second</Button>
      <Button>Third</Button>
    </Group>
  ),
}

export const Wrap: Story = {
  render: () => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <Group style={{ width: '50px' }}>
        <Button>First</Button>
        <Button>Second</Button>
        <Button>Third</Button>
      </Group>
      <Group wrap="nowrap" style={{ width: '50px' }}>
        <Button>First</Button>
        <Button>Second</Button>
        <Button>Third</Button>
      </Group>
    </div>
  ),
}

export const Grow: Story = {
  render: () => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <Group>
        <Button>First</Button>
        <Button>Second</Button>
        <Button>Third</Button>
      </Group>
      <Group grow>
        <Button>First</Button>
        <Button>Second</Button>
        <Button>Third</Button>
      </Group>
    </div>
  ),
}

export const PreventGrowOverflow: Story = {
  render: () => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <Group grow>
        <Button>First</Button>
        <Button>Second Button With A Long Label</Button>
        <Button>Third</Button>
      </Group>
      <Group grow preventGrowOverflow={false}>
        <Button>First</Button>
        <Button>Second Button With A Long Label</Button>
        <Button>Third</Button>
      </Group>
    </div>
  ),
}
