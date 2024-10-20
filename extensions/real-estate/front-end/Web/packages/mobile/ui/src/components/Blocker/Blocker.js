import { useEffect, useRef } from 'react'
import cx from 'classnames'
import Portal from 'components/Portal/Portal'
import styles from './Blocker.css'

export default function Blocker(props) {
  const { position = 'fixed', className, ...rest } = props

  const blockerRef = useRef()

  useEffect(() => {
    blockerRef.current.focus()
  }, [])

  function handleKeyDown(e) {
    if (!position) {
      return
    }

    e.preventDefault()
    e.stopPropagation()
  }

  const cxClassName = cx(
    styles.blocker,
    {
      [styles.fixed]: position === 'fixed',
      [styles.absolute]: position === 'absolute',
    },
    'ignore-onclickoutside',
    className
  )

  const blocker = (
    <div
      tabIndex={0} // eslint-disable-line
      {...rest}
      ref={blockerRef}
      role="presentation"
      className={cxClassName}
      onKeyDown={handleKeyDown}
    />
  )

  return position === 'fixed' ? <Portal>{blocker}</Portal> : blocker
}
