import { renderHook, waitFor } from '@testing-library/react'
import {
  DatePickerDayRangeOptions,
  DatePickerBusinessRangeOptions,
} from '@willow/ui'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetEmbedUrls from './useGetEmbedUrls'
import * as embedUrlsService from '../../services/KpiDashboard/EmbedUrlsService'

describe('useGetEmbedUrls', () => {
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(embedUrlsService, 'getEmbedUrls')
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(
      () =>
        useGetEmbedUrls({
          start: '1999',
          end: '2999',
          customerId: 'customerId-4',
          siteIds: ['site-5', 'site-6'],
          selectedDayRange: DatePickerDayRangeOptions.ALL_DAYS_KEY,
          selectedBusinessHourRange:
            DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
          url: '/sigma/portfolio/portfolioId-3/embedurls',
        }),
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
    const responseData: embedUrlsService.EmbedUrlsResponse = [
      {
        name: 'report #3',
        url: 'url-3',
        reportId: 'report-3',
      },
      {
        name: 'report #4',
        url: 'url-4',
        reportId: 'report-4',
      },
    ]

    jest.spyOn(embedUrlsService, 'getEmbedUrls').mockResolvedValue(responseData)

    const { result } = renderHook(
      () =>
        useGetEmbedUrls({
          start: '1900',
          end: '2999',
          customerId: 'customerId-4',
          siteIds: ['site-7', 'site-8'],
          selectedDayRange: DatePickerDayRangeOptions.ALL_DAYS_KEY,
          selectedBusinessHourRange:
            DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
          url: '/sigma/portfolio/portfolioId-4/embedurls',
        }),
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
