/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { renderHook, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetConnectivityData from '../hooks/useGetConnectivityData'
import SitesProvider from '../../../../../providers/sites/SitesStubProvider'

const customerId = 'customer1'
const portfolioId = 'portfolio1'

const makeTelemetry = (lastHourCount: number) => {
  const makeTimeStamp = (hour: number) => `2022-06-16T${hour % 24}:00:00.000Z`
  const ret = Array.from(Array(49)).map((_, i) => ({
    timeStamp: makeTimeStamp(i),
    totalTelemetryCount: 0,
  }))

  ret[0].totalTelemetryCount = lastHourCount
  return ret
}

const makeConnector = (
  connectorId,
  siteId,
  currentSetState,
  currentStatus,
  telemetry
) => ({
  connectorId,
  siteId,
  currentSetState,
  currentStatus,
  telemetry,
})

const makeSite = (id, name, country, state, suburb, type) => ({
  id,
  name,
  country,
  state,
  suburb,
  type,
})
const siteId1 = 'siteId1'
const siteId2 = 'siteId2'
const siteId3 = 'siteId3'

const site1 = makeSite(siteId1, siteId1, 'Canada', 'AB', 'Calgary', 'Office')
const site2 = makeSite(siteId2, siteId2, 'USA', 'NY', 'New York', 'Office')
const site3 = makeSite(siteId3, siteId3, 'AUS', 'NSW', 'Sydney', 'Office')

const sites = [site1, site2, site3]

const connectorA = makeConnector(
  'A',
  siteId1,
  'DISABLED',
  'DISABLED',
  makeTelemetry(0)
)
const connectorB = makeConnector(
  'B',
  siteId1,
  'ENABLED',
  'ONLINE',
  makeTelemetry(3901162)
)
const connectorC = makeConnector(
  'C',
  siteId1,
  'ENABLED',
  'OFFLINE',
  makeTelemetry(0)
)

const connectorD = makeConnector(
  'D',
  siteId2,
  'ENABLED',
  'READY',
  makeTelemetry(1903019)
)

const connectorE = makeConnector(
  'E',
  siteId3,
  'ENABLED',
  'UNKNOWN',
  makeTelemetry(0)
)
const connectorF = makeConnector(
  'F',
  siteId3,
  'ENABLED',
  'OFFLINE',
  makeTelemetry(0)
)

const makeConnectorStats = (siteId, connectors) => ({
  siteId,
  connectorStats: connectors,
})
const response = [
  makeConnectorStats(siteId1, [connectorA, connectorB, connectorC]),
  makeConnectorStats(siteId2, [connectorD]),
  makeConnectorStats(siteId3, [connectorE, connectorF]),
]

const handler = [
  rest.post(
    `/api/customers/${customerId}/portfolio/${portfolioId}/livedata/stats/connectors`,
    (_req, res, ctx) => res(ctx.json(response))
  ),
]
const server = setupServer(...handler)

const setupServerWithReject = () =>
  server.use(
    rest.post(
      `/api/customers/${customerId}/portfolio/${portfolioId}/livedata/stats/connectors`,
      (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    )
  )

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('useGetConnectivityData', () => {
  function Wrapper({ children }) {
    return (
      <BaseWrapper>
        <SitesProvider sites={sites}>{children}</SitesProvider>
      </BaseWrapper>
    )
  }
  test('should provide error when exception error happens', async () => {
    setupServerWithReject()

    const { result } = renderHook(
      () => useGetConnectivityData(customerId, portfolioId),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.isLoading).toBeFalse()
      expect(result.current.isError).toBeTruthy()
    })

    expect(result.current.data).toBeUndefined()
  })

  test('should return correct values', async () => {
    const expectedConnectivityDataTable = [
      {
        siteId: siteId1,
        name: siteId1,
        state: 'AB',
        country: 'Canada',
        city: 'Calgary',
        assetClass: 'Office',
        dataIn: 3901162,
        isOnline: false,
        connectorStatus: '1/2',
        color: 'orange',
      },
      {
        siteId: siteId2,
        name: siteId2,
        state: 'NY',
        country: 'USA',
        city: 'New York',
        assetClass: 'Office',
        dataIn: 1903019,
        isOnline: true,
        connectorStatus: '1/1',
        color: 'green',
      },
      {
        siteId: siteId3,
        name: siteId3,
        state: 'NSW',
        country: 'AUS',
        city: 'Sydney',
        assetClass: 'Office',
        dataIn: 0,
        isOnline: false,
        connectorStatus: '0/2',
        color: 'gray',
      },
    ]

    const { result } = renderHook(
      () => useGetConnectivityData(customerId, portfolioId),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(result.current!.isSuccess).toBeTruthy()
    })

    expect(result.current!.data!.connectivityTableData).toEqual(
      expectedConnectivityDataTable
    )
    expect(result.current!.data!.renderMetricObject['Sites online'].count).toBe(
      (1).toLocaleString()
    )
    expect(
      result.current!.data!.renderMetricObject['Connection errors'].count
    ).toBe((3).toLocaleString())
  })
})
