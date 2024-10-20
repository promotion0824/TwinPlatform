import SelectedPointsProvider from './SelectedPointsProvider.tsx'
import MiniTimeSeriesComponent from './MiniTimeSeriesComponent'

export default function MiniTimeSeries(props) {
  return (
    <SelectedPointsProvider>
      <MiniTimeSeriesComponent {...props} />
    </SelectedPointsProvider>
  )
}
