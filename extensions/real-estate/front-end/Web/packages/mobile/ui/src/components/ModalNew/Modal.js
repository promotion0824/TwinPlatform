import { useEffect, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import Button from 'components/Button/Button'
import FocusTrap from 'components/FocusTrap/FocusTrap'
import Spacing from 'components/Spacing/Spacing'
import Timeout from 'components/Timeout/Timeout'
import OnClickOutside from 'components/OnClickOutsideNew/OnClickOutside'
import Portal from 'components/Portal/Portal'
import TextNew from 'components/Text/Text'
import { useEffectOnceMounted } from '@willow/common'
import { useModal, ModalContext } from './ModalContext'
import styles from './Modal.css'

export { useModal } from './ModalContext'

export default function Modal({
  type = 'right',
  size = 'small',
  header,
  closable = true,
  children,
  className,
  contentClassName,
  onClose,
}) {
  const modal = useModal()

  const focusTrapRef = useRef()
  const [status, setStatus] = useState('closed')
  const [response, setResponse] = useState()

  const cxClassName = cx(
    styles.modal,
    {
      [styles.isClosable]: closable,
      [styles.typeLeft]: type === 'left',
      [styles.typeRight]: type === 'right',
      [styles.typeCenter]: type === 'center',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      [styles.closing]: status === 'closing',
      [styles.closed]: status === 'closed',
    },
    className
  )
  const cxMaskClassName = cx(styles.mask, {
    [styles.maskTypeCenter]: type === 'center',
    [styles.closing]: status === 'closing',
    [styles.closed]: status === 'closed',
  })
  const cxContentClassName = cx(styles.content, contentClassName)

  function close(nextResponse) {
    setResponse(nextResponse)
    setStatus('closing')
  }

  useEffect(() => {
    setStatus('opening')
  }, [])

  useEffectOnceMounted(() => {
    if (status === 'closed') {
      onClose(response)
    }
  }, [status])

  const context = {
    close,

    closeAll() {
      context.close()

      modal?.closeAll()
    },
  }

  return (
    <ModalContext.Provider value={context}>
      <Portal>
        <div className={cxMaskClassName} />
        <OnClickOutside
          targetRefs={[focusTrapRef]}
          isClosable={closable}
          onClose={() => close()}
        >
          {({ isTop }) => (
            <FocusTrap
              ref={focusTrapRef}
              isTop={isTop}
              className={cxClassName}
              onTransitionEnd={() => {
                if (status === 'opening') {
                  setStatus('opened')
                } else if (status === 'closing') {
                  setStatus('closed')
                }
              }}
            >
              <Spacing
                horizontal
                type="header"
                height="large"
                align="middle"
                className={styles.header}
              >
                {_.isFunction(header) ? (
                  header(context)
                ) : (
                  <Spacing padding="large">
                    <TextNew type="h2">{header}</TextNew>
                  </Spacing>
                )}
                <Button
                  icon="close"
                  tabIndex={-1}
                  className={styles.close}
                  onClick={close}
                  data-segment="Close"
                />
              </Spacing>
              <div className={cxContentClassName}>
                {_.isFunction(children) ? children(context) : children}
              </div>
            </FocusTrap>
          )}
        </OnClickOutside>
        {status === 'closing' && (
          <Timeout ms={200} onTimeout={() => onClose()} />
        )}
      </Portal>
    </ModalContext.Provider>
  )
}
