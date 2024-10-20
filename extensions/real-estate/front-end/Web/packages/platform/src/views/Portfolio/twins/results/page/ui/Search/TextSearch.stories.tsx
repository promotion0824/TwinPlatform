import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import TextSearch from './TextSearch'

const meta: Meta<typeof TextSearch> = {
  component: TextSearch,
  render: (args) => (
    <TextSearch
      useSearchResults={() => ({ t: (_) => _, changeTerm: () => {}, ...args })}
    />
  ),
}

export default meta
type Story = StoryObj<typeof TextSearch>

export const NothingSearched: Story = {}

export const TermSearched: Story = {
  args: {
    term: 'My twin',
  },
}
