import { useEffect, useRef } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useForm } from 'components/Form/Form'
import Progress from 'components/Progress/Progress'
import styles from './FileButton.css'

export default function FileButton({
  name,
  value,
  type,
  disabled,
  readOnly,
  accept,
  multiple,
  className,
  children,
  onChange,
  ...rest
}) {
  const form = useForm()
  const fileRef = useRef()

  const nextValue = _.get(form?.data, name) ?? null
  const nextReadOnly = disabled ?? readOnly ?? form?.readOnly

  const cxClassName = cx(
    styles.fileButton,
    {
      [styles.readOnly]: readOnly,
      [styles.disabled]: disabled,
    },
    className
  )

  useEffect(() => {
    if (nextValue != null && type === 'submit') {
      form.submit()
    }
  }, [nextValue])

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      fileRef.current.click()
    }
  }

  function handleFileClick() {
    const file = multiple
      ? Array.from(fileRef.current.files)
      : fileRef.current.files[0]

    fileRef.current.value = ''

    form?.setData((prevData) => _.set(prevData, name, file))
    form?.clearError(name)

    onChange(file)
  }

  return (
    /* eslint-disable-next-line */
    <label
      {...rest}
      tabIndex={0} // eslint-disable-line
      className={cxClassName}
      onKeyDown={handleKeyDown}
    >
      {form.type === 'submit' && form.isSubmitting && <Progress />}
      {(form.type !== 'submit' || !form.isSubmitting) && children}
      <input
        ref={fileRef}
        type="file"
        accept={accept}
        multiple={multiple ? 'multiple' : undefined}
        disabled={nextReadOnly}
        className={styles.file}
        onChange={handleFileClick}
      />
    </label>
  )
}
