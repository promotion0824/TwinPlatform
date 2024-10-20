import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { InputGroup } from '.'
import { Button } from '../../buttons/Button'
import { Menu } from '../../overlays/Menu'
import { Select } from '../Select'
import { TextInput } from '../TextInput'

const defaultStory = {
  component: InputGroup,
  title: 'InputGroup',
}

export default defaultStory

type Story = StoryObj<typeof InputGroup>

export const MultipleChildren: Story = {
  render: () => (
    <InputGroup>
      <TextInput defaultValue="default" />

      <Select
        data={[
          {
            label: 'Kg',
            value: 'kg',
          },
          {
            label: 'gram',
            value: 'gram',
          },
        ]}
      />
      <Button>button</Button>
      <Menu>
        <Menu.Target>
          <Button kind="secondary">Toggle Menu</Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item>Menu item 1</Menu.Item>
        </Menu.Dropdown>
      </Menu>
    </InputGroup>
  ),
  decorators: [FlexDecorator],
}
