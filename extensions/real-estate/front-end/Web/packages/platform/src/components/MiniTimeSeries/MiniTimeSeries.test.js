import { v4 as uuidv4 } from 'uuid'
import React from 'react'
import { render, screen, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import MiniTimeSeries from './MiniTimeSeries'
import { useAnalytics } from '../../../../ui/src/providers/AnalyticsProvider/AnalyticsContext'

jest.mock('../../../../ui/src/providers/AnalyticsProvider/AnalyticsContext')
const mockedUseAnalytics = jest.mocked(useAnalytics)

const handler = [
  rest.get(`/api/sites/:siteId/equipments/:equipmentId`, (_req, res, ctx) =>
    res(ctx.json(equipment))
  ),
  ...[
    `/api/sites/:siteId/points/:pointId/liveData`,
    `/api/sites/:siteId/livedata/impactScores/:pointId`,
  ].map((url) =>
    rest.get(url, (req, res, ctx) => {
      const { pointId } = req.params
      return res(
        ctx.json(
          makeLiveData({
            pointId,
            pointEntityId: pointId,
          })
        )
      )
    })
  ),
  rest.get(`/api/timezones`, (_req, res, ctx) => res(ctx.json([]))),
]
const server = setupServer(...handler)

beforeAll(() => {
  server.listen()
  // this useLayoutEffect runs inside:
  // packages\platform\src\components\TimeSeriesGraph\Graph\LineGraph\Path.js
  // which draws the path on svg, it will cause this test suite to fail;
  // for scope of this test suite, we care only if graph is rendered, so we mock it out
  jest.spyOn(React, 'useLayoutEffect').mockImplementation(() => jest.fn())
})
afterEach(() => {
  server.resetHandlers()
  mockedUseAnalytics.mockClear()
})
afterAll(() => {
  server.close()
  // this will clear mocks by spyOn, which is not necessary here, but just to be safe
  jest.restoreAllMocks()
})

// to support the business request listed here: https://dev.azure.com/willowdev/Unified/_workitems/edit/85071
// we had introduced a property of "twinInfo" that is only relevant to scope of Insight;
// this test suite is to test backward compatibility of MiniTimeSeries component without
// twinInfo
describe('MiniTimeSeries', () => {
  test('expect to see point from equipments to be turned on by default when insight has no points', async () => {
    const mockedTrack = jest.fn()
    mockedUseAnalytics.mockImplementation(() => ({
      track: mockedTrack,
    }))

    render(
      <MiniTimeSeries
        siteEquipmentId={`${equipment.siteId}_${equipment.id}`}
        time={[['2022-11-20T07:08:47.695Z', '2022-11-22T07:08:47.695Z']]}
        equipmentName={equipment.name}
      />,
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await waitFor(() => {
      expect(mockedTrack).toHaveBeenCalledWith('Point Selected', {
        name: pointNameTurnedOnByDefault,
        site: 'site1',
      })
    })

    // expect graph of the point to be turned on and off by default to be rendered
    // on initial render
    await assertPointGraphStatuses({
      visiblePointIds: [pointIdTurnedOnByDefault],
      invisiblePointIds: [pointIdTurnedOffByDefault],
    })
  })

  test('expect to see first three point from equipments to be turned on if none has "hasFeatureTags" set to true', async () => {
    mockedUseAnalytics.mockImplementation(() => ({
      track: jest.fn(),
    }))

    server.use(
      rest.get(`/api/sites/:siteId/equipments/:equipmentId`, (_req, res, ctx) =>
        res(
          ctx.json({
            ...equipment,
            points: [0, 1, 2, 3].map((i) =>
              makePoint({
                id: pointIdsWithNoFeatureTags[i],
                name: `point${i}`,
                equipmentId: equipment.id,
                externalPointId: `System${i}:GmsDevice_${i}_112053_121634924@VavSuSpAirFl_Present_Value`,
                hasFeaturedTags: false,
              })
            ),
          })
        )
      )
    )
    render(
      <MiniTimeSeries
        siteEquipmentId={`${equipment.siteId}_${equipment.id}`}
        time={[['2022-11-20T07:08:47.695Z', '2022-11-22T07:08:47.695Z']]}
        equipmentName={equipment.name}
      />,
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await assertPointGraphStatuses({
      visiblePointIds: [],
      invisiblePointIds: pointIdsWithNoFeatureTags.slice(3),
    })
  })

  test('expect to see point from insight to be turned on by default if it exists in twinInfo', async () => {
    mockedUseAnalytics.mockImplementation(() => ({
      track: jest.fn(),
    }))

    render(
      <MiniTimeSeries
        siteEquipmentId={`${equipment.siteId}_${equipment.id}`}
        time={[['2022-11-20T07:08:47.695Z', '2022-11-22T07:08:47.695Z']]}
        equipmentName={equipment.name}
        twinInfo={twinInfo}
      />,
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    // when insight returns points, we expect point from insight to take priority to be turned on by default
    await assertPointGraphStatuses({
      visiblePointIds: [
        pointIdFromInsightTakePriority,
        pointIdFromImpactScoreTakePriority,
      ],
      invisiblePointIds: [pointIdTurnedOnByDefault, pointIdTurnedOffByDefault],
    })
  })
})

const Wrapper = ({ children }) => (
  <BaseWrapper
    user={{
      options: {
        exportedTimeMachineCsvs: [],
      },
    }}
    sites={[{ id: equipment.siteId, name: 'site1' }]}
  >
    {children}
  </BaseWrapper>
)

const assertPointGraphStatuses = async ({
  visiblePointIds,
  invisiblePointIds,
}) => {
  await Promise.all(
    visiblePointIds.map(async (visiblePointId) => {
      await waitFor(() => {
        expect(
          screen.queryByTestId(`tab-timeSeries-graph-${visiblePointId}`)
        ).not.toBeNull()
      })
    })
  )
  invisiblePointIds.forEach((pointId) => {
    expect(screen.queryByTestId(`tab-timeSeries-graph-${pointId}`)).toBeNull()
  })
}

const pointIdFromInsightTakePriority = '286129e3-51f2-4dad-95fb-18b02c1e6aa3'
const pointIdFromImpactScoreTakePriority =
  'INV-60MP-Urjanet-electric-Meter_1edbd62a-a8cc-d481-b6b8-4ab5fcd24a0e-TotalConsumption'
const pointNameFromInsightTakePriority = 'Air Flow Sp'
const pointIdTurnedOnByDefault = '4b8aed18-cc2c-4b6a-840e-6c0ce7e4ffc3'
const pointIdTurnedOffByDefault = 'b35afef9-3fb4-4501-9422-d442fb5c579b'
const pointNameTurnedOnByDefault = 'Zone Air Temp Sp'
const pointNameTurnedOffByDefault = 'Air Pressure Request Cmd'
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
    {
      id: pointIdTurnedOffByDefault,
      entityId: pointIdTurnedOffByDefault,
      name: pointNameTurnedOffByDefault,
      equipmentId: 'f1f8d8a8-a5e8-4dbc-89d9-69baf4cd19db',
      externalPointId: 'System1:GmsDevice_1_1112159_20971851',
      hasFeaturedTags: false,
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

const makeLiveData = ({ pointId, pointEntityId }) => ({
  timeSeriesData: [
    {
      average: 22.3500003814697,
      minimum: 22.3500003814697,
      maximum: 22.3500003814697,
      timestamp: '2022-11-20T07:00:00.000Z',
    },
    {
      average: 22.3500003814697,
      minimum: 22.3500003814697,
      maximum: 22.3500003814697,
      timestamp: '2022-11-20T07:15:00.000Z',
    },
  ],
  pointId,
  pointEntityId,
  pointName: 'Zone Air Temp Sp',
  pointType: 'analog',
  unit: 'degC',
})

const twinInfo = {
  twinName: 'VAV-PS-L01-02',
  twinId: 'INV-60MP-VAV-PS-L01-02',
  isInsightPointsLoading: false,
  insightPoints: [
    {
      pointTwinId:
        'INV-60MP-VAV-PS-L01-02-AirFlowSp-1_112053_121634924-VavSuSpAirFl_Present_Value',
      trendId: pointIdFromInsightTakePriority,
      name: pointNameFromInsightTakePriority,
      externalId:
        'System1:GmsDevice_1_112053_121634924@VavSuSpAirFl_Present_Value',
      unit: 'l/s',
      entityId: pointIdFromInsightTakePriority,
      externalPointId:
        'System1:GmsDevice_1_112053_121634924@VavSuSpAirFl_Present_Value',
      type: 'InsightPoint',
      defaultOn: true,
    },
  ],
  impactScorePoints: [
    {
      name: 'Total Energy to Date',
      externalId: pointIdFromImpactScoreTakePriority,
      entityId: pointIdFromImpactScoreTakePriority,
      externalPointId: pointIdFromImpactScoreTakePriority,
      unit: 'kWh',
      defaultOn: true,
      type: 'ImpactScorePoint',
    },
  ],
}

const pointIdsWithNoFeatureTags = [0, 1, 2, 3].map(() => uuidv4())
const makePoint = ({
  id,
  name,
  equipmentId,
  externalPointId,
  hasFeaturedTags,
}) => ({
  id,
  entityId: id,
  name,
  equipmentId,
  externalPointId,
  hasFeaturedTags,
})
