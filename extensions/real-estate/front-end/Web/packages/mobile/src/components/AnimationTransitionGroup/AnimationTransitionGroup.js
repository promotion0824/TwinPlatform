import { useState, useMemo, useEffect } from 'react'
import { TransitionGroup, CSSTransition } from 'react-transition-group'
import cx from 'classnames'
import AnimationTransitionProvider from './AnimationTransitionProvider'
import styles from './AnimationTransitionGroup.css'

const DefaultTransitionTimeout = 500

function getTransitionValue(definitions, key) {
  const definition = definitions.find((item) => {
    if (item.exact) {
      return item.expression === key
    }

    if (item.expression instanceof RegExp) {
      return item.expression.test(key)
    }

    return key.startsWith(item.expression)
  })
  return definition ? definition.value : 0
}

export default function AnimationTransitionGroup({
  children,
  transitionKey,
  definitions,
  effect = 'slide',
  direction = 'horizontal',
  transitionTimeout = DefaultTransitionTimeout,
  ...rest
}) {
  const [lastTransitionKey, setLastTransitionKey] = useState()
  const [isAnimating, setIsAnimating] = useState(false)

  const transitionClassName = useMemo(() => {
    if (!definitions) {
      return null
    }

    if (
      !lastTransitionKey ||
      !transitionKey ||
      lastTransitionKey === transitionKey
    ) {
      return null
    }

    const lastTransitionValue = getTransitionValue(
      definitions,
      lastTransitionKey
    )
    const transitionValue = getTransitionValue(definitions, transitionKey)

    if (lastTransitionValue > transitionValue) {
      return styles.slideRight
    }
    return styles.slideLeft
  }, [definitions, transitionKey, lastTransitionKey])

  const randomClassName = useMemo(
    () => `slide-${Math.random().toString().slice(2, 6)}`,
    [transitionKey]
  )

  const transitionAnimationClassName = useMemo(() => {
    if (isAnimating) {
      return cx(randomClassName, transitionClassName)
    }

    return null
  }, [isAnimating, randomClassName])

  const cxClassName = cx(
    {
      [styles.slideEffect]: effect === 'slide',
      [styles.horizontal]: direction === 'horizontal',
      [styles.isAnimating]: isAnimating,
    },
    transitionAnimationClassName,
    styles.transitionGroup
  )

  const handleTransitionEnd = () => {
    setIsAnimating(false)
    setLastTransitionKey(transitionKey)
  }

  const handleTransitionBegin = () => {
    setIsAnimating(true)
  }

  useEffect(() => {
    setLastTransitionKey(transitionKey)
  }, [])

  return (
    <TransitionGroup {...rest} className={cxClassName}>
      <CSSTransition
        classNames="page"
        key={transitionKey}
        timeout={transitionTimeout}
        onEntered={handleTransitionEnd}
        onEnter={handleTransitionBegin}
      >
        <AnimationTransitionProvider
          transitionKey={transitionKey}
          isExiting={transitionKey !== lastTransitionKey}
        >
          <div
            className={styles.page}
            style={{ '--transition-duration': `${transitionTimeout}ms` }}
          >
            {children}
          </div>
        </AnimationTransitionProvider>
      </CSSTransition>
    </TransitionGroup>
  )
}
