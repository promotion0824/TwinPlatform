import axios from 'axios'
import getTwinBuildingData from './TwinBuildingDataService'
import * as autoDeskService from '../AutoDesk/AutoDeskService'
import * as floorService from '../Floors/FloorsService'
import { LayerGroupModule } from './types'

describe('getTwinBuildingData', () => {
  test('should return error when exception error happens on first service call (get3dModule)', async () => {
    jest.spyOn(axios, 'get').mockRejectedValueOnce(new Error(errorMessage))

    await expect(
      getTwinBuildingData({ siteId: 'site-1', autoDeskData: {} })
    ).rejects.toThrowError(errorMessage)
  })

  test('should return error when exception error happens on second service call (getAutoDeskFileManifest)', async () => {
    jest.spyOn(axios, 'get').mockResolvedValueOnce({ data: moduleResponseData })
    jest.spyOn(axios, 'get').mockRejectedValueOnce(new Error(errorMessage))

    await expect(
      getTwinBuildingData({ siteId: 'site-1', autoDeskData: {} })
    ).rejects.toThrowError(errorMessage)
  })

  test('should return error when exception error happens on third service call (getFloors)', async () => {
    jest.spyOn(axios, 'get').mockResolvedValueOnce({ data: moduleResponseData })
    jest
      .spyOn(autoDeskService, 'getAutoDeskFileManifest')
      .mockResolvedValueOnce({
        fileInfo: {
          urn: 'asdf',
        },
        progress: 'complete',
      })
    jest.spyOn(axios, 'get').mockRejectedValueOnce(new Error(errorMessage))

    await expect(
      getTwinBuildingData({ siteId: 'site-1', autoDeskData: {} })
    ).rejects.toThrowError(errorMessage)
  })

  test('should return error when exception error happens on forth service call (getModelsAndOrders)', async () => {
    jest.spyOn(axios, 'get').mockResolvedValueOnce({ data: moduleResponseData })
    jest
      .spyOn(autoDeskService, 'getAutoDeskFileManifest')
      .mockResolvedValueOnce({
        fileInfo: {
          urn: 'asdf',
        },
        progress: 'complete',
      })
    jest.spyOn(floorService, 'getFloors').mockResolvedValueOnce(floors)
    jest.spyOn(axios, 'get').mockRejectedValueOnce(new Error(errorMessage))

    await expect(
      getTwinBuildingData({
        siteId: 'site-2',
        autoDeskData: { access_token: 'something' },
      })
    ).rejects.toThrowError(errorMessage)
  })

  test('should return expected data including sorted models for site floor (floorModelData), when model for building exists, "isDefault" prop on each model in floorModelData is false', async () => {
    initializeSpies(
      moduleResponseData,
      makeModelList({
        firstModelIsDefault: true,
        firstTypeName: 'Electrical',
        secondModelIsDefault: false,
        secondTypeName: 'something',
        thirdModelIsDefault: false,
        thirdTypeName: 'something else',
        forthModelIsDefault: true,
        forthTypeName: 'something else else',
        fifthModelIsDefault: false,
        fifthTypeName: 'else else else',
      }) as any
    )

    const response = await getTwinBuildingData({
      siteId: 'site-2999',
      autoDeskData: { access_token: 'something' },
    })

    expect(response.isSiteBuilding3dModelLoaded).toBeTrue()
    expect(response.siteBuilding3dData).toBe(moduleResponseData)
    expect(response.floorModelData.map((model) => model.id)).toEqual([
      'id-3',
      'id-1',
      'id-2',
      'id-5',
      'id-4',
    ])
    expectModelToBeIsDefault(response.floorModelData, 'id-1', false)
    expectModelToBeIsDefault(response.floorModelData, 'id-2', false)
    expectModelToBeIsDefault(response.floorModelData, 'id-3', false)
    expectModelToBeIsDefault(response.floorModelData, 'id-4', false)
    expectModelToBeIsDefault(response.floorModelData, 'id-5', false)
  })

  test('should return expected data where floorModelData is array of models, when model for building does not exist, all "isDefault" prop stay the same on each model', async () => {
    initializeSpies(
      {} as any,
      makeModelList({
        firstModelIsDefault: true,
        firstTypeName: 'Electrical',
        secondModelIsDefault: false,
        secondTypeName: 'something',
        thirdModelIsDefault: false,
        thirdTypeName: 'something else',
        forthModelIsDefault: true,
        forthTypeName: 'something else else',
        fifthModelIsDefault: false,
        fifthTypeName: 'else else else',
      }) as any
    )

    const response = await getTwinBuildingData({
      siteId: 'site-2999',
      autoDeskData: { access_token: 'something' },
    })

    expect(response.isSiteBuilding3dModelLoaded).toBeFalse()
    expect(response.siteBuilding3dData).toMatchObject({})
    expect(response.siteBuilding3dData.url).not.toBeDefined()
    expect(
      response.floorModelData.map((model: LayerGroupModule) => model.id)
    ).toEqual(['id-3', 'id-1', 'id-2', 'id-5', 'id-4'])
    expectModelToBeIsDefault(response.floorModelData, 'id-1', true)
    expectModelToBeIsDefault(response.floorModelData, 'id-2', false)
    expectModelToBeIsDefault(response.floorModelData, 'id-3', false)
    expectModelToBeIsDefault(response.floorModelData, 'id-4', true)
    expectModelToBeIsDefault(response.floorModelData, 'id-5', false)
  })
})

