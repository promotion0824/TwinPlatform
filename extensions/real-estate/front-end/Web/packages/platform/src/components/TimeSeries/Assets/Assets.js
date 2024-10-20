import { useTimeSeries } from '../TimeSeriesContext'
import Asset from './Asset'

export default function Assets() {
  const timeSeries = useTimeSeries()

  return (
    <>
      {timeSeries.assets.map((asset) => (
        <Asset key={asset.siteAssetId} asset={asset} />
      ))}
    </>
  )
}
