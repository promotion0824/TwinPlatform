import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import * as autoDeskService from '../../../services/AutoDesk/AutoDeskService'
import useAutoDeskFileManifest from './useAutoDeskFileManifest'

describe('useAutoDeskFileManifest', () => {
  const SERVICE_MODULE_NAME = 'getAutoDeskFileManifest'
  const params = {
    urn: 'sdfsdaf',
    accessToken: '#@$%VGR$E#$',
    tokenType: 'Bearer',
  }
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(autoDeskService, SERVICE_MODULE_NAME)
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(() => useAutoDeskFileManifest(params), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provide data when hook triggered successfuly', async () => {
    jest
      .spyOn(autoDeskService, SERVICE_MODULE_NAME)
      .mockResolvedValue({ progress: 'complete', fileInfo: {} })
    const { result } = renderHook(() => useAutoDeskFileManifest(params), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
