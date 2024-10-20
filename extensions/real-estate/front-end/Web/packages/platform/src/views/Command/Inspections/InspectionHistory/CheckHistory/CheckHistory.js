import Graph from './Graph/Graph'
import CheckHistoryDataGrid from './CheckHistoryDataGrid'

export default function CheckHistory({
  check,
  isGraphActive,
  checkRecordsHistory,
  times,
}) {
  return (
    <>
      {isGraphActive && (
        <Graph
          check={check}
          checkRecordsHistory={checkRecordsHistory}
          times={times}
        />
      )}
      {!isGraphActive && (
        <CheckHistoryDataGrid
          check={check}
          checkRecords={checkRecordsHistory}
        />
      )}
    </>
  )
}
