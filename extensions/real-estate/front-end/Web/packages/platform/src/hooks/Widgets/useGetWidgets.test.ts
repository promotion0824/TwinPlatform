import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetWidgets from './useGetWidgets'
import * as widgetsService from '../../services/Widgets/WidgetsService'

const siteId = '4e5fc229-ffd9-462a-882b-16b4a63b2a8a' // 1mw in uat
describe('useGetWidgets', () => {
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(widgetsService, 'getWidgets')
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(
      () => useGetWidgets({ baseUrl: '/api/sites', id: siteId }),
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
    const responseData: widgetsService.WidgetsResponse = {
      widgets: [
        {
          id: 'd0689c40-5e1e-464c-8aaa-6a0e887466e0',
          metadata: {
            embedPath:
              'https://app.sigmacomputing.com/embed/1-6Dx6NX8yFr5bHtGAe84eVj',
            name: 'Comfort Time In Compliance',
            allowExport: 'true',
          },
          type: 'sigmaReport',
        },
      ],
    }

    jest.spyOn(widgetsService, 'getWidgets').mockResolvedValue(responseData)

    const { result } = renderHook(
      () => useGetWidgets({ baseUrl: '/api/sites', id: siteId }),
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
