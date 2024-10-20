import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import IdentityManager from '@arcgis/core/identity/IdentityManager'
import useEsriAuth from './useEsriAuth'

const server = setupServer()

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.clearAllMocks()
})
afterAll(() => server.close())

const esriAuthResponse = {
  gisBaseUrl: 'testUrl',
  token: 1234567,
  authRequiredPaths: ['/', '/path'],
  gisPortalPath: '/portal',
}

const siteId = 'site-1'

describe('useEsriAuth', () => {
  test('register token when fetch auth data succeeds', async () => {
    setupServerToSucceed()
    const { result } = renderHook(() => useEsriAuth(siteId), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
    })

    expect(IdentityManager.registerToken).toHaveBeenCalledTimes(
      esriAuthResponse.authRequiredPaths.length
    )

    assertRegisterToken(esriAuthResponse.authRequiredPaths)
  })
  test('do not register token when fetch auth data fail', async () => {
    setupServerToReject()
    const { result } = renderHook(() => useEsriAuth(siteId), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
    })

    expect(IdentityManager.registerToken).not.toHaveBeenCalled()
  })
  test('do not register token when fetch succeeds but token is not returned', async () => {
    server.use(
      rest.get('/api/sites/:siteId/arcGisToken', (_req, res, ctx) =>
        res(ctx.json({ ...esriAuthResponse, token: undefined }))
      )
    )
    const { result } = renderHook(() => useEsriAuth(siteId), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
    })

    expect(IdentityManager.registerToken).not.toHaveBeenCalled()
  })
})

const assertRegisterToken = (paths: string[]) => {
  paths.forEach((path, index) => {
    expect((IdentityManager.registerToken as any).mock.calls[index][0]).toEqual(
      {
        server: `${esriAuthResponse.gisBaseUrl}${path}`,
        token: esriAuthResponse.token,
        ssl: true,
      }
    )
  })
}

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
