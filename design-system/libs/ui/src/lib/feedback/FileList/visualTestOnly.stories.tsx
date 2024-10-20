import type { StoryObj } from '@storybook/react'

import { FileList } from '.'

const defaultStory = {
  component: FileList,
  title: 'FileList',
}

export default defaultStory

type Story = StoryObj<typeof FileList>

export const HiddenStoryName: Story = {
  render: () => <FileList />,
}
