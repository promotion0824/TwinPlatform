import type { StoryObj } from '@storybook/react'

import { Popover } from '.'

const defaultStory = {
  component: Popover,
  title: 'Popover',
}

export default defaultStory

type Story = StoryObj<typeof Popover>

export const RightStart: Story = {
  render: () => {
    return (
      <Popover position="right-start" width={100} opened withArrow>
        <Popover.Target>
          <button>right-start</button>
        </Popover.Target>
        <Popover.Dropdown>This is the content of the popover</Popover.Dropdown>
      </Popover>
    )
  },
}
