/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { MemoryRouter, Route } from 'react-router'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { renderHook, act, waitFor } from '@testing-library/react'
import { ReactQueryStubProvider } from '@willow/common'
import ManageConnectorsProvider, {
  useManageConnectors,
} from '../providers/ManageConnectorsProvider'
import { makeTelemetry } from '../utils/tests'

const siteId1 = 'siteId1'
const connectorId1 = 'connector1'
const expectedLastHourCount = 3901162
const expectedTotalCapabilitiesCount = 5
const expectedDisabledCapabilitiesCount = 2

const connectorA = {
  connectorId: connectorId1,
  connectorName: connectorId1,
  siteId: siteId1,
  currentSetState: 'ENABLED',
  currentStatus: 'ONLINE',
  telemetry: makeTelemetry(expectedLastHourCount),
  totalCapabilitiesCount: expectedTotalCapabilitiesCount,
  disabledCapabilitiesCount: expectedDisabledCapabilitiesCount,
  hostingDevicesCount: 1,
  totalTelemetryCount: 1,
  status: [{ timestamp: '' }],
}

const siteConnectorsStatsResponse = [connectorA]

const connectorTypesResponse = [
  {
    id: 'f79808ad-bc7f-4c13-9d85-01cd889badc9',
    name: 'DefaultDeltaVConnector',
    columns: [],
  },
  {
    id: '382870c4-0f9c-4c53-92a5-0612090dada5',
    name: 'TestConnectorType',
    columns: [],
  },
  {
    id: 'c7ad853a-7e70-424b-af4d-206c7496aa82',
    name: 'DefaultOpcDaConnector2',
    columns: [
      { name: 'Port', type: 'Number', isRequired: true },
      { name: 'Username', type: 'String', isRequired: true },
      { name: 'MaxNumberThreads', type: 'Number', isRequired: true },
      { name: 'MaxRetry', type: 'Number', isRequired: true },
      { name: 'Password', type: 'String', isRequired: true },
      { name: 'InitDelay', type: 'Number', isRequired: true },
      { name: 'Url', type: 'String', isRequired: true },
      { name: 'ThreadsPerNetwork', type: 'Number', isRequired: true },
      { name: 'Interval', type: 'Number', isRequired: true },
      { name: 'Timeout', type: 'Number', isRequired: true },
      { name: 'MaxDevicesPerThread', type: 'Number', isRequired: true },
    ],
  },
]

const connectorResponse = {
  id: connectorId1,
  name: 'Connector 1',
  siteId: '993a3866-d5e4-4239-b2a4-7ce4cb1e4dc9',
  connectorTypeId: '031eded8-2519-42b2-8dd9-4c3337727ee7',
  errorThreshold: 10,
  isEnabled: true,
  isLoggingEnabled: false,
  connectionType: 'publicapi',
  pointsCount: 0,
}

const handlers = [
  rest.get(
    `/api/sites/${siteId1}/livedata/stats/connectors`,
    (_req, res, ctx) => res(ctx.json(siteConnectorsStatsResponse))
  ),
  rest.get(`/api/sites/${siteId1}/connectorTypes`, (_req, res, ctx) =>
    res(ctx.json(connectorTypesResponse))
  ),
  rest.get(
    `/api/sites/${siteId1}/connectors/${connectorId1}`,
    (_req, res, ctx) => res(ctx.json(connectorResponse))
  ),
]
const server = setupServer(...handlers)

const setupServerWithReject = () =>
  server.use(
    rest.get(
      `/api/sites/${siteId1}/livedata/stats/connectors`,
      (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    ),
    rest.get(`/api/sites/${siteId1}/connectorTypes`, (_req, res, ctx) =>
      res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    ),
    rest.get(
      `/api/sites/${siteId1}/connectors/${connectorId1}`,
      (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    )
  )

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('ManageConnectorsProvider', () => {
  function Wrapper({ children }) {
    return (
      <MemoryRouter
        initialEntries={[
          `/admin/portfolios/152b987f-0da2-4e77-9744-0e5c52f6ff3d/sites/${siteId1}/connectors`,
        ]}
      >
        <Route path="/admin/portfolios/:portfolioId/sites/:siteId/connectors">
          <ReactQueryStubProvider>
            <ManageConnectorsProvider>{children}</ManageConnectorsProvider>
          </ReactQueryStubProvider>
        </Route>
      </MemoryRouter>
    )
  }
  test('should provide error when exception error happens', async () => {
    setupServerWithReject()
    const { result } = renderHook(() => useManageConnectors(), {
      wrapper: Wrapper,
    })

    await waitFor(() =>
      expect(result.current.connectorsStatsQuery!.status).toEqual('error')
    )

    expect(result.current.selectedConnector).not.toBeDefined()
    expect(result.current.connectorsStatsQuery!.isError).toBeTruthy()
    expect(result.current.connectorTypesData).toBeEmpty()
    expect(result.current.connectorQuery!.isIdle).toBeTruthy()
    expect(result.current.connectorDetails!.pointsCount).toEqual(0)

    // `/api/sites/${siteId1}/connectors/${connectorId1}` (connectorQuery) will not be called
    //  when connectorsStatsQuery.status is error as there can never be connector in
    //  connectorsStatsQuery.data with its connectorId matching connectorId coming from url search params
    expect(result.current.connectorQuery!.status).toBe('idle')
  })

  test('should return correct values', async () => {
    const { result } = renderHook(() => useManageConnectors(), {
      wrapper: Wrapper,
    })

    await waitFor(() =>
      expect(result.current.connectorsStatsQuery!.status).toEqual('success')
    )

    expect(result.current.selectedConnector).not.toBeDefined()
    expect(result.current.connectorsStatsQuery!.isSuccess).toBeTruthy()
    expect(result.current.connectorTypesData).toEqual(connectorTypesResponse)
    expect(result.current.connectorQuery!.isIdle).toBeTruthy()
    expect(result.current.connectorDetails!.pointsCount).toEqual(0)

    act(() => {
      result.current.setConnectorId!(connectorA.connectorId)
    })

    expect(result.current.selectedConnector).toEqual(connectorA)

    // `/api/sites/${siteId1}/connectors/${connectorId1}` is called when connector is selected via setSelectConnector
    await waitFor(() =>
      expect(result.current.connectorQuery!.status).toEqual('success')
    )

    expect(result.current.connectorQuery!.isSuccess).toBeTruthy()
    expect(result.current.connectorDetails).toEqual({
      ...connectorResponse,
      telemetry: makeTelemetry(expectedLastHourCount),
      pointsCount:
        expectedTotalCapabilitiesCount - expectedDisabledCapabilitiesCount, // 5 - 2 = 3
    })
  })
})
