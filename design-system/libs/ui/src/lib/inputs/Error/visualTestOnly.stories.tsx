import type { StoryObj } from '@storybook/react'

import { Error } from '.'
import { FlexDecorator } from '../../../storybookUtils'

const defaultStory = {
  component: Error,
  title: 'Error',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Error>

export const Default: Story = {
  render: () => <Error>Error message</Error>,
}
