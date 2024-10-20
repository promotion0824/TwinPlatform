import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import cx from 'classnames'
import MinimizeButton from './MinimizeButton'
import { usePanels } from '../PanelsContext'
import styles from '../Panels.css'

export default function Panel({
  name,
  initialSize,
  maxSize,
  className,
  style,
  children,
  ...rest
}) {
  const panels = usePanels()

  const contentRef = useRef()
  const [hasVerticalPanels, setHasVerticalPanels] = useState(false)
  const [hasHorizontalPanels, setHasHorizontalPanels] = useState(false)

  useLayoutEffect(() => {
    panels.registerPanel(name)

    return () => panels.unregisterPanel(name)
  }, [])

  useEffect(() => {
    setHasVerticalPanels(
      contentRef.current.children[0]?.classList.contains(styles.vertical)
    )
    setHasHorizontalPanels(
      contentRef.current.children[0]?.classList.contains(styles.horizontal)
    )
  })

  const cxClassName = cx(
    styles.panel,
    {
      [styles.hasVerticalPanels]: hasVerticalPanels,
      [styles.hasHorizontalPanels]: hasHorizontalPanels,
    },
    className
  )

  const size = panels.sizes.find((panelSize) => panelSize.name === name)?.size
  const isMinimized = panels.minimized.includes(name)

  let derivedStyle = { ...style, flex: size == null ? 1 : `0 0 ${size}px` }

  if (panels.maximized != null) {
    derivedStyle = { ...style, flex: panels.maximized === name ? 1 : 0 }
  }

  if (isMinimized) {
    derivedStyle = { ...style, flex: 0 }
  }

  return (
    <div {...rest} className={cxClassName} style={derivedStyle}>
      <div ref={contentRef} className={styles.panelContent}>
        {children}
      </div>
      {isMinimized && <MinimizeButton name={name} />}
    </div>
  )
}
