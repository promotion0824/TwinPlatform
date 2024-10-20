import { render, screen, waitFor } from '@testing-library/react'
import MapView from './MapView'
import { Basemap } from './types'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import {
  createWebMap,
  createMapView,
  addWidgetsToView,
} from './utils/gisMapUtils'

jest.mock('./utils/gisMapUtils', () => ({
  createWebMap: jest.fn(),
  createMapView: jest.fn(),
  addWidgetsToView: jest.fn(),
}))

const server = setupServer()

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.clearAllMocks()
})
afterAll(() => server.close())

describe('MapView', () => {
  test('expect to see loading spinner and gis util functions not to be called when authorizing', async () => {
    setupServerToDelay()
    setup()

    expect(
      await screen.findByRole('img', { name: 'loading' })
    ).toBeInTheDocument()
    assertFunctionsToBeCalled(
      [createMapView, createWebMap, addWidgetsToView],
      false
    )
  })

  test('expect gis util functions to be called and GIS map to be visible when esriAuth succeeds', async () => {
    setupServerToSucceed()
    setup()

    await waitFor(() => {
      expect(
        screen.queryByRole('img', { name: 'loading' })
      ).not.toBeInTheDocument()
      assertFunctionsToBeCalled(
        [createMapView, createWebMap, addWidgetsToView],
        true
      )
    })

    expect(
      screen.getByRole('application', { name: 'gis map' })
    ).toBeInTheDocument()
  })

  test('expect to see error message and gis util functions not to be called when error happens during auth', async () => {
    setupServerToReject()
    setup()

    expect(
      await screen.findByText('plainText.errorOccurred')
    ).toBeInTheDocument()

    assertFunctionsToBeCalled(
      [createMapView, createWebMap, addWidgetsToView],
      false
    )
  })

  test('expect to see token missing error message and gis util functions not to be called when token is not returned', async () => {
    server.use(
      rest.get('/api/sites/:siteId/arcGisToken', (_req, res, ctx) =>
        res(ctx.json({ ...esriAuthResponse, token: undefined }))
      )
    )
    setup()

    expect(
      await screen.findByText('plainText.tokenIsMissing')
    ).toBeInTheDocument()

    assertFunctionsToBeCalled(
      [createMapView, createWebMap, addWidgetsToView],
      false
    )
  })
})

const setup = () =>
  render(
    <MapView
      site={{ id: 'site-1', webMapId: 'id-1' }}
      basemap={Basemap.dark_gray_vector}
    />,
    {
      wrapper: BaseWrapper,
    }
  )

const setupServerToReject = () =>
  server.use(
    rest.get('/api/sites/:siteId/arcGisToken', (_req, res, ctx) =>
      res((ctx.status(400), ctx.json({ message: 'FETCH ERROR' })))
    )
  )

const setupServerToSucceed = () =>
  server.use(
    rest.get('/api/sites/:siteId/arcGisToken', (_req, res, ctx) =>
      res(ctx.json(esriAuthResponse))
    )
  )

const setupServerToDelay = () =>
  server.use(
    rest.get('/api/sites/:siteId/arcGisToken', (_req, res, ctx) =>
      res(ctx.delay(), ctx.json(esriAuthResponse))
    )
  )

const esriAuthResponse = {
  gisBaseUrl: 'testUrl',
  token: 1234567,
  authRequiredPaths: ['/', '/path'],
  gisPortalPath: '/portal',
}
const assertFunctionsToBeCalled = (functionsList, isCalled: boolean) => {
  functionsList.forEach((func) => {
    if (isCalled) {
      expect(func).toBeCalled()
    } else {
      expect(func).not.toBeCalled()
    }
  })
}
