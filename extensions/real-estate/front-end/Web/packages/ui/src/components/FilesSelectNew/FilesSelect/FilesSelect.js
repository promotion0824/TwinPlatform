import { useRef } from 'react'
import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import { useTranslation } from 'react-i18next'
import IconNew from 'components/IconNew/Icon'
import File from './File'
import styles from './FilesSelect.css'

export default function FilesSelect({ readOnly, disabled, value, onChange }) {
  const fileRef = useRef()
  const { t } = useTranslation()

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      fileRef.current.click()
    }
  }

  function handleChange() {
    const files = Array.from(fileRef.current.files)
    fileRef.current.value = ''

    onChange([...value, ...files])
  }

  const cxClassName = cx({
    [styles.readOnly]: readOnly,
  })

  return (
    <Flex size="large" className={cxClassName}>
      {value.length > 0 && (
        <Flex size="small">
          {value.map((file, i) => (
            <File
              key={i} // eslint-disable-line
              file={file}
              readOnly={readOnly}
              disabled={disabled}
              onRemoveClick={() => {
                onChange(value.filter((item, prevI) => prevI !== i))
              }}
            />
          ))}
        </Flex>
      )}
      {/* eslint-disable-next-line */}
      <label
        tabIndex={0} // eslint-disable-line
        className={styles.label}
        onKeyDown={handleKeyDown}
      >
        <span className={styles.labelContent}>
          {t('plainText.uploadImage')}
        </span>
        <IconNew icon="upload" size="small" />
        <input
          ref={fileRef}
          type="file"
          accept=".png,.jpg,.jpeg"
          multiple="multiple"
          disabled={readOnly || disabled}
          className={styles.input}
          onChange={handleChange}
        />
      </label>
    </Flex>
  )
}
