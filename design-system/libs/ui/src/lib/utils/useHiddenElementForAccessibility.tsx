import { VisuallyHidden } from '@mantine/core'
import { useRef, useState, useEffect } from 'react'

/**
 * A patch fix to pass an a11y check for aria-controls while the
 * content is still hidden, by create a hidden component with the same id
 * of 'aria-controls' value.
 */
export const useHiddenElementForAccessibility = () => {
  const targetRef = useRef<HTMLElement>(null)
  const [idForHidden, setIdForHidden] = useState<string | undefined>()

  useEffect(() => {
    // this useEffect will not re-trigger when toggle the trigger button,
    // so no need to setState back to undefined
    if (targetRef.current) {
      setIdForHidden(
        targetRef.current.getAttribute('aria-controls') ?? undefined
      )
    }
    // The id will be updated a couple of times until stable,
    // need targetRef.current as an extra dependency to be able to update
    // one more time to get the correct id.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [targetRef.current])

  const hiddenComponent = <VisuallyHidden id={idForHidden} />

  return {
    targetRef,
    hiddenComponent,
  }
}
