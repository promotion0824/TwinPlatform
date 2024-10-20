import { useState } from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Radio } from '../Radio'
import { RadioGroup } from '.'

const meta: Meta<typeof RadioGroup> = {
  title: 'RadioGroup',
  component: RadioGroup,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof RadioGroup>

export const Playground: Story = {
  render: () => (
    <RadioGroup label="Search Type">
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}

export const NoLabel: Story = {
  render: () => (
    <RadioGroup>
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}

export const DefaultValue: Story = {
  render: () => (
    <RadioGroup defaultValue="files" label="Search Type">
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}

export const Disabled: Story = {
  render: () => (
    <RadioGroup label="Search Type">
      <Radio disabled label="Twins" value="twins" />
      <Radio disabled label="Files" value="files" />
    </RadioGroup>
  ),
}

export const WithDescription: Story = {
  render: () => (
    <RadioGroup description="Select the type of search" label="Search Type">
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}

export const HorizontalLayout: Story = {
  render: () => (
    <RadioGroup
      layout="horizontal"
      description="Select the type of search"
      label="Search Type"
    >
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}

export const HorizontalLayoutWithLabelWidth: Story = {
  render: () => (
    <RadioGroup
      layout="horizontal"
      description="Select the type of search"
      label="Search Type"
      labelWidth={300}
    >
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}

export const InvalidWithError: Story = {
  render: () => {
    const [error, setError] = useState<string | undefined>(
      'An option must be selected'
    )

    return (
      <RadioGroup
        error={error}
        label="Search Type"
        onChange={() => {
          setError(undefined)
        }}
        required
      >
        <Radio label="Twins" value="twins" />
        <Radio label="Files" value="files" />
      </RadioGroup>
    )
  },
}

export const Inline: Story = {
  render: () => (
    <RadioGroup inline label="Search Type">
      <Radio label="Twins" value="twins" />
      <Radio label="Files" value="files" />
    </RadioGroup>
  ),
}
