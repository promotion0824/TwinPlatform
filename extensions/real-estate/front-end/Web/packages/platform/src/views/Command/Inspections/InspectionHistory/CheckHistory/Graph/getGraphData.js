export default function constructGraphLines(check, points, color) {
  const lineGraph = {
    id: points[0]?.typeValue,
    name: points[0]?.typeValue,
    type: 'line',
    yAxis: points[0]?.typeValue,
    data: points.map((item) => ({
      x: item.submittedDate,
      y: item.numberValue,
      min: item.minimum,
      max: item.maximum,
      yUnit: item?.typeValue,
    })),
    color,
  }

  return lineGraph
}
