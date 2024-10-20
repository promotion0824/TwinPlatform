import axios from 'axios'
import {
  DatePickerDayRangeOptions,
  DatePickerBusinessRangeOptions,
} from '@willow/ui'
import { getEmbedUrls, EmbedUrlsResponse } from './EmbedUrlsService'

const responseData: EmbedUrlsResponse = [
  {
    name: 'report #1',
    url: 'url-1',
    reportId: 'report-1',
  },
  {
    name: 'report #2',
    url: 'url-2',
    reportId: 'report-2',
  },
]

describe('EmbedUrls Service', () => {
  test('should return expected data', async () => {
    jest
      .spyOn(axios, 'post')
      .mockResolvedValue(Promise.resolve({ data: responseData }))

    const response = await getEmbedUrls({
      start: '1001',
      end: '2001',
      customerId: 'customerId-321',
      siteIds: ['site-1', 'site-2'],
      selectedDayRange: DatePickerDayRangeOptions.ALL_DAYS_KEY,
      selectedBusinessHourRange: DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
      url: 'url-1',
    })
    expect(response).toMatchObject(responseData)
  })

  test('should return error when error occurs', async () => {
    jest.spyOn(axios, 'post').mockRejectedValue(new Error('fetch error'))
    await expect(
      getEmbedUrls({
        start: '1000',
        end: '2000',
        customerId: 'customerId-456',
        siteIds: ['site-1', 'site-3'],
        selectedDayRange: DatePickerDayRangeOptions.ALL_DAYS_KEY,
        selectedBusinessHourRange: DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
        url: 'url-2',
      })
    ).rejects.toThrowError('fetch error')
  })
})
