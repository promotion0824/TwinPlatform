import { render, screen, waitFor } from '@testing-library/react'
import { TicketStatusesStubProvider } from '@willow/common'
import { Status, Tab } from '@willow/common/ticketStatus'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import AssetHistoryTable from '../Table/index'
import AssetHistoryProvider from '../provider/AssetHistoryProvider'

const assetHistory = [
  {
    id: 'd2a4fc3d-81f8-4347-b486-06790f3b9193',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: 'L36',
    sequenceNumber: '60MP-T-1830',
    priority: 3,
    status: Status.open,
    issueType: 'noIssue',
    issueName: '',
    insightName: '',
    summary: 'Cypress UI ticket: 22',
    description: 'Cypress ticket description',
    reporterName: 'Cypress user',
    assignedTo: 'Unassigned',
    createdDate: '2022-08-22T03:54:32.026Z',
    updatedDate: '2022-08-22T03:54:32.026Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 59,
    assigneeType: 'noAssignee',
    assetHistoryType: 'standardTicket',
    date: '2022-08-22T03:54:32.026Z',
    name: '60MP-T-1830',
    statusCode: 0,
  },
  {
    id: '387cb3ac-4458-4693-8833-00bafe515be5',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: '',
    sequenceNumber: '60MP-S-2338',
    priority: 4,
    status: Status.reassign,
    issueType: 'asset',
    issueId: '00600000-0000-0000-0000-000000788482',
    issueName: 'ACB-L01-002',
    insightName: '',
    summary: 'Current Date',
    description: 'CURRENT DATE SCENARIO',
    reporterName: 'Investa-AU PortfolioAdmin',
    assignedTo: 'Unassigned',
    dueDate: '2022-08-24T00:00:00.000Z',
    createdDate: '2022-08-18T00:22:49.676Z',
    updatedDate: '2022-08-18T00:22:49.676Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    scheduledDate: '2022-08-17T00:00:00.000Z',
    tasks: [],
    groupTotal: 400,
    assigneeType: 'noAssignee',
    assetHistoryType: 'scheduledTicket',
    date: '2022-08-18T00:22:49.676Z',
    name: '60MP-S-2338',
    statusCode: 30,
  },
  {
    id: '71ce4e2e-a0bb-4b66-b63d-005b9dc3b6ab',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: '',
    sequenceNumber: '60MP-S-2045',
    priority: 1,
    status: Status.inProgress,
    issueType: 'asset',
    issueId: '00600000-0000-0000-0000-000000842408',
    issueName: 'Cold Water Pump H-CWP-B3-01',
    insightName: '',
    summary: 'TODAY JAN13, MOBILE - 2',
    description: 'TODAY JAN13, MOBILE - 2',
    reporterName: 'New requestor SC',
    assignedTo: 'Test Group',
    dueDate: '2022-08-13T00:00:01.229Z',
    createdDate: '2022-07-11T14:00:02.606Z',
    updatedDate: '2022-07-11T14:00:02.606Z',
    categoryId: 'ad3f15f3-cd55-4a8e-9846-ecb3c2a4369f',
    category: 'Elevator',
    sourceName: 'Platform',
    externalId: '',
    scheduledDate: '2022-07-13T00:00:01.229Z',
    statusCode: 10,
    tasks: [
      {
        id: '7a86dc80-270b-44e6-80eb-b05d555d9064',
        taskName: 'test',
        type: 'Checkbox',
        isCompleted: false,
      },
    ],
    groupTotal: 400,
    assigneeType: 'workGroup',
    assigneeId: 'efe565cb-a56f-4af5-876a-56e61f071e42',
    assetHistoryType: 'scheduledTicket',
    date: '2022-07-11T14:00:02.606Z',
    name: '60MP-S-2340',
  },
  {
    id: 'f3536751-7ae1-4e5e-b29e-80e59adf96e8',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-24',
    floorCode: 'L10',
    equipmentId: '00600000-0000-0000-0000-000000787855',
    type: 'note',
    name: 'Chilled Beam M-ACB-PE-10-02 Test 2',
    priority: 2,
    status: 'acknowledged',
    lastStatus: 'ignored',
    state: 'active',
    sourceType: 'inspection',
    occurredDate: '2022-01-06T04:52:10.123Z',
    updatedDate: '2022-06-29T07:08:04.958Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'Inspection',
    assetHistoryType: 'insight',
    date: '2022-06-29T07:08:04.958Z',
  },
  {
    id: '77da2b2c-f62b-40c8-b83b-e25106d55ae5',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-16',
    floorCode: 'L1',
    equipmentId: '00600000-0000-0000-0000-000000788483',
    type: 'alert',
    name: 'Chilled Beam M-ACB-01-001 Dec 21 Smoke',
    priority: 2,
    status: 'closed',
    lastStatus: 'resolved',
    state: 'active',
    sourceType: 'inspection',
    occurredDate: '2021-12-22T00:45:57.001Z',
    updatedDate: '2022-06-23T23:53:50.825Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'Inspection',
    assetHistoryType: 'insight',
    date: '2022-06-23T23:53:50.825Z',
  },
  {
    id: 'c38959b1-63d9-46e5-929c-096cce1c2331',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: 'L1',
    sequenceNumber: '60MP-T-1326',
    priority: 1,
    status: Status.limitedAvailability,
    issueType: 'noIssue',
    issueName: '',
    insightName: '',
    summary: 'TEST',
    description: 'TEST',
    reporterName: 'Regression - Gmail May 17',
    assignedTo: '60-MP_QAWorkGroup',
    dueDate: '2022-05-23T16:00:00.000Z',
    createdDate: '2022-05-24T08:15:48.415Z',
    updatedDate: '2022-05-24T08:15:48.415Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 59,
    assigneeType: 'workGroup',
    assigneeId: '24ab188e-3419-4b9e-adcd-308e2a9809f4',
    assetHistoryType: 'standardTicket',
    date: '2022-05-24T08:15:48.415Z',
    name: '60MP-T-1326',
    statusCode: 20,
  },
  {
    id: 'e8b95c57-3c77-405a-8296-175841329fa5',
    name: 'House Distribution Board E-DBH-05-01',
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
          workableCheckStatus: 'overdue',
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
    checkRecordSummaryStatus: 'notRequired',
    status: 'notRequired',
    assetHistoryType: 'inspection',
    date: '2022-01-14T10:15:00',
  },
  {
    id: 'df685c9a-e502-478a-a93d-4a8e2ed0dc30',
    name: 'INV-60MP-FLT-B03-01',
    zoneId: '82dd2c26-d985-4a11-a338-fb92efd001ad',
    floorCode: 'B3',
    assetId: '00600000-0000-0000-0000-000000797480',
    assignedWorkgroupId: 'efe565cb-a56f-4af5-876a-56e61f071e42',
    frequency: 8,
    unit: 'hours',
    startDate: '2022-01-05T00:45:00',
    sortOrder: 0,
    checks: [
      {
        id: '5af90237-103c-427a-aa39-2b29cee9f68f',
        inspectionId: 'df685c9a-e502-478a-a93d-4a8e2ed0dc30',
        name: 'This is a test for insight color (inspections)',
        type: 'Numeric',
        typeValue: 'test',
        decimalPlaces: 2,
        minValue: 2,
        maxValue: 5,
        isArchived: false,
        isPaused: false,
        canGenerateInsight: true,
        lastSubmittedRecord: {
          id: '72b10288-e88e-4232-a7e5-feecd9d591de',
          inspectionId: 'df685c9a-e502-478a-a93d-4a8e2ed0dc30',
          checkId: '5af90237-103c-427a-aa39-2b29cee9f68f',
          inspectionRecordId: 'e8be78e3-10d7-4237-806b-80393e698394',
          status: 'completed',
          submittedUserId: 'bc335685-39f6-43ad-8c53-86a3c04eb6fe',
          submittedDate: '2022-01-06T05:17:30.973Z',
          submittedSiteLocalDate: '2022-01-06T16:17:30.973Z',
          numberValue: 88,
          effectiveDate: '2022-01-06T05:00:00.000Z',
          notes: 'testing',
        },
        statistics: {
          checkRecordCount: 0,
          lastCheckSubmittedEntry: '',
          lastCheckSubmittedDate: '2022-01-06T05:17:30.973Z',
          workableCheckStatus: 'overdue',
          nextCheckRecordDueTime: '2022-08-25T22:00:00.000Z',
        },
      },
    ],
    checkRecordCount: 0,
    workableCheckCount: 1,
    completedCheckCount: 0,
    nextCheckRecordDueTime: '2022-08-25T22:00:00.000Z',
    assignedWorkgroupName: 'Test Group',
    zoneName: 'New Zone (After fix)',
    assetName: 'FLT-B03-01',
    checkRecordSummaryStatus: 'overdue',
    status: 'overdue',
    assetHistoryType: 'inspection',
    date: '2022-01-05T00:45:00',
  },
]

