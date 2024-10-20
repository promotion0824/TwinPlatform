import {
  DropzoneProps as MantineDropzoneProps,
  MIME_TYPES,
} from '@mantine/dropzone'
import { filesize } from 'filesize'

/** Returns a string describing the file restrictions for the dropzone. */
export default function dropzoneDescription({
  acceptedFiles,
  maxFiles,
  maxSize,
}: {
  acceptedFiles?: (keyof typeof MIME_TYPES)[]
  maxFiles?: MantineDropzoneProps['maxFiles']
  maxSize?: MantineDropzoneProps['maxSize']
}): string {
  const maxFilesText = maxFiles
    ? `Maximum ${maxFiles} file${maxFiles > 1 ? 's' : ''}. `
    : ''

  const acceptedFilesText = acceptedFiles
    ? `Accepts: ${acceptedFiles.join(', ')}. `
    : ''

  const maxSizeText = maxSize
    ? `Max file size: ${filesize(maxSize, { standard: 'jedec' })}.`
    : ''

  return `${maxFilesText}${acceptedFilesText}${maxSizeText}`.trim()
}
