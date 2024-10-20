import { forwardRef, useRef, useState } from 'react'
import cx from 'classnames'
import { useEffectOnceMounted } from '@willow/common'
import { useUser } from '@willow/ui'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import Text from 'components/Text/Text'
import styles from './FloatingPanel.css'

export { default as FloatingPanelSection } from './FloatingPanelSection'
export { default as FloatingPanelFooter } from './FloatingPanelFooter'

export default forwardRef(function FloatingPanel(
  {
    name,
    header,
    defaultIsOpen,
    className,
    children,
    style,
    onOpen,
    onClose,
    ...rest
  },
  forwardedRef
) {
  const user = useUser()
  const buttonRef = useRef()
  const contentTop = buttonRef.current
    ? buttonRef.current.offsetTop + buttonRef.current.offsetHeight
    : 0
  const contentWidth = buttonRef.current?.offsetWidth

  const [isOpen, setIsOpen] = useState(() => {
    if (defaultIsOpen != null) {
      return defaultIsOpen
    }

    return name != null ? user.options[`floating-panel-${name}`] ?? true : true
  })

  useEffectOnceMounted(() => {
    if (isOpen) {
      onOpen?.()
    } else {
      onClose?.()
    }

    user.saveOptions(`floating-panel-${name}`, isOpen)
  }, [isOpen])

  const cxClassName = cx(
    styles.floatingPanel,
    {
      [styles.isOpen]: isOpen,
    },
    className
  )

  function handleHeaderClick() {
    setIsOpen((prevIsOpen) => !prevIsOpen)
  }

  return (
    <Flex {...rest} ref={forwardedRef} className={cxClassName}>
      <Flex fill="content" className={styles.floatingPanelContent}>
        <Button
          className={styles.header}
          onClick={handleHeaderClick}
          ref={buttonRef}
        >
          <Flex
            horizontal
            fill="header hidden"
            align="middle"
            width="100%"
            padding="0 0 0 tiny"
          >
            <Text type="message">{header}</Text>
            <Icon icon="chevron" className={styles.chevron} />
          </Flex>
        </Button>
        <Flex
          size="medium"
          padding="medium"
          className={styles.content}
          style={{
            top: contentTop,
            maxHeight: `calc(100% - ${contentTop}px - 48px)`,
            width: contentWidth,
          }}
        >
          {children}
        </Flex>
      </Flex>
    </Flex>
  )
})
