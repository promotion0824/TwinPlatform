import { useEffect, useRef, useState } from 'react'
import { useLatest } from '@willow/common'
import useLayoutEffectOnUpdate from '../useLayoutEffectOnUpdate'

export default function useTransition(props = {}) {
  const { isOpen, duration = 300, targetRef, onClose = () => {} } = props

  const ref = useRef()
  const derivedTargetRef = targetRef ?? ref

  const [status, setStatus] = useState(isOpen ? 'entered' : 'exited')

  function end() {
    setStatus(isOpen ? 'entered' : 'exited')

    if (!isOpen) {
      onClose()
    }
  }

  const handleTransitionEnd = useLatest((e) => {
    if (e.target === derivedTargetRef.current) {
      end()
    }
  })

  useEffect(() => {
    derivedTargetRef.current?.addEventListener(
      'transitionend',
      handleTransitionEnd
    )

    return () =>
      derivedTargetRef.current?.removeEventListener(
        'transitionend',
        handleTransitionEnd
      )
  }, [derivedTargetRef])

  const handleTimeout = useLatest(() => {
    if (status === 'entering' || status === 'exiting') {
      end()
    }
  })

  function refresh() {
    setStatus(isOpen ? 'entering' : 'exiting')

    const timerId = window.setTimeout(handleTimeout, duration)

    return () => {
      window.clearTimeout(timerId)
    }
  }

  useLayoutEffectOnUpdate(() => refresh(), [isOpen])

  return {
    targetRef: derivedTargetRef,
    status,
  }
}
