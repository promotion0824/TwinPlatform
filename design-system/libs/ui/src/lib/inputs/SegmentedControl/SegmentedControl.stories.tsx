import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { SegmentedControl } from '.'
import { useState } from 'react'

const meta: Meta<typeof SegmentedControl> = {
  title: 'SegmentedControl',
  component: SegmentedControl,
}
export default meta

type Story = StoryObj<typeof SegmentedControl>

export const Playground: Story = {
  render: () => (
    <SegmentedControl
      data={[
        { value: 'preview_val', label: 'Preview' },
        { value: 'code_val', label: 'Code' },
        { value: 'export_val', label: 'Export' },
      ]}
    />
  ),
  decorators: [FlexDecorator],
}

export const Controlled: Story = {
  render: () => {
    const [value, setValue] = useState('export')

    return (
      <SegmentedControl
        value={value}
        onChange={setValue}
        data={[
          { value: 'preview_val', label: 'Preview' },
          { value: 'code_val', label: 'Code' },
          { value: 'export_val', label: 'Export' },
        ]}
      />
    )
  },
}

export const IconOnly: Story = {
  render: () => (
    <SegmentedControl
      data={[
        {
          value: 'preview',
          iconName: 'visibility',
          label: 'Preview',
          iconOnly: true,
        },
        {
          value: 'code',
          iconName: 'code',
          label: 'Code',
          iconOnly: true,
        },
        {
          value: 'export',
          iconName: 'open_in_new',
          label: 'Export',
          iconOnly: true,
        },
      ]}
    />
  ),
  decorators: [FlexDecorator],
}

export const WithPrefix: Story = {
  render: () => (
    <SegmentedControl
      data={[
        {
          value: 'preview',
          iconName: 'visibility',
          label: 'Preview',
        },
        {
          value: 'code',
          iconName: 'code',
          label: 'Code',
        },
        {
          value: 'export',
          iconName: 'open_in_new',
          label: 'Export',
        },
      ]}
    />
  ),
  decorators: [FlexDecorator],
}

export const Disabled: Story = {
  render: () => (
    <SegmentedControl
      disabled
      data={[
        { value: 'preview_val', label: 'Preview' },
        { value: 'code_val', label: 'Code' },
        { value: 'export_val', label: 'Export' },
      ]}
    />
  ),
  decorators: [FlexDecorator],
}

export const DisabledControl: Story = {
  render: () => (
    <SegmentedControl
      data={[
        {
          value: 'preview',
          label: 'Preview',
          iconName: 'visibility',
          disabled: true,
        },
        {
          value: 'code',
          label: 'Code',
          iconName: 'code',
        },
        {
          value: 'export',
          label: 'Export',
          iconName: 'open_in_new',
        },
      ]}
    />
  ),
  decorators: [FlexDecorator],
}

export const Vertical: Story = {
  render: () => (
    <SegmentedControl
      orientation="vertical"
      data={[
        {
          value: 'preview',
          label: 'Preview',
          iconName: 'visibility',
        },
        {
          value: 'code',
          label: 'Code',
          iconName: 'code',
        },
        {
          value: 'export',
          label: 'Export',
          iconName: 'open_in_new',
        },
      ]}
    />
  ),
  decorators: [FlexDecorator],
}

export const FullWidth: Story = {
  render: () => (
    <SegmentedControl
      fullWidth
      data={[
        {
          value: 'preview',
          label: 'Preview',
          iconName: 'visibility',
        },
        {
          value: 'code',
          label: 'Code',
          iconName: 'code',
        },
        {
          value: 'export',
          label: 'Export',
          iconName: 'open_in_new',
        },
      ]}
    />
  ),
}

/**
 * You can use the string format of the `data` property when you want the value and label to be the same.
 * This provides a simpler way to set up the SegmentedControl when both attributes are identical.
 */

export const StringData: Story = {
  render: () => <SegmentedControl data={['Preview', 'Code', 'Export']} />,
  decorators: [FlexDecorator],
}
