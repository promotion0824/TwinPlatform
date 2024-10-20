import { Fragment } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useLatest } from '@willow/common'
import { useDrag } from '@willow/ui'
import { useEditor } from '../EditorContext'
import { useEditMode } from './EditModeContext'
import Zone from '../Zone/Zone'
import styles from './EditZone.css'

export default function EditZone({ zone }) {
  const editor = useEditor()
  const editMode = useEditMode()

  const selected = editMode.selectedObject === zone

  const cxClassName = cx(styles.zone, {
    [styles.selected]: selected,
    [styles.move]: selected && editMode.selectedPointIndex == null,
  })

  const onMove = useLatest((nextDrag) => {
    const scale = 100 / editor.zoom

    let x = nextDrag.diffClientX * scale
    let y = nextDrag.diffClientY * scale

    x = +x.toFixed(0)
    y = +y.toFixed(0)

    editMode.moveObject(nextDrag.selectedObject, x, y)
  })

  const drag = useDrag({
    onDown(nextDrag) {
      return {
        ...nextDrag,
        selectedObject: _.cloneDeep(editMode.selectedObject),
        x: editor.x,
        y: editor.y,
      }
    },

    onMove(nextDrag) {
      return onMove(nextDrag)
    },
  })

  return (
    <Fragment key={zone.localId}>
      <Zone
        zone={zone}
        className={cxClassName}
        onPointerDown={(e) => {
          const LEFT_BUTTON = 0
          if (e.button === LEFT_BUTTON) {
            if (!selected || editMode.selectedPointIndex != null) {
              editMode.selectObject(zone)
            } else if (editMode.selectedPointIndex == null) {
              drag.onPointerDown(e)
            }
          }
        }}
        onClick={(e) => e.stopPropagation()}
      />
      {editMode.selectedObject === zone && (
        <>
          {zone.points.map((point, pointIndex) => (
            <circle
              key={pointIndex} // eslint-disable-line
              cx={point[0]}
              cy={point[1]}
              r={2}
              className={cx(styles.point, {
                [styles.selected]: editMode.selectedPointIndex === pointIndex,
              })}
              onPointerDown={(e) => {
                const LEFT_BUTTON = 0
                if (e.button === LEFT_BUTTON) {
                  editMode.selectPointIndex(pointIndex)
                }
              }}
              onClick={(e) => e.stopPropagation()}
            />
          ))}
        </>
      )}
    </Fragment>
  )
}
