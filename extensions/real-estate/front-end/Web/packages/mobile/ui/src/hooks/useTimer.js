import { useEffect, useRef } from 'react'

export default function useTimer() {
  const timerIdRef = useRef()
  const animationFrameIdRef = useRef()

  useEffect(
    () => () => {
      window.clearTimeout(timerIdRef.current)
      window.cancelAnimationFrame(animationFrameIdRef.current)
    },
    []
  )

  return {
    setTimeout(fn, ms) {
      window.clearTimeout(timerIdRef.current)
      timerIdRef.current = window.setTimeout(fn, ms)
    },

    clearTimeout() {
      window.clearTimeout(timerIdRef.current)
    },

    requestAnimationFrame(fn) {
      window.cancelAnimationFrame(animationFrameIdRef.current)
      timerIdRef.current = window.requestAnimationFrame(fn)
    },

    cancelAnimationFrame() {
      window.cancelAnimationFrame(animationFrameIdRef.current)
    },
  }
}
