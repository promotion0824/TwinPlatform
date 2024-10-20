import { forwardRef, useEffect, useLayoutEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { useDebounce } from 'hooks'
import Portal from 'components/Portal/Portal'
import styles from './Tooltip.css'

const PADDING = 13

export default forwardRef(function Tooltip(props, forwardedRef) {
  const {
    target,
    animate = true,
    isTransparent = false,
    position = 'top',
    style,
    children,
    ...rest
  } = props

  let ref = useRef()
  ref = forwardedRef ?? ref

  const [state, setState] = useState({
    position: position ?? 'top',
    style: {},
  })

  function refresh() {
    const rect = target.getBoundingClientRect()

    const positionStyles = {
      top: {
        top: rect.top - ref.current.offsetHeight - PADDING,
        left: rect.left - ref.current.offsetWidth / 2 + rect.width / 2,
      },
      bottom: {
        top: rect.bottom + PADDING,
        left: rect.left - ref.current.offsetWidth / 2 + rect.width / 2,
      },
      left: {
        top: rect.top + rect.height / 2 - ref.current.offsetHeight / 2,
        left: rect.left - ref.current.offsetWidth - PADDING,
      },
      right: {
        top: rect.top + rect.height / 2 - ref.current.offsetHeight / 2,
        left: rect.left + rect.width + PADDING,
      },
    }

    let nextPosition = position
    if (nextPosition === 'top') {
      if (
        positionStyles.top.top < PADDING &&
        positionStyles.bottom.top + ref.current.offsetHeight <
          window.innerHeight - PADDING
      ) {
        nextPosition = 'bottom'
      }
    } else if (nextPosition === 'bottom') {
      if (
        positionStyles.bottom.top + ref.current.offsetHeight >
          window.innerHeight - PADDING &&
        positionStyles.top.top > PADDING
      ) {
        nextPosition = 'top'
      }
    } else if (nextPosition === 'right') {
      if (
        positionStyles.right.left + ref.current.offsetWidth >
          window.innerWidth - PADDING &&
        positionStyles.left.left > PADDING
      ) {
        nextPosition = 'left'
      }
    } else if (nextPosition === 'left') {
      if (
        positionStyles.left.left < PADDING &&
        positionStyles.right.left + ref.current.offsetWidth <
          window.innerWidth - PADDING
      ) {
        nextPosition = 'right'
      }
    }

    setState({
      position: nextPosition,
      style: positionStyles[nextPosition],
    })
  }

  // don't have a solution for checking when an ancestor scroll event has ended in order to reposition the tooltip, so a debounce is used here
  const debouncedRefresh = useDebounce(refresh, 200)

  useLayoutEffect(() => {
    refresh()
  }, [target])

  useEffect(() => {
    const observer = new MutationObserver(refresh)
    observer.observe(target, { attributes: true })

    window.addEventListener('wheel', debouncedRefresh)

    return () => {
      observer.disconnect()

      window.removeEventListener('wheel', debouncedRefresh)
    }
  }, [target])

  const cxClassName = cx(styles.tooltip, {
    [styles.animate]: animate,
    [styles.isTransparent]: isTransparent,
    [styles.top]: state.position === 'top',
    [styles.bottom]: state.position === 'bottom',
    [styles.left]: state.position === 'left',
    [styles.right]: state.position === 'right',
  })

  const derivedStyle = {
    ...state.style,
    ...style,
  }

  return (
    <Portal>
      <div {...rest} ref={ref} className={cxClassName} style={derivedStyle}>
        <div className={styles.content}>{children}</div>
      </div>
    </Portal>
  )
})
