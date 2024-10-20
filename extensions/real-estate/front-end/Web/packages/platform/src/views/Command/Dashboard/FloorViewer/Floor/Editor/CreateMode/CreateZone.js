import _ from 'lodash'
import { useLatest } from '@willow/common'
import { useDrag, useEventListener, useWindowEventListener } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import { useEditor } from '../EditorContext'
import Zone from '../Zone/Zone'
import styles from './CreateMode.css'

export default function CreateMode({ mouse, zone, setZone }) {
  const floor = useFloor()
  const editor = useEditor()

  let path
  if (zone != null) {
    path = `M${zone.points.map((point) => point.join(',')).join(' ')}`
  }

  let mousePath
  if (zone != null && mouse != null) {
    const lastPoint = zone.points.slice(-1)[0]

    mousePath = `M${lastPoint.join(',')} ${mouse.join(',')}`
  }

  const latestOnMove = useLatest((nextDrag) => {
    if (
      zone != null &&
      nextDrag.diffClientX !== 0 &&
      nextDrag.diffClientY !== 0
    ) {
      setZone({
        ...zone,
        isDragging: true,
      })
    }
  })

  const latestOnUp = useLatest(() => {
    if (mouse == null) {
      setZone({
        ...zone,
        isDragging: false,
      })
      return
    }

    if (zone?.isDragging && zone.points.length === 1) {
      const minX = Math.min(zone.points[0][0], mouse[0])
      const maxX = Math.max(zone.points[0][0], mouse[0])
      const minY = Math.min(zone.points[0][1], mouse[1])
      const maxY = Math.max(zone.points[0][1], mouse[1])

      if (minX !== maxX && minY !== maxY) {
        floor.addZone({
          points: [
            [minX, minY],
            [maxX, minY],
            [maxX, maxY],
            [minX, maxY],
          ],
        })
      }
      setZone()
    }
  })

  const drag = useDrag({
    onDown(nextDrag) {
      if (floor.layerGroup?.id === 'floor_layer' && zone?.points?.length >= 1) {
        return null
      }

      const hasVisibleZones =
        floor.visibleLayerGroups.flatMap((layerGroup) => layerGroup.equipments)
          .length > 0
      const pointsLength = hasVisibleZones ? 2 : 1

      if (zone != null && zone?.points?.length >= pointsLength) {
        return null
      }

      return {
        ...nextDrag,
        x: editor.x,
        y: editor.y,
      }
    },

    onMove(nextDrag) {
      latestOnMove(nextDrag)
    },

    onUp() {
      latestOnUp()
    },
  })

  useEventListener(editor.svgRef, 'pointerdown', (e) => {
    const LEFT_BUTTON = 0
    if (e.button !== LEFT_BUTTON) {
      return
    }

    if (mouse == null) {
      return
    }

    if (zone == null) {
      setZone({
        points: [mouse],
      })
      return
    }

    const isLast = _.isEqual(zone.points[0], mouse)
    if (isLast) {
      if (zone.points.length > 2) {
        floor.addZone(zone)
      }
      setZone()
    } else {
      setZone({
        points: [...zone.points, mouse],
      })
    }
  })

  useEventListener(editor.svgRef, 'pointerdown', (e) => {
    const LEFT_BUTTON = 0
    if (e.button !== LEFT_BUTTON) {
      return null
    }

    return drag.onPointerDown(e)
  })

  useWindowEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      if (zone?.points.length > 1 && !zone.isDragging) {
        setZone({
          ...zone,
          points: zone.points.slice(0, -1),
        })
      } else {
        setZone()
      }
    }
  })

  return (
    <>
      {mouse != null && (
        <circle cx={mouse[0]} cy={mouse[1]} r={3} className={styles.mouse} />
      )}
      {path != null && <path d={path} className={styles.path} />}
      {mousePath != null && !zone.isDragging && (
        <path d={mousePath} className={styles.path} />
      )}
      {mouse != null && zone != null && zone.isDragging && (
        <Zone
          zone={{
            points: [
              [
                Math.min(zone.points[0][0], mouse[0]),
                Math.min(zone.points[0][1], mouse[1]),
              ],
              [
                Math.max(zone.points[0][0], mouse[0]),
                Math.min(zone.points[0][1], mouse[1]),
              ],
              [
                Math.max(zone.points[0][0], mouse[0]),
                Math.max(zone.points[0][1], mouse[1]),
              ],
              [
                Math.min(zone.points[0][0], mouse[0]),
                Math.max(zone.points[0][1], mouse[1]),
              ],
            ],
          }}
        />
      )}
      {zone != null &&
        zone.points.map((point, i) => (
          <circle
            key={i} // eslint-disable-line
            cx={point[0]}
            cy={point[1]}
            r={2}
            className={styles.circle}
          />
        ))}
    </>
  )
}
