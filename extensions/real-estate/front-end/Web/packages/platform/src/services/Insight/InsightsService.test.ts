import axios from 'axios'
import { SourceType } from '@willow/common/insights/insights/types'
import {
  fetchInsights,
  InsightsResponse,
  FilterOperator,
} from './InsightsService'

const ERROR_MESSAGE = 'fetch error'

const siteId = '404bd33c-a697-4027-b6a6-677e30a53d07'
const params = {
  filterSpecifications: [
    {
      field: 'siteId',
      operator: FilterOperator.equalsLiteral,
      value: siteId,
    },
  ],
}
describe('Insight service', () => {
  test('should return expected data', async () => {
    const responseData: InsightsResponse = [
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
    jest
      .spyOn(axios, 'post')
      .mockResolvedValue({ data: { items: responseData } })

    const response = await fetchInsights({
      specifications: params,
    })

    expect(response).toMatchObject(responseData)
  })
  test('should return error when exception error happens', async () => {
    jest.spyOn(axios, 'post').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(
      fetchInsights({
        specifications: params,
      })
    ).rejects.toThrowError(ERROR_MESSAGE)
  })
})
