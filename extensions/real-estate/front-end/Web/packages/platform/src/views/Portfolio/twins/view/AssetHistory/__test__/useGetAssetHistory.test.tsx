import { renderHook, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { Status, TicketStatus } from '@willow/common/ticketStatus'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetAssetHistory, {
  UseGetAssetHistoryParams,
} from '../hooks/useGetAssetHistory'
import { PropsWithChildren } from 'react'
import { TicketStatusesStubProvider } from '@willow/common'

const siteId = '404bd33c-a697-4027-b6a6-677e30a53d07'
const assetId = '34ee1954-bfb6-4bb1-96e8-809d98a24d2c'
const twinId = 'AHU-L17-01'

const standardTickets = [
  {
    id: '646428df-61a6-49f9-a80b-02ef2cd05906',
    siteId,
    floorCode: 'L10',
    sequenceNumber: '60MP-T-1844',
    priority: 1,
    statusCode: 20,
    issueType: 'asset',
    issueId: assetId,
    issueName: 'DOR-L10-01',
    insightName: '',
    summary: 'Automation Test With Image',
    description: 'Description Test',
    reporterName: 'Automation Request Tester',
    assignedTo: 'Unassigned',
    createdDate: '2022-08-23T23:10:17.495Z',
    updatedDate: '2022-08-23T23:10:17.495Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 0,
    assigneeType: 'noAssignee',
  },
  {
    id: '594cc345-8206-4984-ad7d-127c1ba00b85',
    siteId,
    floorCode: 'L10',
    sequenceNumber: '60MP-T-1835',
    priority: 1,
    statusCode: 20,
    issueType: 'asset',
    issueId: assetId,
    issueName: 'DOR-L10-01',
    insightName: '',
    summary: 'Automation Test With Image',
    description: 'Description Test',
    reporterName: 'Automation Request Tester',
    assignedTo: 'Unassigned',
    createdDate: '2022-08-24T22:55:33.352Z',
    updatedDate: '2022-08-24T22:55:33.352Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 0,
    assigneeType: 'noAssignee',
  },
]

const scheduledTickets = [
  {
    id: '3c56141f-ebc6-4453-a47b-2bda261f6229',
    siteId,
    floorCode: '',
    sequenceNumber: '60MP-S-2321',
    priority: 1,
    statusCode: 20,
    issueType: 'asset',
    issueId: assetId,
    issueName: 'VAV-CN-L02-02',
    insightName: '',
    summary: 'Aug 17 Regression Tom - 5',
    description: 'Aug 17 Regression Tom - 5',
    reporterName: 'Portfolio Admin',
    assignedTo: 'Unassigned',
    dueDate: '2022-09-18T17:00:07.595Z',
    createdDate: '2022-08-17T07:00:15.248Z',
    updatedDate: '2022-08-17T07:00:15.248Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    scheduledDate: '2022-08-18T17:00:07.595Z',
    tasks: [
      {
        id: '101d5677-780d-4c37-bbde-796999cbef78',
        taskName: 'Test',
        type: 'Numeric',
        isCompleted: false,
        decimalPlaces: 2,
        minValue: 2,
        maxValue: 222,
        unit: 'test',
      },
    ],
    groupTotal: 0,
    assigneeType: 'noAssignee',
  },
  {
    id: '2523e981-e449-4519-9f13-36b7f29c1489',
    siteId,
    floorCode: '',
    sequenceNumber: '60MP-S-1344',
    priority: 1,
    statusCode: 20,
    issueType: 'asset',
    issueId: assetId,
    issueName: 'VAV-CN-L02-02',
    insightName: '',
    summary: 'TODAY May 18',
    description: 'TODAY May 18 EDIT',
    reporterName: 'Automation Request Tester',
    assignedTo: 'Unassigned',
    dueDate: '2022-06-17T00:00:00.000Z',
    createdDate: '2022-06-06T00:50:17.778Z',
    updatedDate: '2022-06-06T00:50:17.778Z',
    categoryId: 'ad3f15f3-cd55-4a8e-9846-ecb3c2a4369f',
    category: 'Elevator',
    sourceName: 'Platform',
    externalId: '',
    scheduledDate: '2022-05-17T00:00:00.000Z',
    tasks: [
      {
        id: '969bcb4d-cd33-4a44-a5f0-64effcc5fccf',
        taskName: 'Test1',
        type: 'Checkbox',
        isCompleted: false,
      },
      {
        id: '1c3a691e-7978-4581-9b48-76da08be66d9',
        taskName: 'Test 2',
        type: 'Checkbox',
        isCompleted: false,
      },
    ],
    groupTotal: 0,
    assigneeType: 'noAssignee',
  },
]

const inspections = [
  {
    id: 'e8b95c57-3c77-405a-8296-175841329fa5',
    name: 'House Distribution Board E-DBH-05-01',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    zoneId: '7666449e-6389-4541-9a24-8499c3fb78f3',
    floorCode: 'L5',
    assetId,
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
          status: 'completed',
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
          workableCheckStatus: 'overdue',
          nextCheckRecordDueTime: '2022-09-02T16:00:00.000Z',
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
          workableCheckStatus: 'overdue',
          nextCheckRecordDueTime: '2022-09-02T16:00:00.000Z',
        },
      },
    ],
    checkRecordCount: 0,
    workableCheckCount: 2,
    completedCheckCount: 0,
    nextCheckRecordDueTime: '2022-09-02T16:00:00.000Z',
    assignedWorkgroupName: 'Test Group',
    zoneName: '20220114',
    assetName: 'DBH-L05-01',
    checkRecordSummaryStatus: 'overdue',
  },
]

