import _ from 'lodash'

export default function getFloors(floors) {
  let nextFloors = floors
    .filter(
      (floor) => floor.name !== 'BLDG' && floor.name !== 'SOFI CAMPUS OVERALL'
    )
    .map((floor) => {
      let geometry = []
      try {
        geometry = JSON.parse(floor.geometry)
        if (!_.isArray(geometry)) geometry = []
      } catch (err) {
        // do nothing
      }

      return { ...floor, geometry }
    })

  nextFloors = nextFloors.map((floor) => ({
    ...floor,
    geometry: floor.geometry.map((zone) => [...zone, zone[0]]),
  }))

  const firstGeometry = (
    _.findLast(nextFloors, (floor) => floor.geometry.length > 0) ?? {
      geometry: [
        [
          [0, 0],
          [100, 0],
          [100, 100],
          [0, 100],
          [0, 0],
        ],
      ],
    }
  ).geometry

  nextFloors = nextFloors.map((floor, i) =>
    i === nextFloors.length - 1 ? { ...floor, geometry: firstGeometry } : floor
  )

  nextFloors = nextFloors
    .reduceRight((accFloors, floor) => {
      const prevFloor = accFloors.slice(-1)[0]
      if (prevFloor == null) {
        return [floor]
      }

      return [
        ...accFloors,
        {
          ...floor,
          geometry:
            floor.geometry.length === 0 ? prevFloor.geometry : floor.geometry,
        },
      ]
    }, [])
    .reverse()

  const points = nextFloors.flatMap((floor) =>
    floor.geometry.flatMap((zones) => zones.flatMap((zone) => zone))
  )

  const minPoint = Math.min(...points)
  const maxPoint = Math.max(...points)
  const difference = maxPoint - minPoint

  nextFloors = nextFloors.map((floor) => ({
    ...floor,
    geometry: floor.geometry.map((zone) =>
      zone.map((coordinates) => [
        ((coordinates[0] - minPoint) / difference) * 100,
        ((coordinates[1] - minPoint) / difference) * 100,
      ])
    ),
  }))

  const zones = nextFloors.flatMap((floor) =>
    floor.geometry.flatMap((zone) => zone)
  )
  const xPoints = zones.flatMap((zone) => zone[0])
  const yPoints = zones.flatMap((zone) => zone[1])
  const minX = Math.min(...xPoints)
  const maxX = Math.max(...xPoints)
  const minY = Math.min(...yPoints)
  const maxY = Math.max(...yPoints)
  const midX = (maxX - minX) / 2
  const midY = (maxY - minY) / 2

  nextFloors = nextFloors.map((floor) => ({
    ...floor,
    geometry: floor.geometry.map((zone) =>
      zone.map((coordinate) => [
        coordinate[0] - minX - midX,
        coordinate[1] - minY - midY,
      ])
    ),
  }))

  return nextFloors
}
