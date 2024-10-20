import { useSize, useWindowEventListener } from '@willow/ui'
import cx from 'classnames'
import OnClickOutside from 'components/OnClickOutside/OnClickOutside'
import { useLayoutEffect, useState } from 'react'
import styles from './DropdownContent.css'
import { useDropdown } from './DropdownContext'
import KeyboardHandler from './KeyboardHandler'

export default function DropdownContent({
  position,
  useMinWidth,
  className,
  contentClassName,
  contentStyle,
  children,
  zIndex,
}) {
  const dropdown = useDropdown()

  const contentSize = useSize(dropdown.contentRef)

  const [state, setState] = useState({
    style: undefined,
    isTop: true,
  })

  const cxClassName = cx(
    styles.dropdownContent,
    {
      [styles.top]: state.isTop,
    },
    className
  )
  const cxContentClassName = cx(styles.content, contentClassName)

  function refresh() {
    const rect = dropdown.dropdownRef.current.getBoundingClientRect()
    const contentRect = dropdown.contentRef.current.getBoundingClientRect()

    const { left } = rect
    const right = rect.right - contentRect.width
    let isLeft = left + contentRect.width <= window.innerWidth || right < 0
    if (position?.split(' ').includes('left')) isLeft = true
    if (position?.split(' ').includes('right')) isLeft = false

    const top = rect.top - contentRect.height
    const bottom = rect.top + rect.height
    let isTop = bottom + contentRect.height > window.innerHeight && top >= 0
    if (position?.split(' ').includes('top')) isTop = true
    if (position?.split(' ').includes('bottom')) isTop = false

    setState({
      style: {
        left: isLeft ? left : right,
        top: isTop ? top : bottom,
        minWidth: useMinWidth ? rect.width : undefined,
        ...(zIndex !== undefined ? { zIndex } : {}),
      },
      isTop,
    })
  }

  useLayoutEffect(() => {
    refresh()
  }, [contentSize])

  useWindowEventListener('resize', refresh)
  useWindowEventListener('scroll', dropdown.close)

  return (
    <OnClickOutside
      targetRefs={[dropdown.dropdownRef, dropdown.contentRef]}
      closeOnWheel
      onClose={() => dropdown.close()}
    >
      {({ isTop }) => (
        <>
          <div
            ref={dropdown.contentRef}
            className={cxClassName}
            onWheel={(e) => e.stopPropagation()}
            style={state.style}
            data-testid="dropdown-content"
          >
            <div className={cxContentClassName} style={contentStyle}>
              {children}
            </div>
          </div>
          {isTop && <KeyboardHandler />}
        </>
      )}
    </OnClickOutside>
  )
}
