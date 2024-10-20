import { forwardRef, useEffect, useLayoutEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { useDebounce } from '@willow/ui'
import Portal from 'components/Portal/Portal'
import styles from './Tooltip.css'

const PADDING = 13

export default forwardRef(function Tooltip(
  {
    target,
    yTarget,
    animate = true,
    isTransparent = false,
    showPointer = true,
    position = 'top',
    paddingTop,
    style,
    className,
    contentClassName,
    children,
    zIndex = 'var(--z-tooltip)',
    ...rest
  },
  forwardedRef
) {
  let ref = useRef()
  ref = forwardedRef ?? ref

  const [state, setState] = useState({
    position: position ?? 'top',
    style: {},
  })

  function getPositionStyles() {
    const rect = target.getBoundingClientRect()
    const yRect = yTarget?.getBoundingClientRect() ?? rect

    function getTop(value) {
      return Math.max(
        10,
        Math.min(window.innerHeight - ref.current.offsetHeight - 10, value)
      )
    }

    function getLeft(value) {
      return Math.max(
        10,
        Math.min(window.innerWidth - ref.current.offsetWidth - 10, value)
      )
    }

    return {
      top: {
        top: getTop(
          yRect.top - ref.current.offsetHeight - (paddingTop ?? PADDING)
        ),
        left: getLeft(rect.left - ref.current.offsetWidth / 2 + rect.width / 2),
      },
      bottom: {
        top: getTop(rect.bottom + PADDING),
        left: getLeft(rect.left - ref.current.offsetWidth / 2 + rect.width / 2),
      },
      left: {
        top: getTop(
          yRect.top + yRect.height / 2 - ref.current.offsetHeight / 2
        ),
        left: getLeft(rect.left - ref.current.offsetWidth - PADDING),
      },
      right: {
        top: getTop(
          yRect.top + yRect.height / 2 - ref.current.offsetHeight / 2
        ),
        left: getLeft(rect.left + rect.width + PADDING),
      },
    }
  }

  // eslint-disable-next-line complexity
  function refresh() {
    const positionStyles = getPositionStyles()

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
      style: positionStyles[nextPosition] || positionStyles.bottom, // For KPIDashboard need, all tooltips are bottom
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

  const cxClassName = cx(
    styles.tooltip,
    {
      [styles.animate]: animate,
      [styles.showPointer]: showPointer,
      [styles.isTransparent]: isTransparent,
      [styles.top]: state.position === 'top',
      [styles.bottom]: state.position === 'bottom',
      [styles.left]: state.position === 'left',
      [styles.right]: state.position === 'right',
      [styles.bottomRight]: state.position === 'bottomRight',
      [styles.bottomLeft]: state.position === 'bottomLeft',
      [styles.buildingListView]: state.position === 'buildingListView',
      [styles.performanceView]: state.position === 'performanceView',
    },
    className
  )
  const cxContentClassName = cx(styles.content, contentClassName)

  const derivedStyle = {
    ...state.style,
    ...style,
    zIndex,
  }

  return (
    <Portal>
      <div {...rest} ref={ref} className={cxClassName} style={derivedStyle}>
        <div className={cxContentClassName}>{children}</div>
      </div>
    </Portal>
  )
})
