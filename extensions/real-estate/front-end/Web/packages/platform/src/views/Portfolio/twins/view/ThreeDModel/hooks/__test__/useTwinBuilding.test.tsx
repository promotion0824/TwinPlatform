/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { renderHook, waitFor } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { useTwinBuilding, UseTwinBuildingParams } from '../useTwinBuilding'
import makeModelList from './utils'

const siteId = 'site1'
const urn = '123'

const access_token =
  'eyJhbGciOiJSUzI1NiIsImtpZCI6IlU3c0dGRldUTzlBekNhSzBqZURRM2dQZXBURVdWN2VhIn0.eyJzY29wZSI6WyJ2aWV3YWJsZXM6cmVhZCJdLCJjbGllbnRfaWQiOiJaT0VrcWhESFI4MFpLU3k1Z1NTMzN4UGlUOHBXNFJGSCIsImF1ZCI6Imh0dHBzOi8vYXV0b2Rlc2suY29tL2F1ZC9hand0ZXhwNjAiLCJqdGkiOiJzT0ZOVWlDVEQzQXZFQm5lZlFsZjJ0dnY2ZFkxV2I2OHlLTDlzNDN5c1NRdExtb0lnd1l1a09xZG9JUlB6QkNGIiwiZXhwIjoxNjUyNzE1NDg3fQ.bt47lnrdrGxn86kRORzYUFR-WRU27h2oKto3CZd8T43fKCjszG2JqrZeFnEusNw0pX4t7c_QmRf-WbjmlCy-raJjHkxZBqoc9bgJx2hfXOCuPqWGMASYFuVeaXsat4xFPawgCVk0_yYI1gePLwsjkQVrPo8zY4rJhbeiDsTYt0-CndKT-k1sjgNVke93zqDftRE6oAUAq3KIzy-jNfz3OihIIsxFo3LOsxLr1Uvuohn8HLR1O2ebIaqbAXL5km7JYAyzUn9-sFElnXvpJcWyzJ5gwQSallErW0xp9-I9kN7vSQHkbgJm1538NCMGm5Tg6DN28S3LK5XrkP88mznT9A'
const token_type = 'Bearer'

const useGet3dModuleResponse = {
  canBeDeleted: true,
  groupType: 'Base',
  id: '5da9dbda-d6b1-4bde-a959-3f428698e7ba',
  isDefault: true,
  moduleGroup: {
    id: '9d014348-67c9-403c-8da2-219d1da76b42',
    name: 'Base',
    sortOrder: 0,
    siteId: '934638e3-4bd7-4749-bd52-bd6e47d0fbb2',
  },
  moduleTypeId: 'b36c6af6-f4b1-47da-bdaa-9524846ad3ac',
  name: 'Architecture.nwd',
  sortOrder: 0,
  typeName: 'Architecture',
  url: urn,
  visualId: '00000000-0000-0000-0000-000000000000',
}
const makeAutoDeskData = (access_token, token_type) => ({
  access_token,
  token_type,
})

const floors = [
  {
    id: '4f2bd55f-8fa9-4922-84d4-93d741e58ed3',
    name: 'BLDG',
    code: 'BLDG',
    geometry: '[]',
    isSiteWide: true,
    modelReference: '7ea923ae-b760-4b76-aaaa-6355825b07fe',
  },
  {
    id: '568547a7-68b9-4419-97f4-9d269da813e0',
    name: 'L36',
    code: 'L36',
    geometry: '[]',
    isSiteWide: false,
  },
  {
    id: '6a2caed8-6ec9-4f63-9a00-f8d2c7356219',
    name: 'L35',
    code: 'L35',
    geometry: '',
    isSiteWide: false,
  },
]

