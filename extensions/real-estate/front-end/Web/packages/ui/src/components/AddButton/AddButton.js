import cx from 'classnames'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import styles from './AddButton.css'

/**
 * @deprecated
 * Please use `Button` with prefix `Icon` from '@willowinc/ui' instead.
 */
export default function AddButton({
  className = undefined,
  children = undefined,
  ...rest
}) {
  const cxClassName = cx(styles.addButton, className)

  return (
    <Button
      color="grey"
      height="small"
      ripple
      {...rest}
      className={cxClassName}
    >
      <span className={styles.content}>{children}</span>
      <Flex align="center middle" className={styles.icon}>
        <Icon icon="add" />
      </Flex>
    </Button>
  )
}
