import type { Meta, StoryObj } from '@storybook/react'
import { noop } from 'lodash'
import { useState } from 'react'
import { Dropzone, FileRejection, FileWithPath } from '.'
import { FlexDecorator } from '../../../storybookUtils'
import { Stack } from '../../layout/Stack'

const meta: Meta<typeof Dropzone> = {
  title: 'Dropzone',
  component: Dropzone,
  decorators: [FlexDecorator],
}

export default meta

type Story = StoryObj<typeof Dropzone>

export const Playground: Story = {
  render: () => <Dropzone label="Upload file" onDrop={noop} w={500} />,
}

export const Loading: Story = {
  render: () => <Dropzone label="Upload file" loading onDrop={noop} w={500} />,
}

export const Disabled: Story = {
  render: () => <Dropzone disabled label="Upload file" onDrop={noop} w={500} />,
}

export const Invalid: Story = {
  render: () => <Dropzone invalid label="Upload file" onDrop={noop} w={500} />,
}

export const FileRestrictions: Story = {
  render: () => (
    <Dropzone
      acceptedFiles={['jpeg', 'png']}
      label="Upload file"
      maxFiles={1}
      maxSize={500 * 1024}
      onDrop={noop}
      w={500}
    />
  ),
}

export const ReceivingFiles: Story = {
  render: () => {
    const [files, setFiles] = useState<FileWithPath[]>([])
    const [fileRejections, setFileRejections] = useState<FileRejection[]>([])

    const reset = () => {
      setFiles([])
      setFileRejections([])
    }

    return (
      <Stack>
        <Dropzone
          acceptedFiles={['jpeg', 'png']}
          label="Upload file"
          maxFiles={1}
          maxSize={500 * 1024}
          onDrop={setFiles}
          onDropAny={reset}
          onFileDialogOpen={reset}
          onReject={setFileRejections}
          w={500}
        />

        {files.length > 0 && (
          <>
            <div>Accepted:</div>
            <ul>
              {files.map((file) => (
                <li key={file.name}>{file.name}</li>
              ))}
            </ul>
          </>
        )}

        {fileRejections.length > 0 && (
          <>
            <div>Rejected:</div>
            <ul>
              {fileRejections.map((rejection) => (
                <li key={rejection.file.name}>
                  {rejection.file.name}
                  <ul>
                    {rejection.errors.map((error) => (
                      <li key={error.code}>{error.message}</li>
                    ))}
                  </ul>
                </li>
              ))}
            </ul>
          </>
        )}
      </Stack>
    )
  },
}
