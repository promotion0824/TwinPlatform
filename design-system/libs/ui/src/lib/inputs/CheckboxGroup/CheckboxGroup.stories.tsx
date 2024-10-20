import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { CheckboxGroup } from '.'
import { Checkbox } from '../Checkbox'

const meta: Meta<typeof CheckboxGroup> = {
  title: 'CheckboxGroup',
  component: CheckboxGroup,
}
export default meta

type Story = StoryObj<typeof CheckboxGroup>

export const Playground: Story = {
  render: () => (
    <CheckboxGroup label="Legend">
      <Checkbox label="Label" value="label1" />
      <Checkbox label="Label" value="label2" />
      <Checkbox label="Label" value="label3" />
    </CheckboxGroup>
  ),
}

export const NumberAsValues: Story = {
  render: () => {
    const [value, setValue] = useState<number[]>([3])
    return (
      <CheckboxGroup
        label="Legend"
        value={value}
        onChange={setValue}
        type="number"
      >
        <Checkbox label="Label" value={1} />
        <Checkbox label="Label" value={2} />
        <Checkbox label="Label" value={3} />
      </CheckboxGroup>
    )
  },
}

export const UseDataProps: Story = {
  render: () => (
    <CheckboxGroup
      label="Legend"
      defaultValue={[1]}
      onChange={(values) => {
        console.log('value selected: ', values)
      }}
      type="number"
      data={[
        { value: 1, label: 'Label1' },
        { value: 2, label: 'Label2' },
        { value: 3, label: 'Label3' },
      ]}
    />
  ),
}

export const InlineCheckboxGroup: Story = {
  render: () => (
    <CheckboxGroup label="Legend" inline>
      <Checkbox label="Label" value="label1" />
      <Checkbox label="Label" value="label2" />
      <Checkbox label="Label" value="label3" />
    </CheckboxGroup>
  ),
}

export const GroupOfInvalidCheckbox: Story = {
  render: () => {
    const [error, setError] = useState(true)

    return (
      <CheckboxGroup
        label="Legend"
        error={error}
        onChange={(values) => {
          if (values.length > 0) {
            setError(false)
          } else {
            setError(true)
          }
        }}
      >
        <Checkbox label="Label" value="label1" />
        <Checkbox label="Label" value="label2" />
        <Checkbox label="Label" value="label3" />
      </CheckboxGroup>
    )
  },
}

export const GroupOfDisabledCheckbox: Story = {
  render: () => (
    <CheckboxGroup label="Legend">
      <Checkbox label="Label" value="label1" disabled />
      <Checkbox label="Label" value="label2" disabled />
      <Checkbox label="Label" value="label3" disabled />
    </CheckboxGroup>
  ),
}

export const Description: Story = {
  render: () => (
    <CheckboxGroup label="Legend" description="CheckboxGroup description text">
      <Checkbox label="Label" value="label1" />
      <Checkbox label="Label" value="label2" />
      <Checkbox label="Label" value="label3" />
    </CheckboxGroup>
  ),
}

export const RequiredGroupOfInvalidCheckbox: Story = {
  render: () => {
    const [error, setError] = useState(true)

    return (
      <CheckboxGroup
        label="Legend"
        error={error}
        required
        onChange={(values) => {
          if (values.length > 0) {
            setError(false)
          } else {
            setError(true)
          }
        }}
      >
        <Checkbox label="Label" value="label1" />
        <Checkbox label="Label" value="label2" />
        <Checkbox label="Label" value="label3" />
      </CheckboxGroup>
    )
  },
}

export const RequiredGroupOfInvalidCheckboxWithErrorMessage: Story = {
  render: () => {
    const errorMessage = 'At least one option must be selected'
    const [error, setError] = useState<string | false>(errorMessage)

    return (
      <CheckboxGroup
        label="Legend"
        error={error}
        required
        onChange={(values) => {
          if (values.length > 0) {
            setError(false)
          } else {
            setError(errorMessage)
          }
        }}
      >
        <Checkbox label="Label" value="label1" />
        <Checkbox label="Label" value="label2" />
        <Checkbox label="Label" value="label3" />
      </CheckboxGroup>
    )
  },
}

export const HorizontalLayout: Story = {
  render: () => {
    const errorMessage = 'At least one option must be selected'
    const [error, setError] = useState<string | false>(errorMessage)

    return (
      <CheckboxGroup
        layout="horizontal"
        label="Legend"
        error={error}
        required
        onChange={(values) => {
          if (values.length > 0) {
            setError(false)
          } else {
            setError(errorMessage)
          }
        }}
      >
        <Checkbox label="Label" value="label1" />
        <Checkbox label="Label" value="label2" />
        <Checkbox label="Label" value="label3" />
      </CheckboxGroup>
    )
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  render: () => {
    const errorMessage = 'At least one option must be selected'
    const [error, setError] = useState<string | false>(errorMessage)

    return (
      <CheckboxGroup
        layout="horizontal"
        label="Legend"
        labelWidth={300}
        error={error}
        required
        onChange={(values) => {
          if (values.length > 0) {
            setError(false)
          } else {
            setError(errorMessage)
          }
        }}
      >
        <Checkbox label="Label" value="label1" />
        <Checkbox label="Label" value="label2" />
        <Checkbox label="Label" value="label3" />
      </CheckboxGroup>
    )
  },
}
