import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import UnstyledButton from './UnstyledButton'

const meta: Meta<typeof UnstyledButton> = {
  component: UnstyledButton,
}

export default meta
type Story = StoryObj<typeof UnstyledButton>

export const Alone: Story = {
  render: () => (
    <UnstyledButton onClick={() => alert('clicked')}>
      I'm completely unstyled, but I'm actually a button
    </UnstyledButton>
  ),
}

export const WithSomethingInside: Story = {
  render: () => (
    <UnstyledButton onClick={() => alert('clicked')}>
      <input
        type="text"
        placeholder="I'm a default styled input, inside a button"
      />
    </UnstyledButton>
  ),
}
