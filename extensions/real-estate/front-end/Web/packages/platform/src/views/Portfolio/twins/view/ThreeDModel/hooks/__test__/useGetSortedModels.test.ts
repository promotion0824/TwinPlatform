import { renderHook, waitFor } from '@testing-library/react'
import { ReactQueryStubProvider } from '@willow/common'
import {
  LayerGroupList,
  SortOrder,
} from 'packages/platform/src/services/ThreeDimensionModule/types'
import useGetSortedModels from '../useGetSortedModels'
import * as modelsService from '../../../../../../../services/ThreeDimensionModule/ModelsService'

describe('useGetSortedModels', () => {
  test('should provide empty array when error happens', async () => {
    jest
      .spyOn(modelsService, 'getModelsAndOrders')
      .mockRejectedValue(new Error('fetch error'))

    const { result } = renderHook(
      () =>
        useGetSortedModels({
          siteId: 'siteId-12',
          floorId: 'floorId-12',
        }),
      {
        wrapper: ReactQueryStubProvider,
      }
    )

    expect(result.current.data).toEqual([])
    expect(result.current.error).toBeDefined()
  })

  test('should provide models list with each model ordered by sortOrderData', async () => {
    jest.spyOn(modelsService, 'getModelsAndOrders').mockResolvedValue({
      initialModels: unsortedModelList,
      orders: sortOrderData,
    })

    const { result } = renderHook(
      () =>
        useGetSortedModels({
          siteId: 'siteId-21',
          floorId: 'floorId-21',
        }),
      {
        wrapper: ReactQueryStubProvider,
      }
    )

    await waitFor(() => {
      expect(result.current.data.map((model) => model.id)).toEqual([
        'id-3',
        'id-1',
        'id-2',
        'id-5',
        'id-4',
      ])
    })
  })
})

const sortOrderData: SortOrder = {
  sortOrder2d: [],
  sortOrder3d: ['id-3', 'id-1', 'id-2', 'id-5', 'id-4'],
}

const makeUnsortedModelList = ({
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

const unsortedModelList = makeUnsortedModelList({
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
}) as LayerGroupList
