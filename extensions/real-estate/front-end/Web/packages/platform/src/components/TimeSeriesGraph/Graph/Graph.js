import cx from 'classnames'
import { useDuration } from '@willow/ui'
import BoolGraph from './BoolGraph/BoolGraph'
import MultiStateGraph from './MultiStateGraph/MultiStateGraph'
import LineGraph from './LineGraph/LineGraph'
import styles from './Graph.css'
import { useTimeSeriesGraph } from '../TimeSeriesGraphContext'

export default function Graph({
  graph,
  highlightPointTime,
  loadingSitePointIds,
}) {
  const duration = useDuration()
  const timeSeriesGraph = useTimeSeriesGraph()

  const cxClassName = cx(styles.graph, {
    [styles.lineGraph]: graph.type === 'analog',
  })

  const testId = `tab-timeSeries-graph${
    `-${(graph?.lines ?? [])[0]?.pointId}` ?? ''
  }`

  return (
    <div className={cxClassName} data-testid={testId}>
      <div className={styles.graphContent}>
        <div className={styles.container}>
          <div className={styles.content}>
            <>
              {graph.type === 'multiState' && <MultiStateGraph graph={graph} />}
              {graph.type === 'binary' && (
                <BoolGraph
                  graph={graph}
                  highlightPointTime={highlightPointTime}
                />
              )}
              {graph.type === 'analog' && (
                <LineGraph
                  graph={graph}
                  highlightPointTime={highlightPointTime}
                  size={timeSeriesGraph.size}
                  granularity={duration(
                    timeSeriesGraph.granularity
                  ).milliseconds()}
                  times={timeSeriesGraph.times}
                  isLineLoading={(line) =>
                    loadingSitePointIds.some((x) => x === line.sitePointId)
                  }
                />
              )}
            </>
          </div>
        </div>
      </div>
    </div>
  )
}
