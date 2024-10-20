import { useEffect, useRef } from 'react'
import cx from 'classnames'
import Portal from 'components/Portal/Portal'
import styles from './Blocker.css'

export default function Blocker({ className, ...rest }) {
  const blockerRef = useRef()

  useEffect(() => {
    blockerRef.current.focus()
  }, [])

  function handleKeyDown(e) {
    e.preventDefault()
    e.stopPropagation()
  }

  function handleClick(e) {
    e.preventDefault()
  }

  const cxClassName = cx(styles.blocker, 'ignore-onclickoutside', className)

  return (
    <Portal>
      <div
        tabIndex={0} // eslint-disable-line
        {...rest}
        ref={blockerRef}
        role="presentation"
        className={cxClassName}
        onKeyDown={handleKeyDown}
        onClick={handleClick}
      />
    </Portal>
  )
}
