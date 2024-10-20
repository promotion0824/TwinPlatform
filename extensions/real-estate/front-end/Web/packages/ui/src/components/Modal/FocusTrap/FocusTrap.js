import { forwardRef, useEffect, useRef } from 'react'
import cx from 'classnames'
import { useWindowEventListener } from '@willow/ui'
import styles from './FocusTrap.css'

export default forwardRef(
  ({ isTop = true, className, children, ...rest }, forwardedRef) => {
    let focusTrapRef = useRef()
    if (forwardedRef) {
      focusTrapRef = forwardedRef
    }

    function getFocusable() {
      const focusable = [
        focusTrapRef.current,
        ...focusTrapRef.current.querySelectorAll(
          'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        ),
      ]

      return [focusable[0], focusable[focusable.length - 1]]
    }

    useEffect(() => {
      focusTrapRef.current.focus()
    }, [])

    useWindowEventListener('keydown', (e) => {
      if (isTop && e.key === 'Tab') {
        const [firstFocusable, lastFocusable] = getFocusable()

        if (
          e.shiftKey &&
          (document.activeElement === firstFocusable ||
            !focusTrapRef.current.contains(e.target))
        ) {
          e.preventDefault()
          lastFocusable.focus()
          return
        }

        if (
          !e.shiftKey &&
          (document.activeElement === lastFocusable ||
            !focusTrapRef.current.contains(e.target))
        ) {
          e.preventDefault()
          firstFocusable.focus()
        }
      }
    })

    const cxClassName = cx(styles.focusTrap, className)

    return (
      <div
        {...rest}
        ref={focusTrapRef}
        tabIndex={0} // eslint-disable-line
        className={cxClassName}
      >
        {children}
      </div>
    )
  }
)
