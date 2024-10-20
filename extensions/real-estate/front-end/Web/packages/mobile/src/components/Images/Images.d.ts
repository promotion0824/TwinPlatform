import { ReactElement } from 'react'

export type ImageFile = {
  id: string | number
  file?: File
  previewUrl?: string
  url: string
  fileName: string
  base64?: string
  [x: unknown]: unknown
}

export default function Images(props: {
  addImageText?: string
  allowAdd?: boolean
  /** Error message */
  error?: string
  onAddImage?: (image: ImageFile) => void
  onDeleteImage?: (imageId: string) => void
  images?: ImageFile[]
  label?: string
}): ReactElement
