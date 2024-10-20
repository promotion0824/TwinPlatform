import { filesize } from 'filesize'
import { forwardRef, useEffect, useState } from 'react'
import { FileList } from '../../feedback/FileList'
import { Stack } from '../../layout/Stack'
import { Dropzone, DropzoneProps, FileRejection } from '../Dropzone'
import FileUploadRejection, { FileRejectionCode } from './FileUploadRejections'
import { WillowStyleProps } from '../../utils'

type FileWithDetails = {
  errorMessage?: string
  file: File
  loading: boolean
  uploadFailed?: boolean
  uploadId?: string
}

export type UploadedFile = {
  file: File
  uploadId: string
}

export interface FileUploadProps
  extends WillowStyleProps,
    Pick<
      DropzoneProps,
      | 'acceptedFiles'
      | 'description'
      | 'label'
      | 'maxFiles'
      | 'maxSize'
      | 'required'
    > {
  /** If set, the File List will be displayed in a container with a constrained height. */
  fileListHeight?: number
  /** Called whenever the list of uploaded files changes. */
  onChange?: (files: UploadedFile[]) => void
  /** Called when each file is initially uploaded. */
  onFileUpload: (file: File) => Promise<{
    /** If this process fails, an error message should be provided. */
    errorMessage?: string
    /** Whether the file was uploaded successfully. */
    success: boolean
    /** The unique ID of this upload process. */
    uploadId: string
  }>
}

/** `FileUpload` is used to receive and upload files from users. */
export const FileUpload = forwardRef<HTMLDivElement, FileUploadProps>(
  (
    {
      acceptedFiles,
      description,
      fileListHeight,
      label,
      maxFiles,
      maxSize,
      onChange,
      onFileUpload,
      required,
      ...restProps
    },
    ref
  ) => {
    const [files, setFiles] = useState<FileWithDetails[]>([])
    const [rejectedFiles, setRejectedFiles] = useState<FileRejection[]>([])
    const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([])

    const fileAlreadyUploaded = (newFile: File) => {
      return files.some(
        ({ file }) =>
          file.lastModified === newFile.lastModified &&
          file.name === newFile.name &&
          file.size === newFile.size &&
          file.type === newFile.type
      )
    }

    const rejectFile = (file: File, rejectionCode: FileRejectionCode) => {
      setRejectedFiles((prevRejectedFiles) => [
        ...prevRejectedFiles,
        {
          file,
          errors: [
            {
              code: rejectionCode,
              message: '',
            },
          ],
        },
      ])
    }

    const removeFile = (file: File) => {
      setFiles((prevFiles) =>
        prevFiles.filter((prevFile) => prevFile.file !== file)
      )
    }

    const retryFile = (file: File) => {
      setFiles((prevFiles) =>
        prevFiles.map((prevFile) =>
          prevFile.file === file ? { file, loading: true } : prevFile
        )
      )

      uploadFile(file)
    }

    const uploadFile = async (file: File) => {
      const { errorMessage, success, uploadId } = await onFileUpload(file)

      setFiles((prevFiles) =>
        prevFiles.map((prevFile) =>
          prevFile.file === file
            ? {
                errorMessage,
                file,
                loading: false,
                uploadFailed: !success,
                uploadId,
              }
            : prevFile
        )
      )
    }

    useEffect(() => {
      setUploadedFiles(
        files.flatMap<UploadedFile>(({ file, uploadId }) =>
          uploadId ? [{ file, uploadId }] : []
        )
      )
    }, [files])

    useEffect(() => {
      onChange?.(uploadedFiles)
    }, [onChange, uploadedFiles])

    return (
      <Stack {...restProps} ref={ref}>
        <Dropzone
          acceptedFiles={acceptedFiles}
          description={description}
          disabled={maxFiles !== undefined && files.length >= maxFiles}
          label={label}
          maxFiles={maxFiles}
          maxSize={maxSize}
          onDrop={(newFiles) => {
            let fileCount = files.length
            setRejectedFiles([])

            for (const file of newFiles) {
              if (fileAlreadyUploaded(file)) {
                rejectFile(file, FileRejectionCode.DuplicateFile)
              } else if (maxFiles !== undefined && fileCount >= maxFiles) {
                rejectFile(file, FileRejectionCode.TooManyFiles)
              } else {
                fileCount++
                uploadFile(file)
                setFiles((prevFiles) => [...prevFiles, { file, loading: true }])
              }
            }
          }}
          onReject={setRejectedFiles}
          required={required}
        />

        {rejectedFiles.length > 0 && (
          <FileUploadRejection
            onClearRejectedFiles={() => setRejectedFiles([])}
            rejectedFiles={rejectedFiles}
          />
        )}

        {files.length > 0 && (
          <FileList
            h={fileListHeight}
            withBorder={fileListHeight !== undefined}
          >
            {files.map(({ errorMessage, file, loading, uploadFailed }) => (
              <FileList.File
                failed={uploadFailed}
                key={file.name}
                loading={loading}
                onClose={() => removeFile(file)}
                onRetry={() => retryFile(file)}
                title={file.name}
              >
                {uploadFailed && errorMessage
                  ? errorMessage
                  : filesize(file.size, { standard: 'jedec' })}
              </FileList.File>
            ))}
          </FileList>
        )}
      </Stack>
    )
  }
)