const noAssetFound = 'No Asset History Found'
const summary = 'Summary'
const dateCreated = 'Date Created'
const ticket = 'Ticket'
const scheduledTicket = 'Scheduled Ticket'
const inspection = 'Inspection'
const insight = 'Insight'
const low = 'Low'
const medium = 'Medium'
const high = 'High'
const critical = 'Critical'
const lastFaultedOccurrence = 'Last Faulted Occurrence'
const dateColumnHeader = `${lastFaultedOccurrence} / ${dateCreated}`
const dateColumnHeaderInsightOnly = lastFaultedOccurrence

const Wrapper = ({ children }) => (
  <BaseWrapper
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'plainText.noAssetFound': noAssetFound,
            'labels.summary': summary,
            'plainText.dateCreated': dateCreated,
            'plainText.ticket': ticket,
            'plainText.scheduledTicket': scheduledTicket,
            'plainText.inspection': inspection,
            'plainText.insight': insight,
            'plainText.low': low,
            'plainText.medium': medium,
            'plainText.high': high,
            'plainText.critical': critical,
            'plainText.lastFaultedOccurrence': lastFaultedOccurrence,
          },
        },
      },
      lng: 'en',
      fallbackLng: ['en'],
    }}
  >
    <TicketStatusesStubProvider
      data={[
        {
          customerId: 'id-1',
          status: Status.open,
          color: 'yellow',
          statusCode: 0,
          tab: Tab.open,
        },
        {
          customerId: 'id-1',
          status: Status.inProgress,
          color: 'green',
          statusCode: 10,
          tab: Tab.open,
        },
        {
          customerId: 'id-1',
          status: Status.limitedAvailability,
          color: 'yellow',
          tab: Tab.open,
          statusCode: 20,
        },
        {
          customerId: 'id-1',
          status: Status.reassign,
          color: 'yellow',
          tab: Tab.open,
          statusCode: 30,
        },
      ]}
    >
      <AssetHistoryProvider
        siteId="site123"
        assetId="id-1"
        twinId="twinId-123"
        filterType="all"
        setFilterType={jest.fn()}
        setInsightId={jest.fn()}
      >
        {children}
      </AssetHistoryProvider>
    </TicketStatusesStubProvider>
  </BaseWrapper>
)

