import { renderHook, RenderHookResult, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGet3dModule, { useGet3dModuleFile } from './useGet3dModule'
import * as threeDimensionModule from '../../../services/ThreeDimensionModule/ThreeDimensionModuleService'

type RenderHookData = RenderHookResult<{ error: any; data: any }, unknown>

const provideExceptionError = async (mockServiceName, hookFn) => {
  jest
    .spyOn(threeDimensionModule, mockServiceName)
    .mockRejectedValue(new Error('fetch error'))
  const { result }: RenderHookData = renderHook(hookFn, {
    wrapper: BaseWrapper,
  })

  await waitFor(() => {
    expect(result.current.error).toBeDefined()
    expect(result.current.data).not.toBeDefined()
  })
}
const provideData = async (mockServiceName, hookFn) => {
  jest.spyOn(threeDimensionModule, mockServiceName).mockResolvedValue({})
  const { result }: RenderHookData = renderHook(hookFn, {
    wrapper: BaseWrapper,
  })

  await waitFor(() => {
    expect(result.current.data).toBeDefined()
    expect(result.current.error).toBeNull()
  })
}
describe('useGet3dModule', () => {
  const siteId = 'siteId1'
  const SERVICE_MODULE_NAME = 'get3dModule'

  test('should provide error when exception error happens', async () => {
    await provideExceptionError(SERVICE_MODULE_NAME, () =>
      useGet3dModule(siteId, { enabled: true })
    )
  })

  test('should provide data when upload triggered', async () => {
    await provideData(SERVICE_MODULE_NAME, () =>
      useGet3dModule(siteId, { enabled: true })
    )
  })
})

describe('useGet3dModulefile', () => {
  const SERVICE_MODULE_NAME = 'get3dModuleFile'

  test('should provide error when exception error happens', async () => {
    await provideExceptionError(SERVICE_MODULE_NAME, () =>
      useGet3dModuleFile({}, { enabled: true })
    )
  })

  test('should provide data when upload triggered', async () => {
    await provideData(SERVICE_MODULE_NAME, () =>
      useGet3dModuleFile({}, { enabled: true })
    )
  })
})