const insights = [
  {
    id: 'fbd169a2-8afc-4c3b-a668-5fa618c355a1',
    siteId,
    sequenceNumber: '60MP-I-60',
    floorCode: 'L1',
    equipmentId: '5a3e44bc-393a-4654-b94a-902ef5bb66ff',
    type: 'note',
    name: 'VAV-CN-L01-02 Aug 17 Test w',
    priority: 3,
    status: 'open',
    state: 'active',
    sourceType: 'inspection',
    occurredDate: '2022-08-17T07:50:30.204Z',
    updatedDate: '2022-08-17T07:50:30.337Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'Inspection',
  },
  {
    id: '8797264b-abf0-4424-9543-d249af01d69f',
    siteId,
    sequenceNumber: '60MP-I-61',
    floorCode: 'L1',
    equipmentId: '5a3e44bc-393a-4654-b94a-902ef5bb66ff',
    type: 'note',
    name: 'VAV-CN-L01-02 Aug 17 Test w',
    priority: 3,
    status: 'open',
    state: 'active',
    sourceType: 'inspection',
    occurredDate: '2022-08-17T07:51:11.493Z',
    updatedDate: '2022-08-17T07:51:11.513Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'Inspection',
  },
]

const Wrapper = ({ children }: PropsWithChildren<any>) => (
  <BaseWrapper>
    <TicketStatusesStubProvider
      data={[
        {
          status: Status.open,
          statusCode: 20,
        } as TicketStatus,
      ]}
    >
      {children}
    </TicketStatusesStubProvider>
  </BaseWrapper>
)

const handlers = [
  rest.get(
    `/api/sites/${siteId}/assets/${assetId}/tickets/history`,
    (req, res, ctx) => {
      if (req.url.searchParams.get('scheduled')) {
        return res(ctx.json(scheduledTickets))
      }
      return res(ctx.json(standardTickets))
    }
  ),
  rest.get(
    `/api/sites/${siteId}/assets/${assetId}/insights/history`,
    (_req, res, ctx) => res(ctx.json(insights))
  ),

  rest.post(`/api/insights`, (_req, res, ctx) =>
    res(
      ctx.json({
        items: insights,
      })
    )
  ),

  rest.get(`/api/sites/${siteId}/inspections`, (_req, res, ctx) =>
    res(ctx.json(inspections))
  ),
]
const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('useGetAssetHistory', () => {
  jest.setTimeout(15000) // prevent test failure by timeout

  test('Return correct values', async () => {
    const { result } = renderHook(
      (props: UseGetAssetHistoryParams) =>
        useGetAssetHistory({
          siteId: props.siteId,
          assetId: props.assetId,
          twinId: props.twinId,
          options: props.options,
          isInsightsDisabled: false,
          isInspectionEnabled: true,
          isScheduledTicketsEnabled: true,
          isTicketingDisabled: false,
        }),
      {
        wrapper: Wrapper,
        initialProps: {
          siteId,
          assetId,
          twinId,
          options: { enabled: true },
        },
      }
    )
    expect(result.current.assetHistory).toEqual([])

    await waitFor(() => {
      expect(result.current.assetHistory.length).toBeGreaterThan(0)
    })

    const { assetHistory } = result.current

    expect(assetHistory).toEqual(expectedAssetHistory)

    for (const type of Object.keys(assetHistoryDataMap)) {
      const numberOfAssetHistoryCount = assetHistory.filter(
        ({ assetHistoryType }) => assetHistoryType === type
      ).length

      expect(numberOfAssetHistoryCount).toBe(assetHistoryDataMap[type].length)
    }
  })

  const consistentInitialProps = {
    siteId,
    assetId,
    twinId,
    options: { enabled: true },
  }

  const featureStatues = {
    isInsightsDisabled: false,
    isInspectionEnabled: true,
    isScheduledTicketsEnabled: true,
    isTicketingDisabled: false,
  }

  const assetHistoryDataMap = {
    insight: insights,
    inspection: inspections,
    standardTicket: standardTickets,
    scheduledTicket: scheduledTickets,
  }

  test.each([
    {
      initialProps: {
        ...consistentInitialProps,
        ...featureStatues,
        isInsightsDisabled: true,
      },
      excludedTypes: ['insight'],
      excludedTypeText: 'insights',
    },
    {
      initialProps: {
        ...consistentInitialProps,
        ...featureStatues,
        isInspectionEnabled: false,
      },
      excludedTypes: ['inspection'],
      excludedTypeText: 'inspections',
    },
    {
      initialProps: {
        ...consistentInitialProps,
        ...featureStatues,
        isScheduledTicketsEnabled: false,
      },
      excludedTypes: ['scheduledTicket'],
      excludedTypeText: 'scheduled tickets',
    },
    {
      initialProps: {
        ...consistentInitialProps,
        ...featureStatues,
        isTicketingDisabled: true,
      },
      excludedTypes: ['standardTicket', 'scheduledTicket'],
      excludedTypeText: 'tickets and scheduled tickets',
    },
  ])(
    `Return correct values when feature of "$excludedTypeText" are excluded`,
    async ({ initialProps, excludedTypes }) => {
      const { result } = renderHook(
        (props: UseGetAssetHistoryParams) => useGetAssetHistory(props),
        {
          wrapper: Wrapper,
          initialProps,
        }
      )
      expect(result.current.assetHistory).toEqual([])

      await waitFor(() => {
        expect(result.current.assetHistory.length).toBeGreaterThan(0)
      })

      const { assetHistory } = result.current

      for (const type of Object.keys(assetHistoryDataMap)) {
        if (!excludedTypes.includes(type)) {
          const numberOfAssetHistoryCount = assetHistory.filter(
            ({ assetHistoryType }) => assetHistoryType === type
          ).length

          expect(numberOfAssetHistoryCount).toBe(
            assetHistoryDataMap[type].length
          )
        }

        const numberOfExcludedTypes = assetHistory.filter(
          ({ assetHistoryType }) => excludedTypes.includes(assetHistoryType)
        ).length
        expect(numberOfExcludedTypes).toBe(0)
      }
    }
  )
})

