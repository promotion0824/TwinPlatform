import { useRef } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useUser } from 'providers'
import Blocker from 'components/Blocker/Blocker'
import Icon from 'components/Icon/Icon'
import { usePanels } from '../PanelsContext'
import useDrag from './useDrag'
import styles from './Resizer.css'

export default function Resizer(props) {
  const { index, panelsRef } = props

  const user = useUser()
  const panels = usePanels()

  const ref = useRef()

  const resizable =
    panels.resizable &&
    panels.maximized == null &&
    panels.minimized.length === 0

  const dragger = useDrag({
    onDown() {
      if (!resizable) return null

      panels.setIsDragging(true)

      const nextSizes = _.cloneDeep(panels.sizes)
      const panelsSize = panels.horizontal
        ? panelsRef.current.clientWidth
        : panelsRef.current.clientHeight
      const resizerSize = panels.horizontal
        ? ref.current.offsetWidth
        : ref.current.offsetHeight
      const maxHeight = panelsSize - resizerSize * (nextSizes.length - 1)
      const sumOfPanelSizes = _.sum(nextSizes.map((panel) => panel.size))

      const firstSize = nextSizes[index]
      const secondSize = nextSizes[index + 1]

      let firstMaxSize =
        firstSize.size != null
          ? maxHeight - (sumOfPanelSizes - firstSize.size)
          : firstSize.size
      let secondMaxSize =
        secondSize.size != null
          ? maxHeight - (sumOfPanelSizes - secondSize.size)
          : secondSize.size

      if (firstSize.size != null && secondSize.size != null) {
        firstMaxSize = firstSize.size + secondSize.size
        secondMaxSize = firstSize.size + secondSize.size
      }

      if (firstSize.maxSize != null) {
        firstMaxSize = Math.min(firstMaxSize, firstSize.maxSize)
      }

      if (secondSize.maxSize != null) {
        secondMaxSize = Math.min(secondMaxSize, secondSize.maxSize)
      }

      return {
        firstMaxSize,
        secondMaxSize,
        sizes: nextSizes,
      }
    },

    onMove(drag) {
      const difference = panels.horizontal
        ? -drag.difference.x
        : drag.difference.y

      const nextSizes = drag.sizes.map((size, i) => {
        const isFirstPanel = i === index
        const isSecondPanel = i === index + 1

        const nextSize = isFirstPanel
          ? size.size - difference
          : size.size + difference
        const maxSize = isFirstPanel ? drag.firstMaxSize : drag.secondMaxSize

        return {
          ...size,
          size:
            size.size != null && (isFirstPanel || isSecondPanel)
              ? Math.min(Math.max(nextSize, 0), maxSize)
              : size.size,
        }
      })

      panels.setSizes(nextSizes)
    },

    onUp() {
      panels.setIsDragging(false)

      const panelsSizes = user.options.panelsSizes ?? []
      const nextPanelsSizes = _.uniqBy(
        [...panels.sizes, ...panelsSizes],
        (panel) => panel.name
      )
      user.saveUserOptions('panelsSizes', nextPanelsSizes)
    },
  })

  const cxClassName = cx(styles.resizer, {
    [styles.vertical]: !panels.horizontal,
    [styles.horizontal]: panels.horizontal,
    [styles.notResizable]: !resizable,
  })

  return (
    <div
      ref={ref}
      role="presentation"
      className={cxClassName}
      onMouseDown={dragger.onMouseDown}
      onTouchStart={dragger.onTouchStart}
    >
      <Icon icon="resizer" className={styles.icon} />
      {dragger.isDragging && <Blocker />}
    </div>
  )
}
