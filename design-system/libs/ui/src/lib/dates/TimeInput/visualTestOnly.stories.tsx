import type { Meta, StoryObj } from '@storybook/react'

import { TimeInput } from '.'
import { storyContainerTestId } from '../../../storybookUtils/StoryContainers'

const defaultStory: Meta<typeof TimeInput> = {
  component: TimeInput,
  title: 'TimeInput',
  decorators: [
    (Story) => (
      <div
        data-testid={storyContainerTestId}
        css={{
          height: 300,
          width: 200,
        }}
      >
        <Story />
      </div>
    ),
  ],
}

export default defaultStory

type Story = StoryObj<typeof TimeInput>

export const HiddenWithLabelAndError: Story = {
  args: {
    label: 'Label',
    error: 'Error Message',
  },
}

export const HiddenIn24HoursWithSeconds: Story = {
  args: {
    format: 'HH:mm:ss',
  },
}
