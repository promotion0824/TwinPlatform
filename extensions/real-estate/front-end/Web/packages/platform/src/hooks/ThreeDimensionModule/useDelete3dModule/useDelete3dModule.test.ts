import { renderHook, act, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useDelete3dModule from './useDelete3dModule'
import * as threeDimensionModule from '../../../services/ThreeDimensionModule/ThreeDimensionModuleService'

describe('useDelete3dModule', () => {
  const SERVICE_MODULE_NAME = 'delete3dModule'
  const triggerUploadingFile = (result) => {
    result.current.mutate()
  }

  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(threeDimensionModule, SERVICE_MODULE_NAME)
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(() => useDelete3dModule(), {
      wrapper: BaseWrapper,
    })

    act(() => {
      triggerUploadingFile(result)
    })

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provide data when delete triggered', async () => {
    jest.spyOn(threeDimensionModule, SERVICE_MODULE_NAME).mockResolvedValue({})
    const { result } = renderHook(() => useDelete3dModule(), {
      wrapper: BaseWrapper,
    })

    act(() => {
      triggerUploadingFile(result)
    })

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
