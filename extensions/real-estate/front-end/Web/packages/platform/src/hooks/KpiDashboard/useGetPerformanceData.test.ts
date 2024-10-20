import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetPerformanceData from './useGetPerformanceData'
import * as performanceDataService from '../../services/KpiDashboard/PerformanceDataService'

describe('useGetPerformanceData', () => {
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(performanceDataService, 'getPerformanceData')
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(
      () =>
        useGetPerformanceData({
          startDate: '2000',
          endDate: '3000',
          portfolioId: 'portfolioId-3',
          customerId: 'customer-02',
          selectedDayRange: 'allDays',
          selectedBusinessHourRange: 'allHours',
          siteIds: ['site-99'],
          url: '/api/kpi/building_data',
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
    const responseData: performanceDataService.PerformanceResponse = [
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

    jest
      .spyOn(performanceDataService, 'getPerformanceData')
      .mockResolvedValue(responseData)

    const { result } = renderHook(
      () =>
        useGetPerformanceData({
          startDate: '1991',
          endDate: '2991',
          portfolioId: 'portfolioId-12',
          customerId: 'customerId-32',
          selectedDayRange: 'allDays',
          selectedBusinessHourRange: 'allHours',
          siteIds: ['site-1', 'site-10'],
          url: '/api/kpi/building_data',
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
