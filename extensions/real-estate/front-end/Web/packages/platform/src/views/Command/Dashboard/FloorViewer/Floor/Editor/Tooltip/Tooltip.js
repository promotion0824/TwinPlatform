import { useLayoutEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { useSize } from '@willow/ui'
import { useEditor } from '../EditorContext'
import Portal from './Portal'
import styles from './Tooltip.css'

export default function Tooltip({
  targetRef,
  point,
  clickable,
  selected,
  className,
  children,
  style,
  ...rest
}) {
  const editor = useEditor()

  const tooltipRef = useRef()
  const size = useSize(tooltipRef)

  const [state, setState] = useState({
    style: {
      left: 0,
      top: 0,
    },
  })

  const cxClassName = cx(
    styles.tooltip,
    {
      [styles.isClickable]: clickable,
      [styles.isSelected]: selected,
    },
    className
  )

  function refresh() {
    if (editor.tooltipsRef.current != null) {
      const tooltipsRect = editor.tooltipsRef.current.getBoundingClientRect()
      const targetRect = targetRef.current.getBoundingClientRect()
      const tooltipRect = tooltipRef.current.getBoundingClientRect()

      const left =
        targetRect.left +
        targetRect.width / 2 -
        tooltipRect.width / 2 -
        tooltipsRect.left
      const top = targetRect.top - tooltipRect.height - tooltipsRect.top

      setState({
        style: {
          left,
          top,
        },
      })
    }
  }

  useLayoutEffect(() => {
    refresh()
  }, [editor.x, editor.y, editor.zoom, point, size, children])

  return (
    <Portal target={editor.tooltipsRef.current}>
      <div
        ref={tooltipRef}
        {...rest}
        className={cxClassName}
        style={{
          ...state.style,
          ...style,
        }}
      >
        {children}
      </div>
    </Portal>
  )
}
