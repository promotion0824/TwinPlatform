import { renderHook, act, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useUpload3dModule from './useUpload3dModule'
import * as threeDimensionModule from '../../../services/ThreeDimensionModule/ThreeDimensionModuleService'

describe('useUpload3dModule', () => {
  const SERVICE_MODULE_NAME = 'post3dModule'
  const expectedDataList = [{ id: 1, fileName: 'abc.nwd' }]
  const triggerUploadingFile = (result) => {
    result.current.mutate({ siteId: 1, file: undefined })
  }

  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(threeDimensionModule, SERVICE_MODULE_NAME)
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(() => useUpload3dModule({ enabled: true }), {
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

  const post3dModuleMockFn = async (_, __, { onUploadProgress }) => {
    const fileSize = 1024
    const progressRate = [0, 0.2, 0.4, 0.6, 0.8, 1]

    progressRate.forEach(async (rate) => {
      onUploadProgress({ loaded: fileSize * rate, total: fileSize })
    })

    return expectedDataList
  }
  const checkEachProgress = async (returnedProgress) => {
    const expectedProgresses = [0, 20, 40, 60, 80, 100]
    const index = expectedProgresses.findIndex(
      (progress) => progress === returnedProgress
    )
    return index === expectedProgresses.length - 1
  }
  test('should provide progress change when upload triggered', async () => {
    jest
      .spyOn(threeDimensionModule, SERVICE_MODULE_NAME)
      .mockImplementation(post3dModuleMockFn)
    const { result } = renderHook(() => useUpload3dModule({ enabled: true }), {
      wrapper: BaseWrapper,
    })

    act(() => {
      triggerUploadingFile(result)
    })

    await waitFor(() => checkEachProgress(result.current.progress))
  })

  test('should reset progress to 0 when the hook resets', async () => {
    jest
      .spyOn(threeDimensionModule, SERVICE_MODULE_NAME)
      .mockImplementation(post3dModuleMockFn)
    const { result } = renderHook(() => useUpload3dModule({ enabled: true }), {
      wrapper: BaseWrapper,
    })
    act(() => {
      triggerUploadingFile(result)
    })

    let initialResult = result.current
    await waitFor(() => {
      expect(result.current).not.toBe(initialResult)
    })

    act(() => {
      result.current.reset()
    })

    initialResult = result.current

    await waitFor(() => {
      expect(result.current).not.toBe(initialResult)
    })

    expect(result.current.progress).toBe(0)
  })
})
