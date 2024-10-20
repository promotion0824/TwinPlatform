import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import styles from './Button.css'

export default function ButtonIcons({
  icon,
  iconSize,
  loading,
  successful,
  error,
  iconClassName,
}) {
  const cxIconClassName = cx(styles.icon, iconClassName)

  return (
    <>
      {loading && (
        <Flex
          position="absolute"
          align="center middle"
          className={styles.loading}
        >
          <Icon icon="progress" color="white" />
        </Flex>
      )}
      {successful && (
        <Flex
          position="absolute"
          align="center middle"
          className={styles.successful}
        >
          <Icon icon="ok" color="white" />
        </Flex>
      )}
      {error && (
        <Flex
          position="absolute"
          align="center middle"
          className={styles.error}
        >
          <Icon icon="cross" />
        </Flex>
      )}
      {icon != null && (
        <Icon icon={icon} size={iconSize} className={cxIconClassName} />
      )}
    </>
  )
}
