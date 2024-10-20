import type { StoryObj } from '@storybook/react'
import { noop } from 'lodash'
import { Dropzone } from '.'
import { FlexDecorator } from '../../../storybookUtils'

const defaultStory = {
  component: Dropzone,
  title: 'Dropzone',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof Dropzone>

export const Accept: Story = {
  render: () => (
    <Dropzone data-accept label="Upload file" onDrop={noop} w={500} />
  ),
}

export const Reject: Story = {
  render: () => (
    <Dropzone data-reject label="Upload file" onDrop={noop} w={500} />
  ),
}
