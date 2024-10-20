import { useEffect, useState } from 'react'
import { EditModeContext } from './EditModeContext'
import { useFloor } from '../../FloorContext'

export default function EditModeProvider(props) {
  const floor = useFloor()

  const [selectedObjectLocalId, setSelectedObjectLocalId] = useState()
  const [selectedPointIndex, setSelectedPointIndex] = useState()
  const [copiedZone, setCopiedZone] = useState()

  useEffect(() => {
    setSelectedObjectLocalId()
    setSelectedPointIndex()
    setCopiedZone()
  }, [floor.mode])

  let layerGroups = []
  if (floor.layerGroup != null) {
    if (floor.layerGroup.name === 'Assets layer') {
      layerGroups = floor.layerGroups
    } else {
      layerGroups = [floor.layerGroup]
    }
  }
  const zones = floor.layerGroup?.zones ?? []
  const equipments = layerGroups.flatMap((layerGroup) => layerGroup.equipments)

  const context = {
    zones,
    equipments,
    selectedObject:
      zones.find((zone) => zone.localId === selectedObjectLocalId) ??
      equipments.find(
        (equipment) => equipment.localId === selectedObjectLocalId
      ),
    selectedPointIndex,
    copiedZone,

    setCopiedZone,

    selectObject(nextSelectedObject) {
      if (copiedZone != null) {
        return
      }

      setSelectedObjectLocalId((prevSelectedObjectLocalId) =>
        nextSelectedObject != null &&
        (prevSelectedObjectLocalId !== nextSelectedObject.localId ||
          selectedPointIndex != null)
          ? nextSelectedObject.localId
          : undefined
      )

      setSelectedPointIndex()
    },

    selectPointIndex(nextSelectedPointIndex) {
      if (copiedZone != null) {
        return
      }

      setSelectedPointIndex((prevSelectedPointIndex) =>
        prevSelectedPointIndex !== nextSelectedPointIndex
          ? nextSelectedPointIndex
          : undefined
      )
    },

    moveObject(object, x, y) {
      let nextObject = {
        ...object,
        points: object.points
          .map((point, i) => {
            if (selectedPointIndex != null) {
              return selectedPointIndex === i
                ? [point[0] + x, point[1] + y]
                : point
            }

            return [point[0] + x, point[1] + y]
          })
          .map((point) => [+point[0].toFixed(0), +point[1].toFixed(0)]),
      }

      if (selectedPointIndex == null) {
        const minX = Math.min(...nextObject.points.map((point) => point[0]), 0)
        const minY = Math.min(...nextObject.points.map((point) => point[1]), 0)
        const maxX =
          Math.max(...nextObject.points.map((point) => point[0]), floor.width) -
          floor.width
        const maxY =
          Math.max(
            ...nextObject.points.map((point) => point[1]),
            floor.height
          ) - floor.height

        nextObject = {
          ...nextObject,
          points: nextObject.points.map((point) => {
            const nextX = point[0] - minX - maxX
            const nextY = point[1] - minY - maxY

            return [nextX, nextY]
          }),
        }
      }

      const isValid = nextObject.points.every(
        (point) =>
          point[0] >= 0 &&
          point[0] <= floor.width &&
          point[1] >= 0 &&
          point[1] <= floor.height
      )

      if (isValid) {
        floor.updateObject(nextObject)
      }
    },
  }

  return <EditModeContext.Provider {...props} value={context} />
}
