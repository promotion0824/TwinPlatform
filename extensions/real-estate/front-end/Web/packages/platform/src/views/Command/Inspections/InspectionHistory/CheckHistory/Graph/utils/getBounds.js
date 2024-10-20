import getValue from './getValue'

export default function getGraph(graph, bounds) {
  const xValues = graph.lines.flatMap((line) =>
    line.points.map((point) => point.x)
  )

  const minX = getValue(bounds.minX) ?? Math.min(...xValues)
  const maxX = getValue(bounds.maxX) ?? Math.max(...xValues)

  const yValues = graph.lines.flatMap((line) =>
    line.points
      .filter((point) => point.x >= minX && point.x <= maxX)
      .map((point) => point.y)
  )

  let minY = getValue(bounds.minY) ?? Math.min(...yValues)
  let maxY = getValue(bounds.maxY) ?? Math.max(...yValues)

  minY = minY === Infinity ? 0 : minY
  maxY = maxY === -Infinity ? 0 : maxY

  const nextBounds = {
    minX,
    maxX,
    minY: minY === maxY ? minY - 1 : minY,
    maxY: minY === maxY ? maxY + 1 : maxY,
  }

  return {
    ...nextBounds,
    diffX: nextBounds.maxX - nextBounds.minX,
    diffY: nextBounds.maxY - nextBounds.minY,
  }
}
