import { Children, useRef } from 'react'
import cx from 'classnames'
import usePanelsContext from './usePanelsContext'
import { PanelsContext } from './PanelsContext'
import Resizer from './Resizer/Resizer'
import styles from './Panels.css'

export { usePanels } from './PanelsContext'
export { default as Panel } from './Panel/Panel'

export default function Panels(props) {
  const { className, children } = props

  const panels = usePanelsContext(props)

  const panelsRef = useRef()

  const cxClassName = cx(
    styles.panels,
    {
      [styles.vertical]: !panels.horizontal,
      [styles.horizontal]: panels.horizontal,
      [styles.isDragging]: panels.isDragging,
    },
    className
  )

  const content = Children.toArray(children).reduce(
    (accumulatedChildren, child, i) => {
      const nextChild = child

      return accumulatedChildren.length === 0
        ? [nextChild]
        : [
            ...accumulatedChildren,
            <Resizer
              key={i} // eslint-disable-line
              index={i - 1}
              panelsRef={panelsRef}
            />,
            nextChild,
          ]
    },
    []
  )

  return (
    <PanelsContext.Provider value={panels}>
      <div ref={panelsRef} className={cxClassName}>
        {content}
      </div>
    </PanelsContext.Provider>
  )
}
