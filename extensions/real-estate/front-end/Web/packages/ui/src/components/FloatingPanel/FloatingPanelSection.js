import { useState } from 'react'
import cx from 'classnames'
import { useEffectOnceMounted } from '@willow/common'
import { useUser } from '@willow/ui'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import Text from 'components/Text/Text'
import styles from './FloatingPanelSection.css'

export default function FloatingPanelSection({
  name,
  header,
  className,
  contentClassName,
  children,
  onOpen,
  onClose,
  ...rest
}) {
  const user = useUser()

  const [isOpen, setIsOpen] = useState(() =>
    name != null ? user.options[`floating-panel-section-${name}`] ?? true : true
  )

  useEffectOnceMounted(() => {
    if (isOpen) {
      onOpen?.()
    } else {
      onClose?.()
    }

    user.saveOptions(`floating-panel-section-${name}`, isOpen)
  }, [isOpen])

  const cxClassName = cx(
    styles.floatingPanelSection,
    {
      [styles.isOpen]: isOpen,
    },
    className
  )

  const cxContentClassName = cx(styles.content, contentClassName)

  function handleHeaderClick() {
    setIsOpen((prevIsOpen) => !prevIsOpen)
  }

  return (
    <Flex {...rest} className={cxClassName}>
      <Flex horizontal fill="header" className={styles.header}>
        <Button className={styles.headerButton} onClick={handleHeaderClick}>
          <Flex
            horizontal
            fill="content hidden"
            align="middle"
            size="medium"
            padding="0 0 0 small"
          >
            <Icon icon="chevronFill" className={styles.chevron} />
            <Text type="message" color={isOpen ? 'white' : 'inherit'}>
              {header}
            </Text>
          </Flex>
        </Button>
      </Flex>
      <Flex className={cxContentClassName}>{children}</Flex>
    </Flex>
  )
}
