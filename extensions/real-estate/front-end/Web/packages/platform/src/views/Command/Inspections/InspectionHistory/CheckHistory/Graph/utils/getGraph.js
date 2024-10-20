import _ from 'lodash'
import getBounds from './getBounds'
import getValue from './getValue'

function getNormalizedGraphs(graphs, bounds) {
  return graphs.map((graph) => {
    const nextBounds = getBounds(graph, bounds)

    return {
      ...graph,
      bounds: nextBounds,
      lines: graph.lines.map((line) => ({
        ...line,
        points: line.points.map((point) =>
          line.type === 'line'
            ? {
                ...point,
                x: (point.x - nextBounds.minX) / nextBounds.diffX,
                y: 1 - (point.y - nextBounds.minY) / nextBounds.diffY,
                min:
                  point.min !== undefined
                    ? 1 - (point.min - nextBounds.minY) / nextBounds.diffY
                    : undefined,
                max:
                  point.max !== undefined
                    ? 1 - (point.max - nextBounds.minY) / nextBounds.diffY
                    : undefined,
              }
            : {
                ...point,
                x: (point.x - nextBounds.minX) / nextBounds.diffX,
              }
        ),
      })),
    }
  })
}

export default function getGraph(data, bounds) {
  let graphs = data.map((graph) => ({
    ...graph,
    lines: graph.lines.map((line) => ({
      ...line,
      points: line.data.map((point) => ({
        ...point,
        x: getValue(point.x),
      })),
    })),
  }))

  graphs = getNormalizedGraphs(graphs, bounds)

  const xValues = _(graphs)
    .flatMap((graph) => graph.lines)
    .flatMap((line) => line.points)
    .flatMap((point) => point.x)
    .orderBy()
    .uniq()
    .value()

  return {
    xValues,
    graphs,
  }
}
