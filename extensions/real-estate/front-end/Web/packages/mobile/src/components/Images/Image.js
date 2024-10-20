/* eslint-disable jsx-a11y/click-events-have-key-events, jsx-a11y/no-noninteractive-element-interactions */
import { ImgModal, useModal, Button } from '@willow/mobile-ui'
import { native } from 'utils'
import noop from 'utils/noop'
import styles from './Image.css'

export default function Image({
  id,
  previewUrl,
  url,
  base64,
  fileName,
  onDeleteImage = noop,
  allowRemove = false,
}) {
  const modal = useModal()

  const handleClick = () => {
    if (base64 == null && native.functionExists('showImage')) {
      native.showImage(url)
    } else if (base64 != null && native.functionExists('showImageBase64')) {
      native.showImageBase64(base64)
    } else {
      modal.open(<ImgModal src={url} name="Image Viewer" />)
    }
  }

  const handleRemove = () => {
    onDeleteImage(id)
  }

  return (
    <div className={styles.container}>
      <img
        className={styles.image}
        src={previewUrl}
        alt={fileName}
        onClick={handleClick}
      />
      {allowRemove && (
        <Button
          icon="close"
          size="small"
          tabIndex={-1}
          className={styles.close}
          onClick={handleRemove}
        />
      )}
    </div>
  )
}
