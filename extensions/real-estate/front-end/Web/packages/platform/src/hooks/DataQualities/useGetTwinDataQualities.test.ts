import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetTwinDataQualities from './useGetTwinDataQualities'
import * as dataQualitiesService from '../../services/DataQualities/DataQualities'

const siteId = '123'
const twinId = '456'
describe('useGetTwinDataQualities', () => {
  test('should provide error when expection error happens', async () => {
    jest
      .spyOn(dataQualitiesService, 'getTwinDataQualities')
      .mockRejectedValue(new Error('fetch error'))

    const { result } = renderHook(
      () => useGetTwinDataQualities({ siteId, twinId }),
      { wrapper: BaseWrapper }
    )

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provided data when hook is triggered', async () => {
    const responseData: dataQualitiesService.TwinDataQualitiesResponse = {
      attributePropertiesScore: 50,
      sensorsDefinedScore: 20,
      staticScore: 30,
      sensorsReadingDataScore: 40,
      connectivityScore: 40,
      overallScore: 30,
    }
    jest
      .spyOn(dataQualitiesService, 'getTwinDataQualities')
      .mockResolvedValue(responseData)

    const { result } = renderHook(
      () => useGetTwinDataQualities({ siteId, twinId }),
      { wrapper: BaseWrapper }
    )

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
