import { filesize } from 'filesize'
import styled from 'styled-components'
import { Alert } from '../../feedback/Alert'
import { FileRejection } from '../Dropzone'
import { Box } from '../../misc/Box'

export enum FileRejectionCode {
  DuplicateFile = 'duplicate-file',
  FileTooLarge = 'file-too-large',
  FileInvalidType = 'file-invalid-type',
  TooManyFiles = 'too-many-files',
}

const FileRejectionErrorMessage = {
  [FileRejectionCode.DuplicateFile]: 'File has already been uploaded',
  [FileRejectionCode.FileTooLarge]: 'File is too large',
  [FileRejectionCode.FileInvalidType]: 'Invalid file type',
  [FileRejectionCode.TooManyFiles]: 'Too many files',
}

const FileRejectionItem = styled.li(({ theme }) => ({
  color: theme.color.intent.negative.fg.default,
}))

const FileRejectionList = styled.ul(({ theme }) => ({
  margin: 0,
  paddingInlineStart: theme.spacing.s16,
}))

export default function FileUploadRejection({
  onClearRejectedFiles,
  rejectedFiles,
}: {
  onClearRejectedFiles: () => void
  rejectedFiles: FileRejection[]
}) {
  return (
    <Alert
      hasIcon
      intent="negative"
      onClose={onClearRejectedFiles}
      title={`The following file${
        rejectedFiles.length > 1 ? 's' : ''
      } could not be uploaded`}
      withCloseButton
    >
      <FileRejectionList>
        {rejectedFiles.map(({ errors, file }) => (
          <FileRejectionItem key={file.name}>
            {file.name}{' '}
            <Box component="span" c="neutral.fg.default">
              ({filesize(file.size, { standard: 'jedec' })}) -{' '}
              {FileRejectionErrorMessage[errors[0].code as FileRejectionCode]}
            </Box>
          </FileRejectionItem>
        ))}
      </FileRejectionList>
    </Alert>
  )
}
