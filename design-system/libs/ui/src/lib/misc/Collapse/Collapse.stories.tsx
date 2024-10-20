import type { Meta, StoryObj } from '@storybook/react'

import { Collapse } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'
import { Checkbox } from '../../inputs/Checkbox'
import { CheckboxGroup } from '../../inputs/CheckboxGroup'
import { Group } from '../../layout/Group'
import { Icon } from '../Icon'

const meta: Meta<typeof Collapse> = {
  title: 'Collapse',
  component: Collapse,
}
export default meta

type Story = StoryObj<typeof Collapse>

const ContentText = () => (
  <p>
    Lorem Ipsum is simply dummy text of the printing and typesetting industry.
    Lorem Ipsum has been the industry's standard dummy text ever since the
    1500s, when an unknown printer took a galley of type and scrambled it to
    make a type specimen book. It has survived not only five centuries, but also
    the leap into electronic typesetting, remaining essentially unchanged. It
    was popularised in the 1960s with the release of Letraset sheets containing
    Lorem Ipsum passages, and more recently with desktop publishing software
    like Aldus PageMaker including versions of Lorem Ipsum.
  </p>
)

export const Playground: Story = {
  render: () => {
    const [opened, { toggle }] = useDisclosure(false)

    return (
      <div>
        <Button onClick={toggle}>Toggle content</Button>

        <Collapse opened={opened}>
          <ContentText />
        </Collapse>
      </div>
    )
  },
}

export const CollapsibleFilter: Story = {
  render: () => {
    const [opened, { toggle }] = useDisclosure(false)

    return (
      <div>
        <Group onClick={toggle} css={{ cursor: 'pointer' }}>
          Toggle content
          <Icon icon={opened ? 'keyboard_arrow_up' : 'keyboard_arrow_down'} />
        </Group>

        <Collapse opened={opened}>
          <CheckboxGroup label="Status">
            <Checkbox label="Opened" value="opened" />
            <Checkbox label="Closed" value="closed" />
          </CheckboxGroup>
        </Collapse>
      </div>
    )
  },
}

export const NestedCollapse: Story = {
  render: () => {
    const [openedOutside, { toggle: toggleOutside }] = useDisclosure(false)
    const [openedInside, { toggle: toggleInside }] = useDisclosure(false)

    return (
      <div>
        <Button onClick={toggleOutside}>Toggle content</Button>

        <Collapse opened={openedOutside}>
          <ContentText />
          <div>
            <Button onClick={toggleInside}>Toggle nested content</Button>
            <Collapse opened={openedInside}>
              <ContentText />
            </Collapse>
          </div>
        </Collapse>
      </div>
    )
  },
}
