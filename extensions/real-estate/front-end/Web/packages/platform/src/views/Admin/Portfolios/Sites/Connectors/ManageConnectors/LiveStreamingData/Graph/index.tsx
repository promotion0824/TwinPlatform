/* eslint-disable react/require-default-props */
import { ProviderRequiredError } from '@willow/common'
import { useSize } from '@willow/ui'
import {
  createContext,
  MutableRefObject,
  useContext,
  useLayoutEffect,
  useRef,
  useState,
} from 'react'
import { styled } from 'twin.macro'
import Labels from './Labels'
import Svg from './Svg'
import { Columns, GraphContextType, GraphData } from './types'

// Graph is a clone from packages\platform\src\views\PortfolioOxford\Connectivity\ConnectivitySite\LiveStreamingData
// This provider has been refactored and adjustments were made to be better suited for the new manage connector page.
const GraphContext = createContext<GraphContextType | undefined>(undefined)

export function useGraph() {
  const context = useContext(GraphContext)
  if (context == null) {
    throw new ProviderRequiredError('Graph')
  }
  return context
}

/**
 * Graph will construct columns object that will be used in Svg component to display the group of columns
 * timestamps, array of timestamps, will be calculated that will be used to display x label points in XLabels component
 * maxValue, max totalTelemetryCount in telemetry, will be calculated that will be used to display y label points in YLabels component
 */
export default function Graph({
  graphData = [],
  liveStreamingDataRef,
  children,
}: {
  graphData: GraphData
  liveStreamingDataRef: MutableRefObject<HTMLDivElement | null>
  children?: React.ReactNode
}) {
  const svgRef = useRef<SVGSVGElement>(null)
  const { width } = useSize(liveStreamingDataRef)
  const [columns, setColumns] = useState<Columns>([])

  const timestamps = [...new Set(graphData.map((column) => column.timestamp))]

  const maxValue = Math.max(...graphData.map((column) => column.value))

  function refresh() {
    const offsetXLabel = getOffsetXLabel(maxValue, liveStreamingDataRef.current)

    const columnWidth =
      (svgRef.current?.clientWidth || 1) / graphData.length - offsetXLabel / 2

    const nextColumns = graphData.map((column, i) => ({
      timestamp: column.timestamp,
      x: columnWidth * (i + offsetXLabel),
      y: 0,
      height:
        maxValue !== 0
          ? (column.value / maxValue) * (svgRef.current?.clientHeight || 1)
          : 1,
      width: columnWidth,
      left: columnWidth * i + columnWidth / 2,
      value: column.value,
    }))

    setColumns(nextColumns)
  }

  useLayoutEffect(() => {
    refresh()
  }, [liveStreamingDataRef.current?.offsetWidth, width])

  const context = {
    svgRef,
    columns,
    maxValue,
    timestamps,
    liveStreamingDataRef,
  }

  return (
    <GraphContext.Provider value={context}>
      <Container>
        <ContentDiv data-testid="connectivity-graph">
          <Svg />
          <Labels />
          {children}
        </ContentDiv>
      </Container>
    </GraphContext.Provider>
  )
}

const Container = styled.div({
  display: 'flex',
  flexFlow: 'column',
  fontSize: '11px',
  overflow: 'hidden !important',
  flex: '1',
  position: 'relative',
})

const ContentDiv = styled.div({
  display: 'flex',
})

function getOffsetXLabel(maxValue: number, div?: HTMLDivElement | null) {
  if (div != null) {
    const containerWidth = div.offsetWidth
    const scaleFactor = containerWidth / 400
    return maxValue.toString().length / scaleFactor
  } else {
    return 0
  }
}
