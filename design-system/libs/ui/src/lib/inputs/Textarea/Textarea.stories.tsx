import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Textarea } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Textarea> = {
  title: 'Textarea',
  component: Textarea,
  args: {
    label: 'Label',
    placeholder: 'Placeholder',
  },
}
export default meta

type Story = StoryObj<typeof Textarea>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const WithDefaultValue: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: 'Default Value',
    placeholder: undefined,
  },
}

export const Required: Story = {
  ...storybookAutoSourceParameters,
  args: {
    required: true,
    defaultValue: 'Default Value',
    placeholder: undefined,
  },
}

export const Readonly: Story = {
  ...storybookAutoSourceParameters,
  args: {
    readOnly: true,
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
  },
}

export const WithDescription: Story = {
  ...storybookAutoSourceParameters,
  args: {
    description: 'Description text',
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    description: 'Description text',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    labelWidth: 300,
    description: 'Description text',
  },
}

export const InvalidWithErrorMessage: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: 'Failed validation message',
  },
}

export const WithoutLabel: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: undefined,
  },
}

export const MaxLength: Story = {
  ...storybookAutoSourceParameters,
  args: {
    maxLength: 3,
  },
}

export const Resizeable: Story = {
  render: () => (
    <Textarea
      label="Label"
      defaultValue="Default text"
      css={{ textarea: { resize: 'both' } }}
    />
  ),
}

export const MinRows: Story = {
  ...storybookAutoSourceParameters,
  args: {
    minRows: 3,
  },
}

export const MaxRows: Story = {
  ...storybookAutoSourceParameters,
  args: {
    maxRows: 6,
  },
}

export const ConfigureWidth: Story = {
  render: () => (
    <Textarea
      label="Label"
      placeholder="Placeholder Text"
      css={{ width: '300px' }}
    />
  ),
  decorators: [FlexDecorator],
}
