import _ from 'lodash'

function getClosestXPoint(floor, point) {
  return _(floor.layerGroup.zones)
    .flatMap((layerGroupZone) => layerGroupZone.points)
    .filter((layerGroupPoint) => Math.abs(layerGroupPoint[0] - point[0]) < 5)
    .orderBy((layerGroupPoint) => Math.abs(layerGroupPoint[1] - point[1]))
    .value()[0]
}

function getClosestYPoint(floor, point) {
  return _(floor.layerGroup.zones)
    .flatMap((layerGroupZone) => layerGroupZone.points)
    .filter((layerGroupPoint) => Math.abs(layerGroupPoint[1] - point[1]) < 5)
    .orderBy((layerGroupPoint) => Math.abs(layerGroupPoint[0] - point[0]))
    .value()[0]
}

export default function getClosestPoint(floor, point) {
  return (
    floor.layerGroup.zones
      .flatMap((layerGroupZone) => layerGroupZone.points)
      .find(
        (layerGroupPoint) =>
          Math.abs(layerGroupPoint[0] - point[0]) < 5 &&
          Math.abs(layerGroupPoint[1] - point[1]) < 5
      ) ?? [
      getClosestXPoint(floor, point)?.[0] ?? point[0],
      getClosestYPoint(floor, point)?.[1] ?? point[1],
    ]
  )
}
