import { useEffect, useState } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import File from 'components/File/File'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import Text from 'components/Text/Text'
import styles from './File.css'

export default function FileComponent({ file, readOnly, children, onClick }) {
  const [src, setSrc] = useState()

  const cxClassName = cx(styles.file, {
    [styles.readOnly]: readOnly,
  })

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
    <Flex
      horizontal
      fill="content"
      align="middle"
      size="medium"
      padding="medium"
      className={cxClassName}
    >
      {src != null ? (
        <File src={src} name={children} size="small" />
      ) : (
        <div className={styles.img}>
          <Icon icon="progress" />
        </div>
      )}
      <Text className={styles.text}>{children}</Text>
      {!readOnly && (
        <Button icon="close" className={styles.delete} onClick={onClick} />
      )}
    </Flex>
  )
}
