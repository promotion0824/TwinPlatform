import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import Panel from 'components/Panel/Panel'
import Icon from 'components/Icon/Icon'
import Text from 'components/Text/Text'
import Button from 'components/Button/Button'
import Portal from 'components/Portal/Portal'
import styles from './MessagePanel.css'

function getAnchor(attributeName, node) {
  if (node?.getAttribute == null) {
    return null
  }

  const anchorAttribute = node.getAttribute(attributeName)
  if (anchorAttribute != null) {
    return {
      current: node,
    }
  }
  if (!node.parentNode) {
    return null
  }

  return getAnchor(attributeName, node.parentNode)
}

export default function MessagePanel({
  icon,
  iconColor,
  title,
  children,
  onClose,
  className,
  target,
  useAsAnchorTargetAttributeName = 'data-useAsAnchorTarget',
  ...rest
}) {
  const panel = (
    <Panel padding="large" className={cx(styles.panel, className)} {...rest}>
      <Button
        color="grey"
        icon="cross"
        height="small"
        className={styles.closeButton}
        onClick={onClose}
      />
      <Flex horizontal>
        {icon != null && <Icon icon={icon} color={iconColor} size="large" />}
        <Flex className={cx({ [styles.rightSection]: icon != null })}>
          {title && (
            <Text type="h2" className={styles.title}>
              {title}
            </Text>
          )}
          <Text type="message" size="medium" className={styles.message}>
            {children}
          </Text>
        </Flex>
      </Flex>
    </Panel>
  )

  if (target) {
    const anchor = getAnchor(useAsAnchorTargetAttributeName, target.current)

    return <Portal target={anchor ?? target}>{panel}</Portal>
  }

  return panel
}
