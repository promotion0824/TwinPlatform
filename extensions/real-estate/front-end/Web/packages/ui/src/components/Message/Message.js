import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import Text from 'components/Text/Text'
import styles from './Message.css'

export default function Message({
  icon = null,
  className = undefined,
  children,
  ...rest
}) {
  const cxClassName = cx(styles.message, className)

  return (
    <Flex align="center middle" size="medium" {...rest} className={cxClassName}>
      {icon != null && <Icon icon={icon} size="large" />}
      <Text type="message">{children}</Text>
    </Flex>
  )
}
