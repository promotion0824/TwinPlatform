/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { renderHook, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { ReactQueryStubProvider } from '@willow/common'
import useGroupedSensors from '../useGroupedSensors'

const siteId = '123'
const twinId = '1111'

const makeDevice = (id: string, name: string) => ({ id, name })
const makeConnector = (id: string, name: string) => ({ id, name })
const makeSensor = (
  externalId: string,
  name: string,
  connector?: ReturnType<typeof makeConnector>,
  device?: ReturnType<typeof makeDevice>
) => ({
  name,
  externalId,
  device,
  properties: {
    connectorID: connector?.id ? { value: connector.id } : undefined,
    siteID: {
      value: siteId,
    },
  },
  connectorName: connector?.name,
})

const deviceOne = makeDevice('d1', 'Device One')
const connectorOne = makeConnector('c1', 'Connector One')
const connectorTwo = makeConnector('c2', 'Connector Two')

const sensorA = makeSensor('sensor-A-id', 'Sensor A', connectorOne, deviceOne)
const sensorB = makeSensor('sensor-B-id', 'Sensor B', connectorTwo)
const sensorC = makeSensor('sensor-C-id', 'Sensor C', connectorOne)
const sensorD = makeSensor('sensor-D-id', 'Sensor D', connectorTwo)
const sensorE = makeSensor('sensor-E-id', 'Sensor E - No Connector')

const server = setupServer(
  rest.get(`/api/sites/${siteId}/twins/${twinId}/points`, (_req, res, ctx) =>
    res(ctx.json([sensorA, sensorB, sensorC, sensorD, sensorE]))
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('useGroupedSensors', () => {
  test('Should disable query when no siteId and twinId specified', () => {
    const { result, rerender } = renderHook(
      (props: { siteId?: string; twinId?: string }) =>
        useGroupedSensors(props?.siteId, props?.twinId),
      {
        wrapper: ReactQueryStubProvider,
        initialProps: {
          siteId: undefined,
          twinId: undefined,
        },
      }
    )

    expect(result.current.isLoading).toBeFalsy()

    rerender({ siteId: '', twinId: 'twin-1' })
    expect(result.current.isLoading).toBeFalsy()

    rerender({ siteId: 'site-1', twinId: '' })
    expect(result.current.isLoading).toBeFalsy()
  })

  test('Should transform data', async () => {
    const { result } = renderHook(() => useGroupedSensors(siteId, twinId), {
      wrapper: ReactQueryStubProvider,
    })

    expect(result.current.isLoading).toBeTruthy()

    await waitFor(() => {
      expect(result.current.data).toEqual({
        d1_c1: [sensorA],
        _c1: [sensorC],
        _c2: [sensorB, sensorD],
        _: [sensorE],
      })
    })
  })
})