describe('Twin view: Asset History Table', () => {
  test('No asset history found', async () => {
    const { container } = render(
      <AssetHistoryTable
        assetHistory={[]}
        assetHistoryQueryStatus="success"
        onSelectItem={() => {}}
        filterType="inspection"
      />,
      {
        wrapper: Wrapper,
      }
    )
    // wait until wrapper is rendered
    await waitFor(() =>
      expect(container.querySelectorAll('div').length).toBeGreaterThan(0)
    )

    expect(screen.queryByText(noAssetFound)).toBeInTheDocument()
  })

  test('Table with asset history', async () => {
    const { container } = render(
      <AssetHistoryTable
        assetHistory={assetHistory}
        assetHistoryQueryStatus="success"
        onSelectItem={() => {}}
        filterType="inspection"
      />,
      {
        wrapper: Wrapper,
      }
    )
    // wait until wrapper is rendered
    await waitFor(() =>
      expect(container.querySelectorAll('div').length).toBeGreaterThan(0)
    )

    // check table column's header
    expect(screen.queryByText('labels.type')).toBeInTheDocument()
    expect(screen.queryByText(summary)).toBeInTheDocument()
    expect(screen.queryByText('labels.priority')).toBeInTheDocument()
    expect(screen.queryByText('labels.status')).toBeInTheDocument()
    expect(screen.queryByText(dateColumnHeader)).toBeInTheDocument()

    // check correct rows
    // types column
    const expectedNumberOfStandardTicketsInTable = assetHistory.filter(
      ({ assetHistoryType }) => assetHistoryType === 'standardTicket'
    ).length
    const expectedNumberOfScheduledTicketsInTable = assetHistory.filter(
      ({ assetHistoryType }) => assetHistoryType === 'scheduledTicket'
    ).length
    const expectedNumberOfInspectionsInTable = assetHistory.filter(
      ({ assetHistoryType }) => assetHistoryType === 'inspection'
    ).length
    const expectedNumberOfInsightsInTable = assetHistory.filter(
      ({ assetHistoryType }) => assetHistoryType === 'insight'
    ).length

    expect(screen.getAllByText(ticket)).toHaveLength(
      expectedNumberOfStandardTicketsInTable
    )
    expect(screen.getAllByText(scheduledTicket)).toHaveLength(
      expectedNumberOfScheduledTicketsInTable
    )
    expect(screen.getAllByText(inspection)).toHaveLength(
      expectedNumberOfInspectionsInTable
    )
    expect(screen.getAllByText(insight)).toHaveLength(
      expectedNumberOfInsightsInTable
    )

    // ID column
    assetHistory.forEach(({ name, description }) => {
      expect(screen.getByText(name ?? description)).toBeInTheDocument()
    })

    // Priority column
    const expectedNumberOfLowPriority = assetHistory.filter(
      ({ priority }) => priority === 4
    ).length
    const expectedNumberOfMediumPriority = assetHistory.filter(
      ({ priority }) => priority === 3
    ).length
    const expectedNumberOfHighPriority = assetHistory.filter(
      ({ priority }) => priority === 2
    ).length
    const expectedNumberOfUrgentPriority = assetHistory.filter(
      ({ priority }) => priority === 1
    ).length

    expect(screen.getAllByText(low)).toHaveLength(expectedNumberOfLowPriority)
    expect(screen.getAllByText(medium)).toHaveLength(
      expectedNumberOfMediumPriority
    )
    expect(screen.getAllByText(high)).toHaveLength(expectedNumberOfHighPriority)
    expect(screen.getAllByText(critical)).toHaveLength(
      expectedNumberOfUrgentPriority
    )

    // Status column
    assetHistory.forEach(({ lastStatus = '', status }) => {
      expect(
        screen.getByText(`${lastStatus || status}`, { exact: false })
      ).toBeInTheDocument()
    })
  })

  test('Load main table columns if asset type is not insight', async () => {
    render(
      <AssetHistoryTable
        filterType="standardTicket"
        assetHistory={assetHistory}
        assetHistoryQueryStatus="success"
        onSelectItem={() => {}}
      />,
      { wrapper: Wrapper }
    )

    expect(await screen.findByText('labels.type')).toBeInTheDocument()
    expect(await screen.findByText(summary)).toBeInTheDocument()
    expect(await screen.findByText('labels.priority')).toBeInTheDocument()
    expect(await screen.findByText('labels.status')).toBeInTheDocument()
    expect(screen.queryByText(dateColumnHeader)).toBeInTheDocument()
  })

  test('Date column header should be "Last Faulted Occurrence" when asset type is "insight"', async () => {
    render(
      <AssetHistoryTable
        filterType="insight"
        assetHistory={assetHistory}
        assetHistoryQueryStatus="success"
        onSelectItem={() => {}}
      />,
      { wrapper: Wrapper }
    )

    expect(screen.queryByText(dateColumnHeader)).not.toBeInTheDocument()
    expect(screen.queryByText(dateColumnHeaderInsightOnly)).toBeInTheDocument()
  })
})
