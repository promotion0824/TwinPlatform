import type { Meta, StoryObj } from '@storybook/react'

import { InputGroup } from '.'
import { Button } from '../../buttons/Button'
import { Icon } from '../../misc/Icon'
import { Menu } from '../../overlays/Menu'
import { Select } from '../Select'
import { TextInput } from '../TextInput'

const meta: Meta<typeof InputGroup> = {
  title: 'InputGroup',
  component: InputGroup,
}
export default meta

type Story = StoryObj<typeof InputGroup>

export const Playground: Story = {
  render: () => (
    <InputGroup>
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
        defaultValue={'option1'}
      />
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 100 }}>
        <Story />
      </div>
    ),
  ],
}
export const WithLabelAndDescription: Story = {
  render: () => (
    <InputGroup label="Label" description="This is a description text">
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
        defaultValue={'option1'}
      />
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 150 }}>
        <Story />
      </div>
    ),
  ],
}

export const WithLabelAndError: Story = {
  render: () => (
    <InputGroup label="Label" error="Error message">
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
        defaultValue={'option1'}
      />
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 150 }}>
        <Story />
      </div>
    ),
  ],
}

export const HorizontalLayout: Story = {
  render: () => (
    <InputGroup layout="horizontal" label="Label" error="Error message">
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
        defaultValue={'option1'}
      />
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 150 }}>
        <Story />
      </div>
    ),
  ],
}

export const HorizontalLayoutWithLabelWidth: Story = {
  render: () => (
    <InputGroup
      layout="horizontal"
      label="Label"
      labelWidth={300}
      error="Error message"
    >
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
        defaultValue={'option1'}
      />
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 150 }}>
        <Story />
      </div>
    ),
  ],
}

export const SelectPrecedingInput: Story = {
  render: () => (
    <InputGroup>
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
        defaultValue={'option1'}
      />
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 100 }}>
        <Story />
      </div>
    ),
  ],
}

export const InputWithButton: Story = {
  render: () => (
    <InputGroup>
      <TextInput
        defaultValue="Default value"
        css={{
          flexBasis: '200px',
        }}
      />
      <Button kind="secondary">Submit</Button>
    </InputGroup>
  ),
}

export const InputWithMenu: Story = {
  render: () => (
    <InputGroup>
      <TextInput
        defaultValue="Default value"
        css={{
          flexBasis: '200px',
        }}
      />
      <Menu>
        <Menu.Target>
          <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
            Toggle Menu
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item>Menu item 1</Menu.Item>
          <Menu.Item>Menu item 2</Menu.Item>
          <Menu.Item>Menu item 3</Menu.Item>
        </Menu.Dropdown>
      </Menu>
    </InputGroup>
  ),
  decorators: [
    (Story) => (
      <div css={{ height: 130 }}>
        <Story />
      </div>
    ),
  ],
}

export const DisabledGroup: Story = {
  render: () => (
    <InputGroup disabled>
      <TextInput
        defaultValue="Default value"
        css={{
          flexGrow: 1,
        }}
      />
      <Select
        data={[
          {
            label: 'option 1',
            value: 'option1',
          },
          {
            label: 'option 2',
            value: 'option2',
          },
        ]}
      />
    </InputGroup>
  ),
}

export const ReadonlyTextInput: Story = {
  render: () => {
    const defaultValue = 'FGWWXY32F53GTH'
    return (
      <InputGroup>
        <TextInput defaultValue={defaultValue} readOnly />
        <Button
          kind="secondary"
          onClick={() => {
            navigator.clipboard.writeText(defaultValue)
          }}
        >
          Copy
        </Button>
      </InputGroup>
    )
  },
}
