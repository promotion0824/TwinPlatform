import type { Meta, StoryObj } from '@storybook/react'

import { FileList } from '.'

const meta: Meta<typeof FileList> = {
  title: 'FileList',
  component: FileList,
}
export default meta

type Story = StoryObj<typeof FileList>

export const Playground: Story = {
  render: () => (
    <FileList>
      <FileList.File title="filename.pdf">123KB</FileList.File>
      <FileList.File title="filename.pdf">123KB</FileList.File>
      <FileList.File title="filename.pdf">123KB</FileList.File>
    </FileList>
  ),
}

export const WithBorder: Story = {
  render: () => (
    <FileList h={200} withBorder>
      <FileList.File title="filename.pdf">123KB</FileList.File>
      <FileList.File title="filename.pdf">123KB</FileList.File>
      <FileList.File title="filename.pdf">123KB</FileList.File>
      <FileList.File title="filename.pdf">123KB</FileList.File>
      <FileList.File title="filename.pdf">123KB</FileList.File>
    </FileList>
  ),
}

export const Succeed: Story = {
  render: () => (
    <FileList>
      <FileList.File title="filename.pdf" onClose={() => window.alert('close')}>
        123KB
      </FileList.File>
    </FileList>
  ),
}

export const Loading: Story = {
  render: () => (
    <FileList>
      <FileList.File
        title="filename.pdf"
        loading
        onClose={() => window.alert('close')}
      >
        Uploading...
      </FileList.File>
    </FileList>
  ),
}

export const Disabled: Story = {
  render: () => (
    <FileList>
      <FileList.File title="filename.pdf" disabled>
        123KB
      </FileList.File>
    </FileList>
  ),
}

export const Failed: Story = {
  render: () => (
    <FileList>
      <FileList.File
        title="filename.pdf"
        failed
        onRetry={() => window.alert('retry')}
        onClose={() => window.alert('close')}
      >
        File upload failed message.
      </FileList.File>
    </FileList>
  ),
}
