import { useEffect, useState } from 'react'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import IconNew from 'components/IconNew/Icon'
import FileDescription from './FileDescription'
import ImgModal from './ImgModal'
import styles from './File.css'

export default function File({ file, readOnly, disabled, onRemoveClick }) {
  const [src, setSrc] = useState()
  const [error, setError] = useState()
  const [showModal, setShowModal] = useState(false)

  useEffect(() => {
    if (file.url != null) {
      setSrc(file.url)
      return
    }

    const reader = new FileReader()
    reader.onload = (e) => {
      setSrc(e.target.result)
    }
    reader.readAsDataURL(file)
  }, [])

  return (
    <>
      <Flex
        horizontal
        fill="content"
        padding="large"
        size="large"
        className={styles.file}
      >
        <div className={styles.imgContainer}>
          {error != null && <Icon icon="error" />}
          {error == null && src == null && (
            <IconNew icon="image" size="small" />
          )}
          {error == null && src != null && (
            <img // eslint-disable-line
              src={src}
              alt={file.name}
              className={styles.img}
              onClick={() => setShowModal(true)}
              onError={() => setError(true)}
            />
          )}
        </div>
        <FileDescription file={file} />
        {!readOnly && !disabled && (
          <Button
            icon="cross"
            className={styles.close}
            onClick={onRemoveClick}
          />
        )}
      </Flex>
      {showModal && (
        <ImgModal file={file} src={src} onClose={() => setShowModal(false)} />
      )}
    </>
  )
}
