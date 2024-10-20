import SelectedPointsProvider from '../MiniTimeSeries/SelectedPointsProvider'
import EChartsMiniTimeSeriesComponent from './MiniTimeSeriesComponent'

export default function EChartsMiniTimeSeries(props) {
  return (
    <SelectedPointsProvider>
      <EChartsMiniTimeSeriesComponent {...props} />
    </SelectedPointsProvider>
  )
}