const errorMessage = 'fetch error'

const moduleResponseData = {
  id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  name: 'abc.nwd',
  visualId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  url: 'ADSJFNuuaebEsbKKDJ',
  sortOrder: 0,
  canBeDeleted: true,
  typeName: 'string',
  groupType: 'string',
  moduleTypeId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  isDefault: true,
  moduleGroup: {
    id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    name: 'string',
    sortOrder: 0,
    siteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  },
}

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

const sortOrderData: { sortOrder2d?: string[]; sortOrder3d?: string[] } = {
  sortOrder2d: [],
  sortOrder3d: ['id-3', 'id-1', 'id-2', 'id-5', 'id-4'],
}

const initializeSpies = (
  building3dModel: LayerGroupModule,
  modelList: LayerGroupModule[]
) => {
  jest.spyOn(axios, 'get').mockResolvedValueOnce({ data: building3dModel })
  jest.spyOn(autoDeskService, 'getAutoDeskFileManifest').mockResolvedValueOnce({
    fileInfo: {
      urn: 'asdf',
    },
    progress: 'complete',
  })
  jest.spyOn(floorService, 'getFloors').mockResolvedValueOnce(floors)
  jest.spyOn(axios, 'get').mockResolvedValueOnce(
    Promise.resolve({
      data: modelList,
    })
  )
  jest
    .spyOn(axios, 'get')
    .mockResolvedValueOnce(Promise.resolve({ data: sortOrderData }))
}

const makeModelList = ({
  firstModelIsDefault,
  firstTypeName,
  secondModelIsDefault,
  secondTypeName,
  thirdModelIsDefault,
  thirdTypeName,
  forthModelIsDefault,
  forthTypeName,
  fifthModelIsDefault,
  fifthTypeName,
}: {
  firstModelIsDefault: boolean
  firstTypeName?: string
  secondModelIsDefault: boolean
  secondTypeName?: string
  thirdModelIsDefault: boolean
  thirdTypeName?: string
  forthModelIsDefault: boolean
  forthTypeName?: string
  fifthModelIsDefault: boolean
  fifthTypeName?: string
}) => ({
  floorId: 'some floorId',
  modules3D: [
    {
      id: 'id-1',
      visualId: '00000000-0000-0000-0000-000000000000',
      url: 'url-1',
      sortOrder: 1,
      canBeDeleted: true,
      isDefault: firstModelIsDefault,
      moduleTypeId: 'id-1',
      typeName: firstTypeName,
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
      isDefault: secondModelIsDefault,
      moduleTypeId: 'id-2',
      typeName: secondTypeName,
      moduleGroup: {
        id: '1caacbfe-3180-4d12-9e44-bfc483628803',
        name: 'Base',
        sortOrder: 0,
        siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
      },
    },
    {
      id: 'id-3',
      visualId: '00000000-0000-0000-0000-000000000000',
      sortOrder: 1,
      url: 'url-3',
      canBeDeleted: true,
      isDefault: thirdModelIsDefault,
      typeName: thirdTypeName,
      moduleTypeId: 'id-3',
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
      isDefault: forthModelIsDefault,
      typeName: forthTypeName,
      moduleTypeId: 'id-4',
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
      isDefault: fifthModelIsDefault,
      typeName: fifthTypeName,
      moduleTypeId: 'id-5',
      moduleGroup: {
        id: '1caacbfe-3180-4d12-9e44-bfc483628803',
        name: 'Base',
        sortOrder: 0,
        siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
      },
    },
  ],
})

const expectModelToBeIsDefault = (
  models: LayerGroupModule[],
  id: string,
  expectedToBeTrue: boolean
) =>
  expect(models.find((model) => model.id === id)?.isDefault).toBe(
    expectedToBeTrue
  )
