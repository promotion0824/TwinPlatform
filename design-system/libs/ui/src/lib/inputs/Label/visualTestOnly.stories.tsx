import type { StoryObj } from '@storybook/react'

import { Label } from '.'
import { FlexDecorator } from '../../../storybookUtils'

const defaultStory = {
  component: Label,
  title: 'Label',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Label>

export const Default: Story = {
  render: () => <Label>label</Label>,
}

export const Required: Story = {
  render: () => <Label required>label</Label>,
}
