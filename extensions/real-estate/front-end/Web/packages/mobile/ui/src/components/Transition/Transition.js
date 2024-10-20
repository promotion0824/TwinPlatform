import { forwardRef, useRef } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useTransition } from 'hooks'
import styles from './Transition.css'

export default forwardRef(function Transition(props, forwardedRef) {
  const {
    isOpen,
    defaultIsOpen = false,
    handleHeight,
    elementType,
    type = 'snackbar',
    duration = 300,
    className,
    style,
    children,
    onChange = () => {},
    onClose,
    ...rest
  } = props

  const ref = useRef()
  const targetRef = forwardedRef ?? ref

  const transition = useTransition({
    isOpen,
    defaultIsOpen,
    handleHeight,
    targetRef,
    duration,

    onChange,
    onClose,
  })

  const cxClassName = cx(
    {
      [styles.snackbar]: type === 'snackbar',
      [styles.pill]: type === 'pill',
      [styles.exited]: transition.status === 'exited',
      [styles.entering]: transition.status === 'entering',
      [styles.entered]: transition.status === 'entered',
      [styles.exiting]: transition.status === 'exiting',
    },
    className
  )

  const derivedStyle = {
    maxHeight: transition.maxHeight,
    transition: `all ${duration}ms ease`,
    ...style,
  }

  const TransitionTag = elementType != null ? elementType : 'div'

  return (
    <TransitionTag
      ref={targetRef}
      className={cxClassName}
      style={derivedStyle}
      {...rest}
    >
      {_.isFunction(children) ? children(transition) : children}
    </TransitionTag>
  )
})
