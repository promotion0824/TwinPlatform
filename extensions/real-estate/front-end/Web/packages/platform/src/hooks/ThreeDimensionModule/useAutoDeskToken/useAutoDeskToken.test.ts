import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import * as autoDeskService from '../../../services/AutoDesk/AutoDeskService'
import useAutoDeskToken from './useAutoDeskToken'

describe('useAutoDeskToken', () => {
  const SERVICE_MODULE_NAME = 'getAutoDeskAccessToken'

  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(autoDeskService, SERVICE_MODULE_NAME)
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(() => useAutoDeskToken(), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provide data when hook triggered successfuly', async () => {
    jest.spyOn(autoDeskService, SERVICE_MODULE_NAME).mockResolvedValue({
      access_token: '123',
      token_type: 'token',
    })
    const { result } = renderHook(() => useAutoDeskToken(), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