const expectedAssetHistory = [
  {
    id: 'fbd169a2-8afc-4c3b-a668-5fa618c355a1',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-60',
    floorCode: 'L1',
    equipmentId: '5a3e44bc-393a-4654-b94a-902ef5bb66ff',
    type: 'note',
    name: 'VAV-CN-L01-02 Aug 17 Test w',
    priority: 3,
    status: 'open',
    state: 'active',
    sourceType: 'inspection',
    occurredDate: '2022-08-17T07:50:30.204Z',
    updatedDate: '2022-08-17T07:50:30.337Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'Inspection',
    assetHistoryType: 'insight',
    date: '2022-08-17T07:50:30.337Z',
    ID: '60MP-I-60',
  },
  {
    id: '8797264b-abf0-4424-9543-d249af01d69f',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-61',
    floorCode: 'L1',
    equipmentId: '5a3e44bc-393a-4654-b94a-902ef5bb66ff',
    type: 'note',
    name: 'VAV-CN-L01-02 Aug 17 Test w',
    priority: 3,
    status: 'open',
    state: 'active',
    sourceType: 'inspection',
    occurredDate: '2022-08-17T07:51:11.493Z',
    updatedDate: '2022-08-17T07:51:11.513Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'Inspection',
    assetHistoryType: 'insight',
    date: '2022-08-17T07:51:11.513Z',
    ID: '60MP-I-61',
  },
  {
    id: '646428df-61a6-49f9-a80b-02ef2cd05906',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: 'L10',
    sequenceNumber: '60MP-T-1844',
    priority: 1,
    statusCode: 20,
    status: Status.open,
    issueType: 'asset',
    issueId: '34ee1954-bfb6-4bb1-96e8-809d98a24d2c',
    issueName: 'DOR-L10-01',
    insightName: '',
    summary: 'Automation Test With Image',
    description: 'Description Test',
    reporterName: 'Automation Request Tester',
    assignedTo: 'Unassigned',
    createdDate: '2022-08-23T23:10:17.495Z',
    updatedDate: '2022-08-23T23:10:17.495Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 0,
    assigneeType: 'noAssignee',
    assetHistoryType: 'standardTicket',
    date: '2022-08-23T23:10:17.495Z',
    ID: '60MP-T-1844',
  },
  {
    id: '594cc345-8206-4984-ad7d-127c1ba00b85',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: 'L10',
    sequenceNumber: '60MP-T-1835',
    priority: 1,
    statusCode: 20,
    status: Status.open,
    issueType: 'asset',
    issueId: '34ee1954-bfb6-4bb1-96e8-809d98a24d2c',
    issueName: 'DOR-L10-01',
    insightName: '',
    summary: 'Automation Test With Image',
    description: 'Description Test',
    reporterName: 'Automation Request Tester',
    assignedTo: 'Unassigned',
    createdDate: '2022-08-24T22:55:33.352Z',
    updatedDate: '2022-08-24T22:55:33.352Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 0,
    assigneeType: 'noAssignee',
    assetHistoryType: 'standardTicket',
    date: '2022-08-24T22:55:33.352Z',
    ID: '60MP-T-1835',
  },
  {
    id: '3c56141f-ebc6-4453-a47b-2bda261f6229',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: '',
    sequenceNumber: '60MP-S-2321',
    priority: 1,
    statusCode: 20,
    status: Status.open,
    issueType: 'asset',
    issueId: '34ee1954-bfb6-4bb1-96e8-809d98a24d2c',
    issueName: 'VAV-CN-L02-02',
    insightName: '',
    summary: 'Aug 17 Regression Tom - 5',
    description: 'Aug 17 Regression Tom - 5',
    reporterName: 'Portfolio Admin',
    assignedTo: 'Unassigned',
    dueDate: '2022-09-18T17:00:07.595Z',
    createdDate: '2022-08-17T07:00:15.248Z',
    updatedDate: '2022-08-17T07:00:15.248Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    scheduledDate: '2022-08-18T17:00:07.595Z',
    tasks: [
      {
        id: '101d5677-780d-4c37-bbde-796999cbef78',
        taskName: 'Test',
        type: 'Numeric',
        isCompleted: false,
        decimalPlaces: 2,
        minValue: 2,
        maxValue: 222,
        unit: 'test',
      },
    ],
    groupTotal: 0,
    assigneeType: 'noAssignee',
    assetHistoryType: 'scheduledTicket',
    date: '2022-08-17T07:00:15.248Z',
    ID: '60MP-S-2321',
  },
  {
    id: '2523e981-e449-4519-9f13-36b7f29c1489',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: '',
    sequenceNumber: '60MP-S-1344',
    priority: 1,
    statusCode: 20,
    status: Status.open,
    issueType: 'asset',
    issueId: '34ee1954-bfb6-4bb1-96e8-809d98a24d2c',
    issueName: 'VAV-CN-L02-02',
    insightName: '',
    summary: 'TODAY May 18',
    description: 'TODAY May 18 EDIT',
    reporterName: 'Automation Request Tester',
    assignedTo: 'Unassigned',
    dueDate: '2022-06-17T00:00:00.000Z',
    createdDate: '2022-06-06T00:50:17.778Z',
    updatedDate: '2022-06-06T00:50:17.778Z',
    categoryId: 'ad3f15f3-cd55-4a8e-9846-ecb3c2a4369f',
    category: 'Elevator',
    sourceName: 'Platform',
    externalId: '',
    scheduledDate: '2022-05-17T00:00:00.000Z',
    tasks: [
      {
        id: '969bcb4d-cd33-4a44-a5f0-64effcc5fccf',
        taskName: 'Test1',
        type: 'Checkbox',
        isCompleted: false,
      },
      {
        id: '1c3a691e-7978-4581-9b48-76da08be66d9',
        taskName: 'Test 2',
        type: 'Checkbox',
        isCompleted: false,
      },
    ],
    groupTotal: 0,
    assigneeType: 'noAssignee',
    assetHistoryType: 'scheduledTicket',
    date: '2022-06-06T00:50:17.778Z',
    ID: '60MP-S-1344',
  },
  {
    id: 'e8b95c57-3c77-405a-8296-175841329fa5',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    name: 'House Distribution Board E-DBH-05-01',
    zoneId: '7666449e-6389-4541-9a24-8499c3fb78f3',
    floorCode: 'L5',
    assetId: '34ee1954-bfb6-4bb1-96e8-809d98a24d2c',
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
          status: 'completed',
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
          workableCheckStatus: 'overdue',
          nextCheckRecordDueTime: '2022-09-02T16:00:00.000Z',
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
          workableCheckStatus: 'overdue',
          nextCheckRecordDueTime: '2022-09-02T16:00:00.000Z',
        },
      },
    ],
    checkRecordCount: 0,
    workableCheckCount: 2,
    completedCheckCount: 0,
    nextCheckRecordDueTime: '2022-09-02T16:00:00.000Z',
    assignedWorkgroupName: 'Test Group',
    zoneName: '20220114',
    assetName: 'DBH-L05-01',
    checkRecordSummaryStatus: 'overdue',
    assetHistoryType: 'inspection',
    date: '2022-01-14T10:15:00',
    ID: 'House Distribution Board E-DBH-05-01',
    status: 'overdue',
  },
]
