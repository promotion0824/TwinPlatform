import { useEffect, useLayoutEffect, useState } from 'react'
import cx from 'classnames'
import { useSize, useWindowEventListener } from 'hooks'
import OnClickOutside from 'components/OnClickOutsideNew/OnClickOutside'
import { useDropdown } from './DropdownContext'
import styles from './Content.css'

let dropdowns = []

export default function Content({
  targetRef,
  className,
  contentClassName,
  children,
  position: positionX,
  ...rest
}) {
  const dropdown = useDropdown()

  const contentSize = useSize(dropdown.contentRef)
  const derivedPositionX = positionX ?? dropdown.position

  const [state, setState] = useState({
    style: undefined,
    position: undefined,
  })

  const cxClassName = cx(
    styles.container,
    {
      [styles.positionTop]: state.position === 'top',
      [styles.positionBottom]: state.position === 'bottom',
    },
    className
  )
  const cxContentClassName = cx(styles.content, contentClassName)

  function refresh() {
    const target = targetRef?.current ?? dropdown.dropdownRef.current
    const content = dropdown.contentRef.current

    const rect = target.getBoundingClientRect()

    const positionY =
      rect.top + rect.height + content.offsetHeight > window.innerHeight &&
      rect.top - content.offsetHeight > 0
        ? 'top'
        : 'bottom'
    const isRight =
      derivedPositionX === 'right' ||
      (rect.left - 1 + content.offsetWidth > window.innerWidth &&
        rect.right + 1 - content.offsetWidth > 0)

    const left = isRight ? rect.right + 1 - content.offsetWidth : rect.left - 1
    const top =
      positionY === 'top'
        ? rect.top - content.offsetHeight + 1
        : rect.top + rect.height - 1
    const minWidth = rect.width

    setState({
      style: {
        left,
        top,
        minWidth,
      },
      position: positionY,
    })
  }

  useEffect(() => {
    dropdowns = [...dropdowns, dropdown]

    return () => {
      dropdowns = dropdowns.filter((prevDropdown) => prevDropdown !== dropdown)
    }
  }, [dropdown])

  useLayoutEffect(() => {
    refresh()
  }, [])

  useLayoutEffect(() => {
    refresh()
  }, [contentSize])

  useWindowEventListener('resize', () => {
    refresh()
  })

  useWindowEventListener('keydown', (e) => {
    const isTopDropdown = dropdowns.slice(-1)[0] === dropdown
    if (isTopDropdown) {
      if (e.key === 'Escape') {
        dropdown.close()
      }
    }
  })

  useWindowEventListener('wheel', (e) => {
    const isTopDropdown = dropdowns.slice(-1)[0] === dropdown
    if (isTopDropdown) {
      if (!dropdown.containerRef.current.contains(e.target)) {
        dropdown.close()
      }
    }
  })

  useWindowEventListener('scroll', () => {
    dropdown.close()
  })

  return (
    <OnClickOutside
      targetRefs={[dropdown.dropdownRef, dropdown.contentRef]}
      onClose={() => dropdown.close()}
    >
      <div
        {...rest}
        ref={dropdown.containerRef}
        className={cxClassName}
        style={state.style}
      >
        <div ref={dropdown.contentRef} className={cxContentClassName}>
          {children}
        </div>
      </div>
    </OnClickOutside>
  )
}
