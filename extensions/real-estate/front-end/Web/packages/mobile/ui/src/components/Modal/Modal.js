import { useRef } from 'react'
import cx from 'classnames'
import { useWindowEventListener } from 'hooks'
import Button from 'components/Button/Button'
import FocusTrap from 'components/FocusTrap/FocusTrap'
import OnClickOutside from 'components/OnClickOutside/OnClickOutside'
import Portal from 'components/Portal/Portal'
import Spacing from 'components/Spacing/Spacing'
import { useSingleModal } from './SingleModalProvider'
import styles from './Modal.css'

export { default as useModal } from './useModal'
export { GlobalModalProvider as ModalProvider } from './GlobalModalProvider'

export default function Modal(props) {
  const {
    text,
    handleClose = true,
    className,
    modalClassName,
    contentClassName,
    children,
  } = props

  const singleModal = useSingleModal()

  const focusTrapRef = useRef()

  function handleCloseClick() {
    if (handleClose) {
      singleModal.closeIfTopModal()
    }
  }

  useWindowEventListener('keydown', (e) => {
    if (e.key === 'Escape' && handleClose) {
      singleModal.closeIfTopModal()
    }
  })

  const cxMaskClassName = cx(styles.mask, {
    [styles.exited]: singleModal.transition.status === 'exited',
    [styles.entering]: singleModal.transition.status === 'entering',
    [styles.exiting]: singleModal.transition.status === 'exiting',
  })

  const cxClassName = cx(
    styles.modal,
    {
      [styles.exited]: singleModal.transition.status === 'exited',
      [styles.entering]: singleModal.transition.status === 'entering',
      [styles.exiting]: singleModal.transition.status === 'exiting',
    },
    className
  )

  const cxModalContentClassName = cx(styles.modalContent, modalClassName)
  const cxContentClassName = cx(styles.content, contentClassName)

  return (
    <Portal>
      <div className={cxMaskClassName} />
      <div ref={singleModal.transition.targetRef} className={cxClassName}>
        <div className={styles.container}>
          <OnClickOutside
            targetRef={focusTrapRef}
            onClickOutside={handleCloseClick}
          >
            <FocusTrap ref={focusTrapRef} className={cxModalContentClassName}>
              <Spacing
                horizontal
                type="header"
                overflow="hidden"
                className={styles.header}
              >
                <Spacing align="middle">{text}</Spacing>
                {handleClose && (
                  <Button
                    icon="close"
                    tabIndex={-1}
                    className={styles.close}
                    onClick={handleCloseClick}
                  />
                )}
              </Spacing>
              <div className={cxContentClassName}>{children}</div>
            </FocusTrap>
          </OnClickOutside>
        </div>
      </div>
    </Portal>
  )
}
