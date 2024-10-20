import { useScopeSelector, DatePicker, useDateTime } from '@willow/ui'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'

const ExperimentalDashboards = () => {
  const dateTime = useDateTime()
  const [defaultStart, defaultEnd] = [
    dateTime.now().addMonths(-1).format(),
    dateTime.now().format(),
  ]

  // "start"/"end" are the start and end date time to be used to query dashboard data
  const [{ start = defaultStart, end = defaultEnd }, setQueryParams] =
    useMultipleSearchParams(['start', 'end'])

  const scopeSelector = useScopeSelector()
  // location.twin.id is the twin id of current scope
  // when scope is "All Locations", location.twin.id is undefined
  const { location, isScopeUsedAsBuilding } = scopeSelector

  // utility properties to tell which type of dashboard is being used
  const isBuildingDashboard = isScopeUsedAsBuilding(location)
  const isPortfolioDashboard = !isBuildingDashboard

  return (
    <div>
      <DatePicker
        type="date-time-range"
        value={[start, end]}
        onChange={([nextStart, nextEnd]) => {
          setQueryParams({
            start: nextStart,
            end: nextEnd,
          })
        }}
      />
      <h1>Experimental Dashboards</h1>
      <p>Used only by Dashboard Team at the moment</p>
    </div>
  )
}

export default ExperimentalDashboards
