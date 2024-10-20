import type { Meta, StoryObj } from '@storybook/react'
import { StoryFlexContainer } from '../../../storybookUtils'

import { Loader } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Loader> = {
  title: 'Loader',
  component: Loader,
  decorators: [
    (Story) => (
      <StoryFlexContainer css={{ gap: 20 }}>
        <Story />
      </StoryFlexContainer>
    ),
  ],
}
export default meta

type Story = StoryObj<typeof Loader>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const Intents: Story = {
  render: () => (
    <>
      <Loader intent="primary" />
      <Loader intent="secondary" />
      <Loader intent="positive" />
      <Loader intent="negative" />
    </>
  ),
}

export const Oval: Story = {
  render: () => (
    <>
      <Loader size="xs" />
      <Loader size="sm" />
      <Loader size="md" />
      <Loader size="lg" />
      <Loader size="xl" />
    </>
  ),
}

export const Dots: Story = {
  render: () => (
    <>
      <Loader size="xs" variant="dots" />
      <Loader size="sm" variant="dots" />
      <Loader size="md" variant="dots" />
      <Loader size="lg" variant="dots" />
      <Loader size="xl" variant="dots" />
    </>
  ),
}

export const Bars: Story = {
  render: () => (
    <>
      <Loader size="xs" variant="bars" />
      <Loader size="sm" variant="bars" />
      <Loader size="md" variant="bars" />
      <Loader size="lg" variant="bars" />
      <Loader size="xl" variant="bars" />
    </>
  ),
}
