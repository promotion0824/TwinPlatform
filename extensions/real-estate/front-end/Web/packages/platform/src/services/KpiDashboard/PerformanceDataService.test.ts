import axios from 'axios'
import {
  getPerformanceData,
  PerformanceResponse,
  /* eslint-disable-next-line */
} from './PerformanceDataService'

const responseData: PerformanceResponse = [
  {
    name: 'OperationsScore_LastValue',
    values: [
      {
        xValue: '120 Collins Street',
      },
      {
        xValue: '420 George Street',
      },
    ],
    yuom: '%',
  },
]

describe('Performance Service', () => {
  test('should return expected data', async () => {
    jest
      .spyOn(axios, 'post')
      .mockResolvedValue(Promise.resolve({ data: responseData }))

    const response = await getPerformanceData({
      startDate: '1999',
      endDate: '2999',
      portfolioId: '123465',
      customerId: '123444',
      siteIds: ['xxx'],
      selectedDayRange: 'allDays',
      selectedBusinessHourRange: 'allHours',
      url: '/api/kpi/building_data',
    })
    expect(response).toMatchObject(responseData)
  })

  test('should return error when error occurs', async () => {
    jest.spyOn(axios, 'post').mockRejectedValue(new Error('fetch error'))
    await expect(
      getPerformanceData({
        startDate: '1000',
        endDate: '2000',
        portfolioId: 'portfolioId',
        customerId: 'someId',
        siteIds: ['someIds'],
        selectedDayRange: 'allDays',
        selectedBusinessHourRange: 'allHours',
        url: 'someUrl',
      })
    ).rejects.toThrowError('fetch error')
  })
})
