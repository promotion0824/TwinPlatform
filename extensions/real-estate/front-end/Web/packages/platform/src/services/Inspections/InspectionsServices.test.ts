import axios from 'axios'
import {
  getInspections,
  InspectionsResponse,
  CheckRecordStatus,
} from './InspectionsServices'

const ERROR_MESSAGE = 'fetch error'

const siteId = '404bd33c-a697-4027-b6a6-677e30a53d07'
describe('Inspections service', () => {
  test('should return expected data', async () => {
    const responseData: InspectionsResponse = [
      {
        id: 'e8b95c57-3c77-405a-8296-175841329fa5',
        name: 'House Distribution Board E-DBH-05-01',
        siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        zoneId: '7666449e-6389-4541-9a24-8499c3fb78f3',
        floorCode: 'L5',
        assetId: '00600000-0000-0000-0000-000000740353',
        assignedWorkgroupId: 'efe565cb-a56f-4af5-876a-56e61f071e42',
        frequency: 8,
        unit: 'hours',
        startDate: '2022-01-14T10:15:00',
        sortOrder: 0,
        checks: [
          {
            id: '0eb57402-fea9-4e6f-93ad-9f4a89781cd3',
            inspectionId: 'e8b95c57-3c77-405a-8296-175841329fa5',
            name: 'a',
            type: 'Total',
            typeValue: '1',
            decimalPlaces: 1,
            isArchived: false,
            isPaused: false,
            canGenerateInsight: true,
            lastSubmittedRecord: {
              id: '06f50ff4-470c-4106-b27a-96ed9966e18f',
              inspectionId: 'e8b95c57-3c77-405a-8296-175841329fa5',
              checkId: '0eb57402-fea9-4e6f-93ad-9f4a89781cd3',
              inspectionRecordId: '3a8bf4a5-b7c9-49b8-b4af-9cacd6cab53c',
              status: CheckRecordStatus.Completed,
              submittedUserId: '7d00e35c-1f58-4520-9125-89839a6e41da',
              submittedDate: '2022-01-14T01:07:44.530Z',
              submittedSiteLocalDate: '2022-01-14T12:07:44.530Z',
              numberValue: 5,
              effectiveDate: '2022-01-14T01:00:00.000Z',
              notes: 'asdasdasdasdasd',
            },
            statistics: {
              checkRecordCount: 0,
              lastCheckSubmittedEntry: '',
              lastCheckSubmittedDate: '2022-01-14T01:07:44.530Z',
              workableCheckStatus: CheckRecordStatus.Overdue,
              nextCheckRecordDueTime: '2022-08-25T16:00:00.000Z',
            },
          },
          {
            id: '4f38fe06-a7a2-42ee-9b80-c4566a9e3667',
            inspectionId: 'e8b95c57-3c77-405a-8296-175841329fa5',
            name: 'v',
            type: 'Total',
            typeValue: '1',
            decimalPlaces: 1,
            isArchived: false,
            isPaused: false,
            canGenerateInsight: true,
            statistics: {
              checkRecordCount: 0,
              lastCheckSubmittedEntry: '',
              workableCheckStatus: CheckRecordStatus.Overdue,
              nextCheckRecordDueTime: '2022-08-25T16:00:00.000Z',
            },
          },
        ],
        checkRecordCount: 0,
        workableCheckCount: 2,
        completedCheckCount: 0,
        nextCheckRecordDueTime: '2022-08-25T16:00:00.000Z',
        assignedWorkgroupName: 'Test Group',
        zoneName: '20220114',
        assetName: 'DBH-L05-01',
        checkRecordSummaryStatus: CheckRecordStatus.Overdue,
      },
    ]
    jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

    const response = await getInspections(siteId)

    expect(response).toMatchObject(responseData)
  })
  test('should return error when exception error happens', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(getInspections(siteId)).rejects.toThrowError(ERROR_MESSAGE)
  })
})
