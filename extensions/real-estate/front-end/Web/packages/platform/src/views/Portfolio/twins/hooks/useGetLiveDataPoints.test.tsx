import { renderHook, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { ReactQueryStubProvider } from '@willow/common'
import useLiveDataPoints from './useGetLiveDataPoints'

const siteId = '123'
const assetId = '1111'
const liveDataPointOne = {
  id: '1111',
  data: 'one',
}
const liveDataPointTwo = {
  id: '2222',
  data: 'two',
}

const server = setupServer(
  rest.get(
    `/api/sites/${siteId}/assets/${assetId}/pinOnLayer`,
    (req, res, ctx) =>
      res(
        ctx.json(
          req.url.searchParams.get('includeAllPoints') === 'true'
            ? {
                name: 'My asset',
                liveDataPoints: [liveDataPointOne, liveDataPointTwo],
              }
            : {}
        )
      )
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('useLiveDataPoints', () => {
  test('Should disable query when no siteId and twinId specified', () => {
    const { result, rerender } = renderHook(
      (props: { siteId?: string; twinId?: string }) =>
        useLiveDataPoints(props?.siteId, props?.twinId),
      {
        wrapper: ReactQueryStubProvider,
        initialProps: {
          siteId: undefined,
          twinId: undefined,
        },
      }
    )

    expect(result.current.isLoading).toBeFalsy()
    expect(result.current.data).toBeUndefined()

    rerender({ siteId: '', twinId: 'twin-1' })
    expect(result.current.isLoading).toBeFalsy()
    expect(result.current.data).toBeUndefined()

    rerender({ siteId: 'site-1', twinId: '' })
    expect(result.current.isLoading).toBeFalsy()
    expect(result.current.data).toBeUndefined()
  })

  test('Should transform data', async () => {
    const { result } = renderHook(() => useLiveDataPoints(siteId, assetId), {
      wrapper: ReactQueryStubProvider,
    })

    expect(result.current.isLoading).toBeTruthy()
    expect(result.current.data).toBeUndefined()

    await waitFor(() => {
      expect(result.current.isLoading).toBeFalsy()
      expect(result.current.data).toEqual({
        [liveDataPointOne.id]: liveDataPointOne,
        [liveDataPointTwo.id]: liveDataPointTwo,
      })
    })
  })
})
