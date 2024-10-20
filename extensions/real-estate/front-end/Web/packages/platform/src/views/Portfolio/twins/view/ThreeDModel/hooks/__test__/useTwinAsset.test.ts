/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { renderHook, waitFor } from '@testing-library/react'
import { ReactQueryStubProvider } from '@willow/common'
import {
  LayerGroupList,
  SortOrder,
} from '../../../../../../../services/ThreeDimensionModule/types'
import useTwinAsset from '../useTwinAsset'
import * as modelsService from '../../../../../../../services/ThreeDimensionModule/ModelsService'
import makeModelList from './utils'

describe('useTwinAsset', () => {
  test('should provide empty array/values when error happens', async () => {
    jest
      .spyOn(modelsService, 'getModelsAndOrders')
      .mockRejectedValue(new Error('fetch error'))

    const { result } = renderHook(
      () => useTwinAsset({ siteId: 'siteId-1', floorId: 'floorId-1' }),
      {
        wrapper: ReactQueryStubProvider,
      }
    )

    expect(result.current.error).toBeDefined()
    expect(result.current.data.assetModels).toEqual([])
    expect(result.current.data.assetModelIds).toEqual('')
    expect(result.current.data.defaultAssetModule).not.toBeDefined()
    expect(result.current.data.is3dTabForAssetEnabled).toBeFalse()
    expect(result.current.data.assetDefaultUrns).toEqual([])
  })

  test('only when model isDefault is true or moduleTypeNamePath contains model.typeName can the model be included in assetModels', async () => {
    const modelList = makeModelList({
      firstModelIsDefault: true,
      secondModelIsDefault: true,
      thirdModelIsDefault: true,
      forthModelIsDefault: false,
      fifthModelIsDefault: false,
      fifthTypeName: 'Electrical',
    }) as LayerGroupList
    jest.spyOn(modelsService, 'getModelsAndOrders').mockResolvedValue({
      initialModels: modelList,
      orders: sortOrderData,
    })

    const { result } = renderHook(
      () =>
        useTwinAsset({
          siteId: 'siteId-2',
          floorId: 'floorId-2',
          moduleTypeNamePath: 'AC,Electrical,Light,Mech',
        }),
      {
        wrapper: ReactQueryStubProvider,
      }
    )

    const initialData = result.current.data
    await waitFor(() => expect(result.current.data).not.toBe(initialData))

    expect(result.current.error).toBeNull()
    expect(result.current.data.assetModels).toEqual([
      {
        id: 'id-1',
        visualId: '00000000-0000-0000-0000-000000000000',
        url: 'url-1',
        sortOrder: 1,
        canBeDeleted: true,
        isDefault: true,
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
        isDefault: true,
        moduleTypeId: 'id-2',
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
        isDefault: true,
        moduleTypeId: 'id-3',
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
    ])
    expect(result.current.data.assetModelIds).toBe('id-1,id-2,id-3,id-5')
    expect(result.current.data.defaultAssetModule?.typeName).toBe('Electrical')
    expect(result.current.data.is3dTabForAssetEnabled).toBeTrue()
    expect(result.current.data.assetDefaultUrns).toEqual([
      'url-1',
      'url-2',
      'url-3',
      'url-5',
    ])
  })

  test('when assetModels is an empty array because no model is included, is3dTabForAssetEnabled should be false', async () => {
    const anotherModelList = makeModelList({
      firstModelIsDefault: false,
      secondModelIsDefault: false,
      thirdModelIsDefault: false,
      forthModelIsDefault: false,
      fifthModelIsDefault: false,
    }) as LayerGroupList

    jest.spyOn(modelsService, 'getModelsAndOrders').mockResolvedValue({
      initialModels: anotherModelList,
      orders: sortOrderData,
    })

    const { result } = renderHook(
      () =>
        useTwinAsset({
          siteId: 'siteId-3',
          floorId: 'floorId-3',
        }),
      {
        wrapper: ReactQueryStubProvider,
      }
    )

    expect(result.current.error).toBeNull()
    expect(result.current.data.assetModels).toEqual([])
    expect(result.current.data.assetModelIds).toEqual('')
    expect(result.current.data.defaultAssetModule).not.toBeDefined()
    expect(result.current.data.is3dTabForAssetEnabled).toBeFalse()
    expect(result.current.data.assetDefaultUrns).toEqual([])
  })

  test('when multiple model.typeName matchs moduleTypeNamePath, defaultAsset will be the first model matching moduleTypeNamePath', async () => {
    const anotherModelList = makeModelList({
      firstModelIsDefault: true,
      firstTypeName: 'Mechanical',
      secondModelIsDefault: false,
      thirdModelIsDefault: false,
      forthModelIsDefault: true,
      forthTypeName: 'Light',
      fifthModelIsDefault: false,
    }) as LayerGroupList

    jest.spyOn(modelsService, 'getModelsAndOrders').mockResolvedValue({
      initialModels: anotherModelList,
      orders: sortOrderData,
    })

    const { result } = renderHook(
      () =>
        useTwinAsset({
          siteId: 'siteId-4',
          floorId: 'floorId-4',
          moduleTypeNamePath: 'AC,Electrical,Light,Mechanical',
        }),
      {
        wrapper: ReactQueryStubProvider,
      }
    )

    const initialData = result.current.data
    await waitFor(() => expect(result.current.data).not.toBe(initialData))

    expect(result.current.error).toBeNull()
    expect(result.current.data.assetModels).toEqual([
      {
        id: 'id-1',
        visualId: '00000000-0000-0000-0000-000000000000',
        url: 'url-1',
        sortOrder: 1,
        canBeDeleted: true,
        isDefault: true,
        moduleTypeId: 'id-1',
        typeName: 'Mechanical',
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
        isDefault: true,
        typeName: 'Light',
        moduleTypeId: 'id-4',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
    ])
    expect(result.current.data.assetModelIds).toBe('id-1,id-4')
    expect(result.current.data.defaultAssetModule?.typeName).toBe('Mechanical')
    expect(result.current.data.is3dTabForAssetEnabled).toBeTrue()
    expect(result.current.data.assetDefaultUrns).toEqual(['url-1', 'url-4'])
  })
})

const sortOrderData: SortOrder = {
  sortOrder2d: [],
  sortOrder3d: ['id-1', 'id-2', 'id-3', 'id-4', 'id-5'],
}
