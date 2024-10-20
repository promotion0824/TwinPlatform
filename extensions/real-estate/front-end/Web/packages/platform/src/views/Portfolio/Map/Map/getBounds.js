export default function getBounds(sites) {
  const sitesWithLocations = sites.filter((site) => site.location != null)

  if (sitesWithLocations.length === 0) {
    return undefined
  }

  const xCoordinates = sitesWithLocations.map((site) => site.location[0])
  const yCoordinates = sitesWithLocations.map((site) => site.location[1])

  const minX = Math.min(...xCoordinates)
  const maxX = Math.max(...xCoordinates)
  const minY = Math.min(...yCoordinates)
  const maxY = Math.max(...yCoordinates)

  if (minX === maxX && minY === maxY) {
    return [[minX, minY]]
  }

  return [
    [minX, minY],
    [maxX, maxY],
  ]
}
