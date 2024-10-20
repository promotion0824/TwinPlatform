import type { Meta, StoryObj } from '@storybook/react'

import { useState } from 'react'
import { Icon } from '../../misc/Icon'
import { TextInput } from './TextInput'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof TextInput> = {
  title: 'TextInput',
  component: TextInput,
  args: {
    label: 'Label',
    placeholder: 'Placeholder Text',
  },
}
export default meta

type Story = StoryObj<typeof TextInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
}

export const WithDefaultValue: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: 'Value Text',
  },
}

export const Controlled: Story = {
  render: () => {
    const [value, setValue] = useState('')

    return (
      <TextInput
        value={value}
        onChange={(event) => setValue(event.currentTarget.value)}
        label="Label"
      />
    )
  },
}

export const Required: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: 'Value Text',
    required: true,
  },
}

export const Invalid: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: true,
  },
}

export const Readonly: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: 'Value Text',
    readOnly: true,
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
  },
}

export const WithPrefixAndSuffix: Story = {
  ...storybookAutoSourceParameters,
  args: {
    prefix: <Icon icon="info" />,
    suffix: <Icon icon="search" />,
  },
}

export const Clearable: Story = {
  ...storybookAutoSourceParameters,
  args: {
    clearable: true,
    defaultValue: 'Clearable text',
  },
}

export const WithoutLabel: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: undefined,
  },
}

export const WithDescription: Story = {
  ...storybookAutoSourceParameters,
  args: {
    description: 'Description Text',
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    description: 'Description Text',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    labelWidth: 300,
    description: 'Description Text',
  },
}

export const InvalidWithErrorMessage: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: 'Failed validation message',
    required: true,
  },
}
