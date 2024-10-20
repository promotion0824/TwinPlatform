import { act } from 'react-dom/test-utils'
import { render, screen, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import _ from 'lodash'
import Connectors from '../ConnectorsTables/Connectors'
import { makeTelemetry, Wrapper } from '../utils/tests'

const siteId1 = 'siteId1'
const connectorId1 = 'connector1'
const connectorId2 = 'connector2'

const expectedLastHourCount = 3901162
const connectorA = {
  connectorId: connectorId1,
  connectorName: connectorId1,
  siteId: siteId1,
  currentSetState: 'ENABLED',
  currentStatus: 'ONLINE',
  telemetry: makeTelemetry(expectedLastHourCount),
}

const connectorB = {
  connectorId: connectorId2,
  connectorName: connectorId2,
  siteId: siteId1,
  currentSetState: 'DISABLED',
  currentStatus: 'ONLINE',
  telemetry: makeTelemetry(0),
}

const connectorTypesResponse = [
  {
    id: 'connectorType1',
    name: 'connector',
    columns: [],
  },
]

const connectorResponse = {
  id: connectorId1,
  name: 'Connector 1',
  siteId: siteId1,
  connectorTypeId: connectorId1,
}

const siteConnectorsStatsResponse = [connectorA, connectorB]

const handlers = [
  rest.get(
    `/api/sites/${siteId1}/livedata/stats/connectors`,
    (_req, res, ctx) => res(ctx.json(siteConnectorsStatsResponse))
  ),
  rest.get(
    `/api/sites/${siteId1}/connectors/${connectorId1}`,
    (_req, res, ctx) => res(ctx.json(connectorResponse))
  ),
  rest.get(`/api/sites/${siteId1}/connectorTypes`, (_req, res, ctx) =>
    res(ctx.json(connectorTypesResponse))
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
    rest.get(
      `/api/sites/${siteId1}/connectors/${connectorId1}`,
      (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    ),
    rest.get(`/api/sites/${siteId1}/connectorTypes`, (_req, res, ctx) =>
      res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    )
  )

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('ManageConnectors: Connectors view', () => {
  function renderConnector() {
    act(() => {
      render(
        <Wrapper siteId={siteId1}>
          <Connectors />
        </Wrapper>
      )
    })
  }

  test('Should display correct texts: error state', async () => {
    setupServerWithReject()
    renderConnector()

    await waitFor(() =>
      expect(screen.queryByText('plainText.errorOccurred')).toBeInTheDocument()
    )
  })

  test('Should display correct texts: loading state', async () => {
    renderConnector()

    await waitFor(() =>
      expect(screen.getByRole('progressbar')).toBeInTheDocument()
    )
  })

  test('Should display correct texts: success state, no connectors', async () => {
    server.use(
      rest.get(
        `/api/sites/${siteId1}/livedata/stats/connectors`,
        (_req, res, ctx) => res(ctx.json([]))
      )
    )
    renderConnector()

    await waitFor(() =>
      expect(
        screen.queryByText('plainText.noConnectorsFound')
      ).toBeInTheDocument()
    )
  })

  test('Should display correct texts: success state, has connectors', async () => {
    renderConnector()

    // once successful call that have connectors. Table column headers will appear, along with the list of connectors in table.
    await waitFor(() =>
      // check table's column header
      expect(screen.queryByText('headers.connectors')).toBeInTheDocument()
    )
    expect(screen.queryByText('headers.connectorStatus')).toBeInTheDocument()
    expect(screen.queryByText('plainText.switch')).toBeInTheDocument()
    expect(screen.queryByText('plainText.dataIn')).toBeInTheDocument()

    expect(
      screen.queryByText(`(${_.capitalize('plainText.lastHour')})`)
    ).toBeInTheDocument()

    // check connectorA values in table
    expect(screen.queryByText(connectorA.connectorName)).toBeInTheDocument()
    expect(
      screen.queryByText(
        `${connectorA.telemetry[0].totalTelemetryCount.toLocaleString()} plainText.points`
      )
    ).toBeInTheDocument()

    // check connectorB values in table
    expect(screen.queryByText(connectorB.connectorName)).toBeInTheDocument()

    expect(screen.queryByText('headers.offline')).toBeInTheDocument()
  })
})
