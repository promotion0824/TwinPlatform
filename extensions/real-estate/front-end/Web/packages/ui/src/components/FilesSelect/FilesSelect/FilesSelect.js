import cx from 'classnames'
import FileButton from 'components/FileButton/FileButton'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import File from './File'
import styles from './FilesSelect.css'

export default function FilesSelect({
  name,
  error,
  value,
  disabled,
  readOnly,
  icon,
  accept,
  align,
  multiple = true,
  className,
  buttonClassName,
  buttonContentClassName,
  contentClassName,
  children,
  onChange,
  ...rest
}) {
  const cxClassName = cx(
    styles.filesSelect,
    {
      [styles.isDisabled]: disabled || readOnly,
      [styles.hasError]: error != null,
    },
    styles.className
  )
  const cxButtonClassName = cx(
    styles.fileButton,
    {
      [styles.hasValue]: value.length > 0,
    },
    buttonClassName
  )
  const cxButtonContentClassName = cx(
    styles.buttonContent,
    buttonContentClassName
  )
  const cxContentClassName = cx(styles.content, contentClassName)

  function handleFileChange(nextFiles) {
    const formattedNextFiles = multiple ? nextFiles : [nextFiles]
    const nextFilesSelectValue = multiple
      ? [...value, ...formattedNextFiles]
      : formattedNextFiles

    onChange(nextFilesSelectValue)
  }

  function handleDeleteClick(file) {
    const nextFilesSelectValue = value.filter((prevFile) => prevFile !== file)

    onChange(nextFilesSelectValue)
  }

  return (
    <Flex {...rest} className={cxClassName}>
      <Flex align={align}>
        <FileButton
          accept={accept}
          multiple={multiple}
          disabled={disabled}
          readOnly={readOnly}
          onChange={handleFileChange}
          className={cxButtonClassName}
        >
          <Flex
            horizontal
            align="center middle"
            className={cxButtonContentClassName}
          >
            {icon != null && <Icon icon={icon} className={styles.icon} />}
            {children}
          </Flex>
        </FileButton>
      </Flex>
      {value.length > 0 && (
        <Flex className={cxContentClassName}>
          {value.map((file, i) => (
            <File
              key={`${file.name}-${i}`} // eslint-disable-line
              file={file}
              readOnly={readOnly}
              onClick={() => handleDeleteClick(file)}
            >
              {file.name}
            </File>
          ))}
        </Flex>
      )}
    </Flex>
  )
}
