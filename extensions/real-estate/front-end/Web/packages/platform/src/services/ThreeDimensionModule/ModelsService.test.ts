import axios from 'axios'
import { getModels, getModelsAndOrders, getSortOrder } from './ModelsService'
import { LayerGroupList, SortOrder } from './types'

export const sortOrderData: SortOrder = {
  sortOrder2d: [],
  sortOrder3d: [
    'a68209ef-6123-4179-b1ad-9485d309ceea',
    '2d1f8e2d-1112-4c13-99d9-1b0476d91ab8',
  ],
}

export const modelsData: LayerGroupList = {
  floorId: '4f2bd55f-8fa9-4922-84d4-93d741e58ed3',
  floorName: 'BLDG',
  layerGroups: [
    {
      id: 'fa91624b-5bff-4a5f-8bd6-c358b6474cdf',
      name: 'Assets layer',
      is3D: false,
      zones: [],
      layers: [],
      equipments: [],
    },
  ],
  layerGroups2D: [],
  layerGroups3D: [],
  modules2D: [],
  modules3D: [
    {
      id: '4e987f5c-a8b4-412b-be74-1529efbfb4b3',
      name: 'ELE-BLDG-BB.nwd',
      visualId: '00000000-0000-0000-0000-000000000000',
      url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9FTEUtQkxERy1CQl8yMDIxMDkwMzAzMTYwNi5ud2Q=',
      sortOrder: 1,
      canBeDeleted: true,
      isDefault: false,
      typeName: 'Electrical',
      groupType: 'Base',
      moduleTypeId: '11bc1f16-251e-40a2-84b2-428d6cb846b9',
      moduleGroup: {
        id: '1caacbfe-3180-4d12-9e44-bfc483628803',
        name: 'Base',
        sortOrder: 0,
        siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
      },
    },
  ],
}

describe('Models Service', () => {
  test('should return expected data running getModels', async () => {
    jest
      .spyOn(axios, 'get')
      .mockResolvedValue(Promise.resolve({ data: modelsData }))

    const modelsResponse = await getModels('siteId-1', 'floorId-1')

    expect(modelsResponse).toMatchObject(modelsData)
  })

  test('should return expected data running getSortOrder', async () => {
    jest
      .spyOn(axios, 'get')
      .mockResolvedValue(Promise.resolve({ data: sortOrderData }))

    const sortOrderResponse = await getSortOrder('siteId-1')

    expect(sortOrderResponse).toMatchObject(sortOrderData)
  })

  test('should return error when error occurs running getModels', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))

    await expect(getModels('siteId-1', 'floorId-1')).rejects.toThrowError(
      'fetch error'
    )
  })

  test('should return error when error occurs running getSortOrder', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))

    await expect(getSortOrder('siteId-1')).rejects.toThrowError('fetch error')
  })
})

describe('Models Service with one function combining getModels and getSortOrder', () => {
  test('should return expected data running getModelsAndOrders', async () => {
    jest
      .spyOn(axios, 'get')
      .mockResolvedValueOnce(Promise.resolve({ data: modelsData }))
    jest
      .spyOn(axios, 'get')
      .mockResolvedValueOnce(Promise.resolve({ data: sortOrderData }))

    const response = await getModelsAndOrders('siteId-1', 'floorId-1')

    expect(response).toBeDefined()
    expect(response.initialModels).toBe(modelsData)
    expect(response.orders).toBe(sortOrderData)
  })

  test('should return error when error occurs running getSortOrder', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))

    await expect(
      getModelsAndOrders('siteId-2', 'floorId-2')
    ).rejects.toThrowError('fetch error')
  })
})
