import type { Meta, StoryObj } from '@storybook/react'
import { uniqueId } from 'lodash'
import { useState } from 'react'
import { FileUpload, UploadedFile } from '.'
import { FlexDecorator } from '../../../storybookUtils'
import { Button } from '../../buttons/Button'
import { Stack } from '../../layout/Stack'

function sleep(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

const meta: Meta<typeof FileUpload> = {
  title: 'FileUpload',
  component: FileUpload,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof FileUpload>

export const Playground: Story = {
  render: () => {
    const onFileUpload = async (_: File) => {
      await sleep(2000)
      return { success: true, uploadId: uniqueId() }
    }

    return (
      <FileUpload
        acceptedFiles={['jpeg', 'png']}
        label="Upload file"
        maxFiles={3}
        maxSize={5 * 1024 * 1024}
        onFileUpload={onFileUpload}
        w={500}
      />
    )
  },
}

export const UploadErrors: Story = {
  render: () => {
    const onFileUpload = async (_: File) => {
      await sleep(2000)
      return {
        errorMessage: 'Upload failed. Something went wrong.',
        success: false,
        uploadId: uniqueId(),
      }
    }

    return (
      <FileUpload
        acceptedFiles={['jpeg', 'png']}
        label="Upload file"
        maxFiles={3}
        maxSize={5 * 1024 * 1024}
        onFileUpload={onFileUpload}
        w={500}
      />
    )
  },
}

export const FileListHeight: Story = {
  render: () => {
    const onFileUpload = async (_: File) => {
      await sleep(2000)
      return { success: true, uploadId: uniqueId() }
    }

    return (
      <FileUpload
        fileListHeight={200}
        label="Upload file"
        onFileUpload={onFileUpload}
        w={500}
      />
    )
  },
}

export const CustomDescription: Story = {
  render: () => {
    const onFileUpload = async (_: File) => {
      await sleep(2000)
      return { success: true, uploadId: uniqueId() }
    }

    return (
      <FileUpload
        acceptedFiles={['jpeg', 'png']}
        description="This is where you can upload some files."
        label="Upload file"
        maxFiles={3}
        maxSize={5 * 1024 * 1024}
        onFileUpload={onFileUpload}
        w={500}
      />
    )
  },
}

export const Required: Story = {
  render: () => {
    const onFileUpload = async (_: File) => {
      await sleep(2000)
      return { success: true, uploadId: uniqueId() }
    }

    return (
      <FileUpload
        acceptedFiles={['jpeg', 'png']}
        label="Upload file"
        maxFiles={3}
        maxSize={5 * 1024 * 1024}
        onFileUpload={onFileUpload}
        required
        w={500}
      />
    )
  },
}

export const SavingFiles: Story = {
  render: () => {
    const [files, setFiles] = useState<UploadedFile[]>([])

    const onFileUpload = async (_: File) => {
      await sleep(2000)
      return { success: true, uploadId: uniqueId() }
    }

    return (
      <Stack>
        <FileUpload
          acceptedFiles={['jpeg', 'png']}
          label="Upload file"
          maxFiles={3}
          maxSize={5 * 1024 * 1024}
          onChange={setFiles}
          onFileUpload={onFileUpload}
          required
          w={500}
        />

        <Button
          disabled={files.length === 0}
          onClick={() => {
            window.alert(
              `Saved files:\n${files
                .map(({ file }) => `- ${file.name}`)
                .join('\n')}`
            )
          }}
        >
          Save Files
        </Button>
      </Stack>
    )
  },
}
