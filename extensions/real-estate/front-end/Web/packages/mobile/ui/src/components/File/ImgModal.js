import { useState } from 'react'
import Message from 'components/Message/Message'
import Modal from 'components/Modal/Modal'
import styles from './ImgModal.css'

export default function ImgModal({ src, name }) {
  const [isError, setIsError] = useState(false)

  return (
    <Modal
      text={name}
      modalClassName={styles.modal}
      contentClassName={styles.content}
    >
      {isError && <Message icon="error">An error has occurred</Message>}
      {!isError && (
        <img
          src={src}
          alt={name}
          className={styles.img}
          onError={() => {
            setIsError(true)
          }}
        />
      )}
    </Modal>
  )
}
