import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetFloorsDataQualities from './useGetFloorsDataQualities'
import * as dataQualitiesService from '../../services/DataQualities/DataQualities'

const siteId = 'site-123'
describe('useGetFloorsDataQualities', () => {
  test('should provide error when expection error happens', async () => {
    jest
      .spyOn(dataQualitiesService, 'getFloorsDataQualities')
      .mockRejectedValue(new Error('fetch error'))

    const { result } = renderHook(() => useGetFloorsDataQualities({ siteId }), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provided data when hook is triggered', async () => {
    const responseData: dataQualitiesService.LocationDataQualitiesResponse = [
      {
        locationId: '123',
        dataQuality: {
          staticScore: 0.2,
          connectivityScore: 0.4,
          overallScore: 0.3,
        },
      },
    ]
    jest
      .spyOn(dataQualitiesService, 'getFloorsDataQualities')
      .mockResolvedValue(responseData)

    const { result } = renderHook(() => useGetFloorsDataQualities({ siteId }), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
