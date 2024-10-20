import { renderHook, waitFor, act } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { SourceType } from '@willow/common/insights/insights/types'
import useGetInsights from './useGetInsights'
import * as insightsService from '../../services/Insight/InsightsService'

const siteId = '404bd33c-a697-4027-b6a6-677e30a53d07'
const params = {
  filterSpecifications: [
    {
      field: 'siteId',
      operator: insightsService.FilterOperator.equalsLiteral,
      value: siteId,
    },
  ],
}
describe('useGetInsight', () => {
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(insightsService, 'fetchInsights')
      .mockRejectedValue(new Error('fetch error'))
    const { result } = renderHook(() => useGetInsights(params), {
      wrapper: BaseWrapper,
    })

    await act(async () => {
      waitFor(() => {
        expect(result.current.error).toBeDefined()
        expect(result.current.data).not.toBeDefined()
      })
    })
  })

  test('should provide data when hook triggered', async () => {
    const responseData: insightsService.InsightsResponse = [
      {
        id: 'f4166f23-da12-4592-a5ea-01dc255fb471',
        siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        sequenceNumber: '60MP-I-36',
        floorCode: 'L5',
        equipmentId: '00600000-0000-0000-0000-000000740353',
        type: 'note',
        name: 'Van Inspection a',
        priority: 1,
        status: 'open',
        lastStatus: 'open',
        state: 'active',
        sourceType: SourceType.inspection,
        occurredDate: '2022-01-11T07:19:13.152Z',
        updatedDate: '2022-01-11T07:19:13.657Z',
        externalId: '',
        occurrenceCount: 1,
        sourceName: 'Inspection',
        previouslyIgnored: 0,
        previouslyResolved: 0,
      },
    ]
    jest.spyOn(insightsService, 'fetchInsights').mockResolvedValue(responseData)
    const { result } = renderHook(() => useGetInsights(params), {
      wrapper: BaseWrapper,
    })

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
