import { useLayoutEffect, useState } from 'react'
import cx from 'classnames'
import { useSize, useWindowEventListener } from 'hooks'
import OnClickOutside from 'components/OnClickOutsideNew/OnClickOutside'
import { useDropdown } from './DropdownContext'
import styles from './Content.css'

export default function Content({
  targetRef,
  position,
  useMinWidth,
  className,
  contentClassName,
  children,
  ...rest
}) {
  const dropdown = useDropdown()

  const contentSize = useSize(dropdown.contentRef)

  const [state, setState] = useState({
    style: undefined,
    position: undefined,
  })

  let yPosition
  if (position?.includes('top')) yPosition = 'top'
  if (position?.includes('bottom')) yPosition = 'bottom'

  let xPosition
  if (position?.includes('left')) xPosition = 'left'
  if (position?.includes('right')) xPosition = 'right'

  const cxClassName = cx(
    styles.dropdownContent,
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

    let nextYPosition = yPosition
    if (yPosition == null) {
      nextYPosition =
        rect.top + rect.height + content.offsetHeight > window.innerHeight &&
        rect.top - content.offsetHeight > 0
          ? 'top'
          : 'bottom'
    }

    let nextXPosition = xPosition
    if (xPosition == null) {
      nextXPosition =
        rect.left + content.offsetWidth > window.innerWidth &&
        rect.right - content.offsetWidth > 0
          ? 'right'
          : 'left'
    }

    const top =
      nextYPosition === 'top'
        ? rect.top - content.offsetHeight
        : rect.top + rect.height

    const left =
      nextXPosition === 'left' ? rect.left : rect.right - content.offsetWidth

    const minWidth = rect.width

    setState({
      style: {
        left,
        top,
        minWidth: useMinWidth ? minWidth : undefined,
      },
      position: nextYPosition,
    })
  }

  useLayoutEffect(() => {
    refresh()
  }, [])

  useLayoutEffect(() => {
    refresh()
  }, [contentSize])

  useWindowEventListener('resize', () => {
    refresh()
  })

  useWindowEventListener('scroll', () => {
    dropdown.close()
  })

  return (
    <OnClickOutside
      targetRefs={[dropdown.dropdownRef, dropdown.contentRef]}
      onClose={() => dropdown.close()}
      closeOnWheel
    >
      <div
        {...rest}
        ref={dropdown.contentRef}
        className={cxClassName}
        style={state.style}
      >
        <div className={cxContentClassName}>{children}</div>
      </div>
    </OnClickOutside>
  )
}
