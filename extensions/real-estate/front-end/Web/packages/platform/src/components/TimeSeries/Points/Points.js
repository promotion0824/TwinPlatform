import Point from './Point'
import { useTimeSeries } from '../TimeSeriesContext'

export default function Points() {
  const timeSeries = useTimeSeries()

  return (
    <>
      {timeSeries.points.map((point) => (
        <Point key={point.sitePointId} point={point} />
      ))}
    </>
  )
}
