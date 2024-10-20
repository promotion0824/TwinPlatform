import SelectedPointsProvider from '../MiniTimeSeries/SelectedPointsProvider.tsx'
import EChartsMiniTimeSeriesComponent from './MiniTimeSeriesComponent.js'

export default function EChartsMiniTimeSeries(props) {
  return (
    <SelectedPointsProvider>
      <EChartsMiniTimeSeriesComponent {...props} />
    </SelectedPointsProvider>
  )
}
