import { useState, useEffect } from 'react'
import { useApi } from '@willow/mobile-ui'
import Images from 'components/Images/Images'

export default function InspectionCheckAttachments({
  error,
  siteId,
  checkRecord,
  refSubmitImageRequest,
  allowAdd = false,
}) {
  const api = useApi()
  const [removedImageIds, setRemovedImageIds] = useState([])
  const [addedImages, setAddedImages] = useState([])

  const imagesWithoutRemoved = [
    ...(checkRecord.attachments ?? []).filter(
      (image) => removedImageIds.indexOf(image.id) === -1
    ),
  ]

  const images = [...imagesWithoutRemoved, ...addedImages]

  const removeImage = (imageId) => {
    const index = addedImages.findIndex((image) => image.id === imageId)

    if (index !== -1) {
      addedImages.splice(index, 1)

      setAddedImages([...addedImages])
    } else {
      setRemovedImageIds([...removedImageIds, imageId])
    }
  }

  const addImage = (image) => {
    setAddedImages([...addedImages, image])
  }

  const submitRequest = async () => {
    const removeImagesPromise = Promise.all(
      removedImageIds.map(async (imageId) =>
        api.delete(
          `/api/sites/${siteId}/checkRecords/${checkRecord.id}/attachments/${imageId}`
        )
      )
    )

    const nextAddedImages = await Promise.all(
      addedImages.map(async (image) =>
        api.post(
          `/api/sites/${siteId}/checkRecords/${checkRecord.id}/attachments`,
          {
            fileName: image.fileName,
            attachmentFile: image.file,
          },
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          }
        )
      )
    )

    // wait for both async are ready
    await removeImagesPromise

    return [...imagesWithoutRemoved, ...nextAddedImages]
  }

  useEffect(() => {
    if (refSubmitImageRequest) {
      // eslint-disable-next-line no-param-reassign
      refSubmitImageRequest.current = submitRequest
    }
  }, [submitRequest])

  return (
    <Images
      error={error}
      images={images}
      onAddImage={addImage}
      onDeleteImage={removeImage}
      allowAdd={allowAdd}
      addImageText="Add attachments or photos"
    />
  )
}
