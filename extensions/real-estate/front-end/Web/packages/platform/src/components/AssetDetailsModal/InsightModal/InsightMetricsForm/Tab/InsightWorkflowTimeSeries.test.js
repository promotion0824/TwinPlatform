import React from 'react'
import { render, screen, waitFor, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import InsightWorkflowTimeSeries from './InsightWorkflowTimeSeries'
import { useAnalytics } from '../../../../../../../ui/src/providers/AnalyticsProvider/AnalyticsContext'
import SelectedPointsProvider from '../../../../MiniTimeSeries/SelectedPointsProvider'

jest.mock(
  '../../../../../../../ui/src/providers/AnalyticsProvider/AnalyticsContext'
)
const mockedUseAnalytics = jest.mocked(useAnalytics)

const handler = [
  rest.get('/api/sites/:siteId/equipments/:equipmentId', (_req, res, ctx) =>
    res(ctx.json(equipment))
  ),
  rest.get(`/api/timezones`, (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/sites/:siteId/points/:pointId/liveData', (_req, res, ctx) =>
    res(ctx.json(liveData))
  ),
]
const server = setupServer(...handler)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  mockedUseAnalytics.mockClear()
})
afterAll(() => server.close())

describe('Insight Workflow Mini-TimeSeries Overlays', () => {
  test('expect to see the shading areas', async () => {
    const shadedDurations = [
      {
        start,
        end,
        color: 'red',
      },
    ]

    setupWrapper({
      shadedDurations,
      insight: { ...defaultInsight, twinId: 'vav-box-1' },
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await assertOnShadedGraph()
  })

  test('expect to see the shading areas when twinInfo is defined', async () => {
    const pointName = 'Air Flow Sp'
    const shadedDurations = [
      {
        start,
        end,
        color: 'red',
      },
    ]

    setupWrapper({
      shadedDurations,
      insight: { ...defaultInsight, twinId: 'vav-box-1' },
      twinInfo: {
        isInsightPointsLoading: false,
        insightPoints: [
          {
            pointTwinId:
              'INV-60MP-VAV-PS-L01-02-AirFlowSp-1_112053_121634924-VavSuSpAirFl_Present_Value',
            trendId: pointIdTurnedOnByDefault,
            entityId: pointIdTurnedOnByDefault,
            name: pointName,
            externalId:
              'System1:GmsDevice_1_112053_121634924@VavSuSpAirFl_Present_Value',
            unit: 'l/s',
            externalPointId:
              'System1:GmsDevice_1_112053_121634924@VavSuSpAirFl_Present_Value',
            type: 'InsightPoint',
          },
        ],
        twinId: 'vav-box-1',
        twinName: 'vav-box-1',
      },
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    // find the point option and click it
    expect(screen.queryByText(pointName)).not.toBeNull()
    await act(async () => {
      userEvent.click(screen.queryByText(pointName))
    })

    await assertOnShadedGraph()
  })

  test('expect to see not available text when twinId and equipmentId are NOT defined on the insight', async () => {
    setupWrapper({
      insight: { ...defaultInsight, twinId: null, equipmentId: null },
    })

    await waitFor(() => {
      expect(screen.getByText(notAvailable)).toBeInTheDocument()
    })
  })
})

const start = '2021-01-01T00:00:00.000Z'
const end = '2021-02-01T00:00:00.000Z'
const notAvailable = 'plainText.timeSeriesNotAvailable'

const Wrapper = ({ children }) => (
  <BaseWrapper
    user={{
      options: {
        exportedTimeMachineCsvs: [],
      },
    }}
    sites={[{ id: equipment.siteId, name: 'site1' }]}
  >
    <SelectedPointsProvider>{children}</SelectedPointsProvider>
  </BaseWrapper>
)

const setupWrapper = ({
  shadedDurations,
  insight = defaultInsight,
  twinInfo,
}) => {
  const mockedTrack = jest.fn()
  mockedUseAnalytics.mockImplementation(() => ({
    track: mockedTrack,
  }))
  // this test is not to test the line graph, but to test the visibility of shading,
  // so we mock the line graph useLayoutEffect call to avoid the error
  jest.spyOn(React, 'useLayoutEffect').mockImplementation(() => jest.fn())

  return render(
    <InsightWorkflowTimeSeries
      insight={insight}
      start={start}
      end={end}
      shadedDurations={shadedDurations}
      twinInfo={twinInfo}
    />,
    {
      wrapper: Wrapper,
    }
  )
}

const assertOnShadedGraph = async () => {
  // expect to see the graph
  await waitFor(() => {
    expect(
      screen.queryByTestId(`tab-timeSeries-graph-${pointIdTurnedOnByDefault}`)
    ).not.toBeNull()
  })

  // expect to see the shading
  await Promise.all(
    ['faulty-overlay', 'insufficient-data-overlay'].map(async (testId) => {
      await waitFor(() => {
        expect(screen.queryByTestId(testId)).not.toBeNull()
      })
    })
  )
}

const pointIdTurnedOnByDefault = '4b8aed18-cc2c-4b6a-840e-6c0ce7e4ffc3'
const pointNameTurnedOnByDefault = 'Zone Air Temp Sp'
const equipment = {
  id: 'f1f8d8a8-a5e8-4dbc-89d9-69baf4cd19db',
  name: 'VAV-CN-L11-01',
  customerId: '00000000-0000-0000-0000-000000000000',
  siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
  points: [
    {
      id: pointIdTurnedOnByDefault,
      entityId: pointIdTurnedOnByDefault,
      name: pointNameTurnedOnByDefault,
      equipmentId: 'f1f8d8a8-a5e8-4dbc-89d9-69baf4cd19db',
      externalPointId:
        'System1:GmsDevice_1_1112159_121634839@RAQual_Present_Value',
      hasFeaturedTags: true,
    },
  ],
  tags: [
    {
      name: 'SUPPLYAIR',
    },
  ],
  pointTags: [
    {
      name: 'air',
    },
  ],
}

const defaultInsight = {
  equipmentId: 'id-1',
  equipmentName: 'name-1',
  siteId: equipment.siteId,
}

const liveData = {
  timeSeriesData: [
    {
      average: 90,
      minimum: 90,
      maximum: 90,
      timestamp: '2023-05-03T00:00:00.000Z',
    },
  ],
  pointId: pointIdTurnedOnByDefault,
  pointEntityId: pointIdTurnedOnByDefault,
  pointName: pointNameTurnedOnByDefault,
  pointType: 'analog',
  unit: '%',
}