const handlers = [
  rest.get(`/api/sites/${siteId}/module`, (_req, res, ctx) =>
    res(ctx.json(useGet3dModuleResponse))
  ),
  rest.get(`/api/sites/${siteId}/floors`, (_req, res, ctx) =>
    res(ctx.json(floors))
  ),
  rest.get(
    `/api/sites/${siteId}/floors/:floorId/layerGroups`,
    (req, res, ctx) => {
      if (typeof req.params.floorId === 'string') {
        return res(
          ctx.json(
            makeModelList({
              firstModelIsDefault: true,
              secondModelIsDefault: true,
              thirdModelIsDefault: true,
              forthModelIsDefault: false,
              fifthModelIsDefault: false,
              fifthTypeName: 'Electrical',
            })
          )
        )
      }
    }
  ),
  rest.get(`/api/sites/${siteId}/preferences/moduleGroups`, (_req, res, ctx) =>
    res(
      ctx.json({
        sortOrder2d: [],
        sortOrder3d: ['id-3', 'id-1', 'id-2', 'id-5', 'id-4'],
      })
    )
  ),
  rest.get('/api/sites/:siteId/module', (req, res, ctx) => {
    const id = req.params.siteId
    return res(
      ctx.json({
        // Should all these ids be the same? Almost certainly not, but this is
        // a well-formed response at least.
        id,
        name: 'string',
        visualId: id,
        url: '123',
        sortOrder: 0,
        canBeDeleted: true,
        isDefault: true,
        typeName: '123',
        groupType: '123',
        moduleTypeId: id,
        moduleGroup: {
          id: id,
          name: '123',
          sortOrder: 0,
          siteId: siteId,
        },
      })
    )
  }),
]
const server = setupServer(...handlers)
const setupServerWithData = (progress: string) =>
  server.use(
    rest.get(
      `https://developer.api.autodesk.com/modelderivative/v2/designdata/${urn}/manifest`,
      (_req, res, ctx) =>
        res(
          ctx.json({
            progress,
            derivatives: [
              {
                hasThumbnail: 'true',
                children: [],
                name: 'Architecture_20220308005050.nwd',
                progress: 'complete',
                outputType: 'svf',
                properties: {
                  'Document Information': {
                    'Navisworks File Creator': 'nwexportrevit',
                  },
                },
                status: 'success',
              },
            ],
          })
        )
    )
  )

