import { DependencyList, EffectCallback, useEffect, useRef } from 'react'

/**
 * Functions the same as the useEffect hook, except it will only run after the initial render.
 * Can be used to skip effects firing on loading.
 * @param effect Imperative function that can return a cleanup function
 * @param deps If present, effect will only activate if the values in the list change
 */
export const useEffectOnceMounted = (
  effect: EffectCallback,
  deps?: DependencyList
) => {
  const isMountedRef = useRef(false)

  useEffect(() => {
    if (!isMountedRef.current) {
      isMountedRef.current = true
      return () => {}
    } else {
      return effect()
    }
  }, deps)

  return isMountedRef.current
}
