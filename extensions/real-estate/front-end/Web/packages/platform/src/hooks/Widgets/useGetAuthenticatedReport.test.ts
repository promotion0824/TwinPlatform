import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetAuthenticatedReport from './useGetAuthenticatedReport'
import * as authWidgtService from '../../services/Widgets/AuthWidgetService'

describe('useGetAuthenticatedReport', () => {
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(authWidgtService, 'getAuthenticatedReport')
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(
      () => useGetAuthenticatedReport({ url: 'url_to_authenticate_report' }),
      {
        wrapper: BaseWrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provide data when hook triggered', async () => {
    const responseData: authWidgtService.AuthenticatedReport = {
      token: 'myToken',
      url: 'authenticate_url',
      expiration: '2022-01-03T03:41:03.000Z',
    }

    jest
      .spyOn(authWidgtService, 'getAuthenticatedReport')
      .mockResolvedValue(responseData)

    const { result } = renderHook(
      () => useGetAuthenticatedReport({ url: 'url_to_authenticate_report' }),
      {
        wrapper: BaseWrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
