import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'

import FilesSelect from './FilesSelect'

const file = new File(['foo'], 'foo.txt', {
  type: 'text/plain',
})

const meta: Meta<typeof FilesSelect> = {
  component: FilesSelect,
  render: ({ value, ...args }) => {
    const [files, setFiles] = useState(value)

    return (
      <FilesSelect
        label="Upload files"
        value={files}
        onChange={setFiles}
        {...args}
      />
    )
  },
  decorators: [
    (Story) => (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <Story />
      </div>
    ),
  ],
}

export default meta
type Story = StoryObj<typeof FilesSelect>

// default states

export const Empty: Story = {
  args: {
    label: 'FileSelectNew',
  },
}

export const Value: Story = {
  args: {
    label: 'FileSelectNew with value',
    value: [file],
  },
}

// default readonly

export const ReadonlyEmpty: Story = {
  args: {
    label: 'Readonly FileSelectNew',
    readOnly: true,
  },
}

export const ReadonlyValue: Story = {
  args: {
    label: 'Readonly FileSelectNew with value',
    readOnly: true,
    value: [file],
  },
}

// disabled states

export const DisabledEmpty: Story = {
  args: {
    label: 'Disabled FileSelectNew',
    disabled: true,
  },
}

export const DisabledValue: Story = {
  args: {
    label: 'Disabled FileSelectNew with value',
    disabled: true,
    value: [file],
  },
}

// error states

export const ErrorEmpty: Story = {
  args: {
    error: 'Errored FileSelectNew',
  },
}

export const ErrorValue: Story = {
  args: {
    error: 'Errored FileSelectNew with value',
    value: [file],
  },
}
