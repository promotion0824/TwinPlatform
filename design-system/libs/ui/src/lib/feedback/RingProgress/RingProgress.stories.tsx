import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { RingProgress } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof RingProgress> = {
  title: 'RingProgress',
  component: RingProgress,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof RingProgress>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    value: 65,
  },
}

export const Intents: Story = {
  render: () => (
    <>
      <RingProgress intent="negative" value={65} />
      <RingProgress intent="notice" value={65} />
      <RingProgress intent="positive" value={65} />
      <RingProgress intent="primary" value={65} />
      <RingProgress intent="secondary" value={65} />
    </>
  ),
}

export const Icon: Story = {
  render: () => (
    <>
      <RingProgress icon="eco" intent="negative" value={65} />
      <RingProgress icon="eco" intent="notice" value={65} />
      <RingProgress icon="eco" intent="positive" value={65} />
      <RingProgress icon="eco" intent="primary" value={65} />
      <RingProgress icon="eco" intent="secondary" value={65} />
    </>
  ),
}

export const ShowValue: Story = {
  render: () => (
    <>
      <RingProgress intent="negative" showValue value={65} />
      <RingProgress intent="notice" showValue value={65} />
      <RingProgress intent="positive" showValue value={65} />
      <RingProgress intent="primary" showValue value={65} />
      <RingProgress intent="secondary" showValue value={65} />
    </>
  ),
}

export const Sizes: Story = {
  render: () => (
    <>
      <RingProgress size="xs" value={65} />
      <RingProgress size="lg" value={65} />
    </>
  ),
}
