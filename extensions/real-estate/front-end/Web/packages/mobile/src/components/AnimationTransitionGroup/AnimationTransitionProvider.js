import { createContext, useContext } from 'react'

export const AnimationTransitionContext = createContext()

export function useAnimationTransition() {
  return useContext(AnimationTransitionContext)
}

export default function AnimationTransitionProvider({
  children,
  isExiting,
  transitionKey,
}) {
  const context = {
    isExiting,
    transitionKey,
  }

  return (
    <AnimationTransitionContext.Provider value={context}>
      {children}
    </AnimationTransitionContext.Provider>
  )
}
