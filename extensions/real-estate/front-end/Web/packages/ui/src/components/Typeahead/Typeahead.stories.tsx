import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'

import Typeahead from './Typeahead'
import TypeaheadButton from './Typeahead/TypeaheadButton'

const meta: Meta<typeof Typeahead> = {
  component: Typeahead,
  render: ({ value, ...args }) => {
    const [val, setVal] = useState(value)
    return (
      <Typeahead value={val} onChange={setVal} noFetch preservePlaceholder>
        <TypeaheadButton key="key1" value="key1">
          Key 1
        </TypeaheadButton>
        <TypeaheadButton key="key2" value="key2">
          Key 2
        </TypeaheadButton>
      </Typeahead>
    )
  },
}

export default meta
type Story = StoryObj<typeof Typeahead>

export const Empty: Story = {
  args: {
    label: 'Select',
  },
}

export const Placeholder: Story = {
  args: {
    label: 'Select with placeholder',
    placeholder: 'Select placeholder',
  },
}

export const Value: Story = {
  args: {
    label: 'Select with value',
    value: 'Key 1',
  },
}
