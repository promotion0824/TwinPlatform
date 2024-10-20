import cx from 'classnames'
import { passToFunction } from '@willow/ui'
import { useUniqueId } from '@willow/ui'
import Flex from 'components/Flex/Flex'
import styles from './Label.css'
import LabelText from './LabelText'

export default function Label({
  id,
  label,
  error,
  readOnly,
  disabled,
  value,
  required,
  hasFocus,
  className,
  labelClassName,
  children,
  hiddenLabel,
  ...rest
}) {
  const labelId = useUniqueId()

  const nextLabelId = id ?? labelId

  const content = passToFunction(children, nextLabelId)

  if (label == null && (error == null || error === true) && !hiddenLabel) {
    return content
  }

  return (
    <Flex
      display="inline"
      {...rest}
      className={cx(
        styles.label,
        {
          [styles.hasError]: error != null,
          [styles.readOnly]: readOnly,
          [styles.disabled]: disabled,
          [styles.hasFocus]: hasFocus,
          [styles.required]:
            required &&
            (value == null || value === '') &&
            !readOnly &&
            !disabled,
        },
        className
      )}
    >
      <LabelText
        htmlFor={nextLabelId}
        className={cx(labelClassName, {
          [styles.hiddenLabel]: hiddenLabel && !error,
        })}
        onPointerDown={(e) => {
          e.preventDefault()
          document.getElementById(nextLabelId)?.focus()
        }}
        onClick={(e) => {
          e.preventDefault()
        }}
      >
        {error ?? label}
      </LabelText>
      {content}
    </Flex>
  )
}
