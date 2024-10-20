import { useEffect, useRef, useState } from 'react'
import { useEffectOnceMounted } from '@willow/common'
import useTransitionBase from './useTransitionBase'

export default function useHeightTransition(props) {
  const { handleHeight = true, targetRef, onChange = () => {} } = props

  const ref = useRef()
  const derivedTargetRef = targetRef ?? ref

  const [state, setState] = useState({
    enterHeight: undefined,
    exitHeight: undefined,
  })

  const transition = useTransitionBase(props)

  useEffect(() => {
    if (handleHeight && derivedTargetRef.current != null) {
      setState((prevState) => ({
        ...prevState,
        enterHeight: derivedTargetRef.current?.scrollHeight,
        exitHeight: undefined,
      }))
    } else {
      onChange(true)
    }
  }, [])

  useEffectOnceMounted(() => {
    if (state.enterHeight != null) {
      onChange(true)
    }
  }, [state.enterHeight])

  useEffectOnceMounted(() => {
    if (state.exitHeight != null) {
      onChange(false)
    }
  }, [state.exitHeight])

  let maxHeight
  if (state.enterHeight != null && transition.status !== 'entered') {
    maxHeight = state.enterHeight
  }
  if (
    state.exitHeight != null &&
    (transition.status === 'entering' || transition.status === 'entered')
  ) {
    maxHeight = state.exitHeight
  }

  return {
    ...transition,
    maxHeight,

    close() {
      if (handleHeight && derivedTargetRef.current != null) {
        setState((prevState) => ({
          ...prevState,
          enterHeight: undefined,
          exitHeight: derivedTargetRef.current?.scrollHeight,
        }))
      } else {
        onChange(false)
      }
    },
  }
}
