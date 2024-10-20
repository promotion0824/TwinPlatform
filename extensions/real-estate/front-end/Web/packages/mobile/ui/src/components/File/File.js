import { useState } from 'react'
import cx from 'classnames'
import { useModal } from 'components/Modal/Modal'
import Button from 'components/Button/Button'
import Icon from 'components/Icon/Icon'
import ImgModal from './ImgModal'
import styles from './File.css'

export default function Img({
  size = 'medium',
  src,
  name,
  className,
  imgClassName,
  ...rest
}) {
  const modal = useModal()

  const [showImage, setShowImage] = useState(false)

  const isImage = ['png', 'gif', 'jpg', 'jpeg'].includes(
    name?.split('.').slice(-1)[0]
  )

  const cxClassName = cx(
    styles.img,
    {
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.showImage]: showImage,
    },
    className
  )
  const cxImgClassName = cx(styles.imgTag, imgClassName)

  function handleClick() {
    if (showImage) {
      modal.open(<ImgModal src={src} name={name} />)
    }
  }

  return (
    <Button {...rest} className={cxClassName} onClick={handleClick}>
      {isImage && (
        <img
          src={src}
          alt={name}
          className={cxImgClassName}
          onLoad={() => setShowImage(true)}
        />
      )}
      <Icon icon="file" size="large" />
    </Button>
  )
}