const setupServerWithReject = () =>
  server.use(
    rest.get(
      `https://developer.api.autodesk.com/modelderivative/v2/designdata/${urn}/manifest`,
      (_req, res, ctx) =>
        res(ctx.status(422), ctx.json({ message: 'FETCH ERROR' }))
    )
  )

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('useTwinBuilding', () => {
  test('should provide error when exception error happens', async () => {
    setupServerWithReject()

    const { result } = renderHook(
      (props: UseTwinBuildingParams) =>
        useTwinBuilding({
          siteId: props.siteId,
          autoDeskData: props.autoDeskData,
          options: props.options,
        }),
      {
        wrapper: Wrapper,
        initialProps: {
          siteId,
          autoDeskData: makeAutoDeskData(access_token, token_type),
          options: { enabled: true },
        },
      }
    )

    await waitFor(() => result.current.status === 'error')

    expect(result.current.error).toBeDefined()
    expect(result.current.data.building3dModels).toEqual([])
    expect(result.current.data.is3dTabForBuildingEnabled).toBeFalsy()
    expect(result.current.data.siteBuilding3dModel).toEqual([])
    expect(result.current.data.building3dModelsIds).toBe('')
    expect(result.current.data.buildingDefaultUrns).toEqual([])
  })

  test('Should return empty results when no siteId or/and autoDeskData or/and options enabled is disabled specified', async () => {
    setupServerWithData('complete')

    const { result, rerender } = renderHook(
      (props: UseTwinBuildingParams) =>
        useTwinBuilding({
          siteId: props.siteId,
          autoDeskData: props.autoDeskData,
          options: props.options,
        }),
      {
        wrapper: Wrapper,
        initialProps: {
          siteId: undefined,
          autoDeskData: undefined,
          options: { enabled: false },
        },
      }
    )

    expect(result.current.data.building3dModels).toEqual([])
    expect(result.current.data.is3dTabForBuildingEnabled).toBeFalsy()
    expect(result.current.data.siteBuilding3dModel).toEqual([])
    expect(result.current.data.building3dModelsIds).toBe('')
    expect(result.current.data.buildingDefaultUrns).toEqual([])

    rerender({
      siteId: undefined,
      autoDeskData: makeAutoDeskData(access_token, token_type),
      options: { enabled: true },
    })

    expect(result.current.data.building3dModels).toEqual([])
    expect(result.current.data.is3dTabForBuildingEnabled).toBeFalsy()
    expect(result.current.data.siteBuilding3dModel).toEqual([])
    expect(result.current.data.building3dModelsIds).toBe('')
    expect(result.current.data.buildingDefaultUrns).toEqual([])

    rerender({
      siteId,
      options: { enabled: false },
      autoDeskData: makeAutoDeskData(access_token, token_type),
    })

    expect(result.current.data.building3dModels).toEqual([])
    expect(result.current.data.is3dTabForBuildingEnabled).toBeFalsy()
    expect(result.current.data.siteBuilding3dModel).toEqual([])
    expect(result.current.data.building3dModelsIds).toBe('')
    expect(result.current.data.buildingDefaultUrns).toEqual([])
  })

  // under ideal condition, we should check manifest for every single model's uploading progress;
  // however, that would considerably increase complexity and most of site floor models
  // were uploaded for some time now, so we restrain manifest progress checking only on
  // building model
  test('Should return results including only floor models but not building model when only siteId, options specified, but no autoDeskData', async () => {
    setupServerWithData('complete')

    const { result } = renderHook(
      () =>
        useTwinBuilding({
          siteId,
          autoDeskData: undefined,
          options: { enabled: true },
        }),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expectModelsToBeOnlyFloorModels(result)
    })
  })

  test('Should return results including only floor models but not building model when siteId, options, autodesk is specified, but autodesk`s manifest is not complete', async () => {
    setupServerWithData('11%')

    const { result } = renderHook(
      () =>
        useTwinBuilding({
          siteId,
          autoDeskData: makeAutoDeskData(access_token, token_type),
          options: { enabled: true },
        }),
      {
        wrapper: Wrapper,
      }
    )
    await waitFor(() => {
      expect(result.current.status).toEqual('success')
      expect(result.current.data).not.toBeUndefined()
    })

    expectModelsToBeOnlyFloorModels(result)
  })

  test('Should return results when siteId, options, autodesk is specified, and autodesk`s manifest is complete', async () => {
    setupServerWithData('complete')

    const expectedSiteBuilding3dModel = {
      ...useGet3dModuleResponse,
      typeName: 'labels.site',
      isUngroupedLayer: true,
    }

    const { result } = renderHook(
      () =>
        useTwinBuilding({
          siteId,
          autoDeskData: makeAutoDeskData(access_token, token_type),
          options: { enabled: true },
        }),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.status).toEqual('success')
      expect(result.current.data).not.toBeUndefined()
    })

    expect(result.current.data.building3dModels).toEqual([
      expectedSiteBuilding3dModel,
      {
        id: 'id-3',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-3',
        canBeDeleted: true,
        isDefault: false,
        moduleTypeId: 'id-3',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-1',
        visualId: '00000000-0000-0000-0000-000000000000',
        url: 'url-1',
        sortOrder: 1,
        canBeDeleted: true,
        isDefault: false,
        moduleTypeId: 'id-1',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-2',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-2',
        canBeDeleted: true,
        isDefault: false,
        moduleTypeId: 'id-2',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-5',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-5',
        canBeDeleted: true,
        isDefault: false,
        typeName: 'Electrical',
        moduleTypeId: 'id-5',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-4',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-4',
        canBeDeleted: true,
        isDefault: false,
        moduleTypeId: 'id-4',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
    ])
    expect(result.current.data.is3dTabForBuildingEnabled).toBeTruthy()
    expect(result.current.data.siteBuilding3dModel).toEqual([
      expectedSiteBuilding3dModel,
    ])
    expect(result.current.data.building3dModelsIds).toBe(
      '5da9dbda-d6b1-4bde-a959-3f428698e7ba,id-3,id-1,id-2,id-5,id-4'
    )
    expect(result.current.data.buildingDefaultUrns).toEqual([
      urn,
      'url-3',
      'url-1',
      'url-2',
      'url-5',
      'url-4',
    ])
  })
})

const expectModelsToBeOnlyFloorModels = (result) => {
  expect(result.current.data.building3dModels.map((model) => model.id)).toEqual(
    ['id-3', 'id-1', 'id-2', 'id-5', 'id-4']
  )
  expect(result.current.data.is3dTabForBuildingEnabled).toBeTruthy()
  expect(result.current.data.siteBuilding3dModel).toEqual([])
  expect(result.current.data.building3dModelsIds).toBe(
    'id-3,id-1,id-2,id-5,id-4'
  )
  expect(result.current.data.buildingDefaultUrns).toEqual([
    'url-3',
    'url-1',
    'url-2',
    'url-5',
    'url-4',
  ])
}
