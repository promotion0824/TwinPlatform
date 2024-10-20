import type { Meta, StoryObj } from '@storybook/react'
import styled from 'styled-components'

import { Progress } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const ProgressDecoratorContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  width: '400px',
}))

const ProgressDecorator = (Story: React.ComponentType) => (
  <ProgressDecoratorContainer data-testId="story-container">
    <Story />
  </ProgressDecoratorContainer>
)

const meta: Meta<typeof Progress> = {
  title: 'Progress',
  component: Progress,
  decorators: [ProgressDecorator],
}
export default meta

type Story = StoryObj<typeof Progress>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    value: 65,
  },
}

export const Intents: Story = {
  render: () => (
    <>
      <Progress intent="negative" value={65} />
      <Progress intent="notice" value={65} />
      <Progress intent="positive" value={65} />
      <Progress intent="primary" value={65} />
      <Progress intent="secondary" value={65} />
    </>
  ),
}

export const Sizes: Story = {
  render: () => (
    <>
      <Progress size="xs" value={65} />
      <Progress size="sm" value={65} />
      <Progress size="md" value={65} />
      <Progress size="lg" value={65} />
    </>
  ),
}
