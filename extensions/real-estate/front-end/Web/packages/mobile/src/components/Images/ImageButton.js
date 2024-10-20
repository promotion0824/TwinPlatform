import { useRef } from 'react'
import cx from 'classnames'
import { Icon, Spacing } from '@willow/mobile-ui'

import styles from './ImageButton.css'

export default function ImageButton({ error, text, onAddImage }) {
  const fileRef = useRef()

  const handleFileClick = () => {
    if (onAddImage) {
      const file = fileRef.current.files[0]

      const reader = new FileReader()
      reader.onload = () => {
        const url = URL.createObjectURL(file)
        const base64 = btoa(reader.result)

        onAddImage({
          id: Date.now(),
          file,
          previewUrl: url,
          url,
          fileName: file.name,
          base64,
        })

        // Not quite sure how this gets to be null
        if (fileRef.current != null) {
          fileRef.current.value = ''
        }
      }
      reader.readAsBinaryString(file)
    }
  }

  const showFileDialog = () => {
    fileRef.current.click()
  }

  const containerClassName = cx(styles.container, {
    [styles.error]: error,
  })

  return (
    <>
      {error && <span className={styles.errorText}>{error}</span>}
      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
      <div className={containerClassName} onClick={showFileDialog}>
        <Spacing horizontal>
          <Icon icon="attachment" className={styles.icon} />
          <Icon icon="camera" className={styles.icon} />
        </Spacing>
        <p className={styles.text}>{text}</p>
        <input
          ref={fileRef}
          type="file"
          accept=".png,.jpg,.jpeg"
          className={styles.file}
          onChange={handleFileClick}
          onClick={(event) => {
            // Avoid doubling up on showing the file dialog
            event.stopPropagation()
          }}
        />
      </div>
    </>
  )
}
