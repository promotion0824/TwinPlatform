import { useState, useRef } from 'react'
import cx from 'classnames'
import { Button, Portal, OnClickOutside } from '@willow/mobile-ui'
import styles from './ActionButton.css'

export default function ActionButton({
  items,
  className,
  onClick,
  children,
  ...rest
}) {
  const [isOpen, setIsOpen] = useState(false)
  const overlayContentRef = useRef()
  const cxActionButtonClassName = cx(className, styles.actionButton)
  const cxOverlayClassName = cx(styles.overlay, {
    [styles.open]: isOpen,
  })

  const hideOverlay = () => {
    setIsOpen(false)
  }

  const handleClick = () => {
    setIsOpen(true)

    if (onClick) {
      onClick()
    }
  }

  return (
    <>
      <Button
        onClick={handleClick}
        className={cxActionButtonClassName}
        {...rest}
      >
        {children}
      </Button>
      <Portal>
        <div className={cxOverlayClassName}>
          <OnClickOutside
            targetRef={overlayContentRef}
            onClickOutside={hideOverlay}
          >
            <div
              role="listbox"
              ref={overlayContentRef}
              className={styles.overlayContent}
            >
              {items.map(({ text, name, ...restItem }) => (
                <Button
                  {...restItem}
                  role="option"
                  key={name}
                  className={styles.overlayButton}
                >
                  {text}
                </Button>
              ))}
              <Button
                icon="close"
                data-segment="Close"
                onClick={hideOverlay}
                className={styles.overlayClose}
                iconSize="large"
                color="grey"
              />
            </div>
          </OnClickOutside>
        </div>
      </Portal>
    </>
  )
}
