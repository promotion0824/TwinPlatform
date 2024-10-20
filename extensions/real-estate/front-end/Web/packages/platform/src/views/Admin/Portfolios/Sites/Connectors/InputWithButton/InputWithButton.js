import cx from 'classnames'
import { Flex, Input, Progress } from '@willow/ui'
import styles from './InputWithButton.css'

export default function InputWithButton({
  value,
  text,
  onClick,
  isLoading,
  className,
  disabled,
}) {
  const cxContainer = cx(styles.container, className, {
    [styles.hasValue]: !!value && value !== '',
  })

  return (
    <Flex horizontal fill="header" className={cxContainer}>
      <Input
        className={styles.input}
        value={value}
        readOnly
        disabled={disabled}
      />
      <button
        type="button"
        className={styles.button}
        onClick={onClick}
        disabled={disabled}
      >
        {isLoading ? <Progress className={styles.progress} /> : text}
      </button>
    </Flex>
  )
}
