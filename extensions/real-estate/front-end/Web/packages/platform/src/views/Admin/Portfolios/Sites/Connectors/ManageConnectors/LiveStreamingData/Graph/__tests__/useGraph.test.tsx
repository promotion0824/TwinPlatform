import { renderHook } from '@testing-library/react'
import LanguageStubProvider from '@willow/ui/providers/LanguageProvider/LanguageStubProvider'
import _ from 'lodash'
import Graph, { useGraph } from '../index'
import { constructGraphData } from '../../index'

const telemetry = [
  {
    timestamp: '2022-06-29T20:00:00.000Z',
    totalTelemetryCount: 1,
    uniqueCapabilityCount: 2,
    setState: 'enabled',
    status: 'enabled',
  },
  {
    timestamp: '2022-06-29T19:00:00.000Z',
    totalTelemetryCount: 2,
    uniqueCapabilityCount: 2,
    setState: 'enabled',
    status: 'enabled',
  },
  {
    timestamp: '2022-06-29T18:00:00.000Z',
    totalTelemetryCount: 3,
    uniqueCapabilityCount: 2,
    setState: 'enabled',
    status: 'enabled',
  },
]

describe('LiveStreamingData: useGraph', () => {
  const createWrapper = ({ graphData, liveStreamingDataRef }) =>
    function Wrapper({ children }) {
      return (
        <LanguageStubProvider>
          <Graph
            graphData={graphData}
            liveStreamingDataRef={liveStreamingDataRef}
          >
            {children}
          </Graph>
        </LanguageStubProvider>
      )
    }

  test('should return correct values', async () => {
    const liveStreamDataElement = document.createElement('div')
    const liveStreamingDataRef = { current: liveStreamDataElement }

    const graphData = constructGraphData(telemetry)
    const expectedMaxValue = 3
    const expectedTimestamps = [
      '2022-06-29T20:00:00.000Z',
      '2022-06-29T19:00:00.000Z',
      '2022-06-29T18:00:00.000Z',
    ]
    const expectedColumns = graphData.map((column) => ({
      timestamp: column.timestamp,
      value: column.value,
    }))
    const { result } = renderHook(() => useGraph(), {
      wrapper: createWrapper({ graphData, liveStreamingDataRef }),
    })

    expect(result.current.maxValue).toEqual(expectedMaxValue)
    expect(result.current.timestamps).toEqual(expectedTimestamps)

    // Unable to test following variables as they are dependent of SVG elements being rendered
    const omittedColumns = result.current.columns.map((column) =>
      _.omit(column, ['x', 'y', 'height', 'width', 'left'])
    )

    expect(omittedColumns).toEqual(expectedColumns)
  })
})
