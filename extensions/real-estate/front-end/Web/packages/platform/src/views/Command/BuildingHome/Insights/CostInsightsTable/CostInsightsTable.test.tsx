/* eslint-disable no-await-in-loop */
import { act, render, screen, within, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { InsightMetric } from '@willow/common'
import { Insight, SourceType } from '@willow/common/insights/insights/types'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { SiteContext } from '../../../../../providers/sites/SiteContext'
import CostInsightsTable, { formatInsight } from './CostInsightsTable'

const handler = [
  rest.get(`/api/sites/:siteId/insights/:insightId`, (req, res, ctx) => {
    const { insightId } = req.params
    return res(ctx.json(insights.find((insight) => insight.id === insightId)))
  }),
  rest.get(`/api/sites/:siteId/insights/:insightId/tickets`, (_req, res, ctx) =>
    res(ctx.json([]))
  ),

  rest.get(
    '/api/sites/:siteId/insights/:insightId/occurrences',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/models', (_req, res, ctx) => res(ctx.json([]))),

  rest.get(
    `/api/sites/:siteId/insights/:insightId/commands`,
    (req, res, ctx) => {
      const { insightId } = req.params
      return res(
        ctx.json({
          available: {
            insightId,
            pointId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            setPointId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            currentReading: 0,
            originalValue: 0,
            unit: 'celcius',
            type: 'temperature',
            desiredValueLimitation: 0,
          },
        })
      )
    }
  ),
]
const server = setupServer(...handler)
const mockedOnSelectedInsightChange = jest.fn()

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('CostInsightsTable', () => {
  const language = 'en'
  const mockedTFunction = jest.fn().mockImplementation((text: string) => text)

  test('expect to see all column headers and cells in CostInsightsTable', async () => {
    render(
      <CostInsightsTable
        insights={formatInsight(insights)}
        metric={InsightMetric.cost}
        analytics={{ track: jest.fn() }}
        language={language}
        t={mockedTFunction}
        onSelectedInsightChange={mockedOnSelectedInsightChange}
        total={insights.length}
        fetchNextPage={jest.fn()}
      />,
      {
        wrapper: Wrapper,
      }
    )

    await assertOnTableState()
  })

  test('expect to see sorting on occurredDate and floorCode', async () => {
    render(
      <CostInsightsTable
        insights={formatInsight(insights)}
        metric={InsightMetric.cost}
        analytics={{ track: jest.fn() }}
        language={language}
        t={mockedTFunction}
        onSelectedInsightChange={mockedOnSelectedInsightChange}
        total={insights.length}
        fetchNextPage={jest.fn()}
      />,
      {
        wrapper: Wrapper,
      }
    )

    let cells = await screen.findAllByRole('cell')
    let occurredDateCells = cells.filter(
      (cell) => cell.getAttribute('data-field') === occurredDate
    )

    // Assert if Insights Table sorted in descending order of Occurred Date
    occurredDateCells.forEach((occurredDateCell, index) => {
      expect(
        within(occurredDateCell).queryByText(descendingOccurredDate[index])
      ).not.toBeNull()
    })

    let headers = await screen.findAllByRole('columnheader')
    const occurredDateHeader = headers.find(
      (header) => header.getAttribute('data-field') === occurredDate
    )

    await act(async () => {
      // Reset Sorting (Occurred Date)
      userEvent.click(occurredDateHeader!)

      // Sorting in Descending (Occurred Date)
      userEvent.click(occurredDateHeader!)
    })

    cells = await screen.findAllByRole('cell')
    occurredDateCells = cells.filter(
      (cell) => cell.getAttribute('data-field') === occurredDate
    )

    await waitFor(() => {
      expect(
        within(occurredDateHeader!).queryByText(arrowUpward)
      ).not.toBeNull()
    })

    occurredDateCells.forEach((occurredDateCell, index) => {
      expect(
        within(occurredDateCell).queryByText(ascendingOccurredDate[index])
      ).not.toBeNull()
    })

    headers = await screen.findAllByRole('columnheader')

    const floorHeader = headers.find(
      (header) => header.getAttribute('data-field') === floorCode
    )

    await act(async () => {
      // Sorting in Ascending (Floor Code)
      userEvent.click(floorHeader!)
    })

    cells = await screen.findAllByRole('cell')
    const floorCodeCells = cells.filter(
      (cell) => cell.getAttribute('data-field') === floorCode
    )

    // Assert if Insights Table sorted in Ascending order of Floor Code
    await waitFor(() => {
      expect(within(floorHeader!).queryByText(arrowDownward)).toBeNull()
    })

    floorCodeCells.forEach((floorCell, index) => {
      expect(
        within(floorCell).queryByText(ascendingFloorCodes[index])
      ).not.toBeNull()
    })
  })

  test('expect to view noInsightsFound message, when the insights array is empty.', async () => {
    // render CostInsightsTable with empty insights array
    render(
      <CostInsightsTable
        insights={[]}
        metric={InsightMetric.energy}
        analytics={{ track: jest.fn() }}
        language={language}
        t={mockedTFunction}
        onSelectedInsightChange={mockedOnSelectedInsightChange}
        total={insights.length}
        fetchNextPage={jest.fn()}
      />,
      {
        wrapper: Wrapper,
      }
    )

    // expect to see no insights found message when insights prop is an empty array
    expect(await screen.findByText(noInsightsFound)).toBeInTheDocument()
  })
})

const insightIdWithLowYearlyCost = 'ee6c98d3-3166-4f43-9618-d84c442d59be'
const floorOne = 'L01'
const floorTen = 'L10'
const floorSeventyOne = 'L71'
const floorSeven = 'L07'
const ascendingFloorCodes = [floorOne, floorSeven, floorTen, floorSeventyOne]
const insightNameHaveNoScore = 'always bottom when sorting on cost or energy'

const assertOnTableState = async () => {
  const dataFieldIds = ['name', `impactScores`, 'floorCode', 'occurredDate']

  // check all dataFieldIds are present as headers
  for (const columnHeaderLabel of dataFieldIds) {
    const headers = await screen.findAllByRole('columnheader')

    const columnHeader = headers.find(
      (cell) => cell.getAttribute('data-field') === columnHeaderLabel
    )

    expect(columnHeader).toBeInTheDocument()
  }

  // check all dataFieldIds are present as cells
  for (const cellLabel of dataFieldIds) {
    const cells = await (
      await screen.findAllByRole('cell')
    ).filter((cell) => cell.getAttribute('data-field') === cellLabel)

    // number of cell under a column is same as number of insights
    expect(cells.length).toBe(insights.length)
  }
}

const ascendingOccurredDate = [
  'May 25, 2020, 00:15',
  'May 23, 2022, 12:16',
  'Jun 15, 2022, 11:07',
  'Aug 12, 2022, 07:06',
]

const descendingOccurredDate = [
  'Aug 12, 2022, 07:06',
  'Jun 15, 2022, 11:07',
  'May 23, 2022, 12:16',
  'May 25, 2020, 00:15',
]

const Wrapper = ({ children }) => (
  <BaseWrapper
    user={{
      customer,
    }}
  >
    <SiteContext.Provider value={sampleSite}>{children}</SiteContext.Provider>
  </BaseWrapper>
)

const customer = {
  id: 'cus-1',
  name: 'custormer-1',
  features: {
    isConnectivityViewEnabled: false,
  },
}

const noInsightsFound = 'plainText.noInsightsFound'
const arrowDownward = 'arrow_downward'
const arrowUpward = 'arrow_upward'
const occurredDate = 'occurredDate'
const floorCode = 'floorCode'

const insights: Insight[] = [
  {
    id: insightIdWithLowYearlyCost,
    siteId: '926d1b17-05f7-47bb-b57b-75a922e69a20',
    sequenceNumber: 'EUG-I-1',
    floorCode: floorOne,
    type: 'fault',
    equipmentId: '483a09ea-ed9b-4596-b564-6f0ed60248ba',
    name: 'low yearly cost',
    priority: 2,
    status: 'open',
    lastStatus: 'open',
    state: 'active',
    updatedDate: '2022-08-11T05:15:07.794Z',
    occurredDate: '2022-08-12T07:06:56.828Z',
    sourceType: SourceType.app,
    sourceName: 'Ruling Engine V3',
    isSourceIdRulingEngineAppId: false,
    externalId: '',
    occurrenceCount: 179,
    subRowInsightIds: [],
    impactScores: [
      {
        name: 'Daily Avoidable Cost',
        value: 129.5245645465464654,
        unit: 'USD',
      },
      {
        name: 'Total Cost to Date',
        value: 310921.64684864684,
        unit: 'USD',
      },
      {
        name: 'Daily Avoidable Energy',
        value: 100.6865464,
        unit: 'kWh',
      },
      {
        name: 'Total Energy to Date',
        value: 48182.68468,
        unit: 'kWh',
      },
    ],
    previouslyIgnored: 0,
    previouslyResolved: 0,
  },
  {
    id: '3ffa2b9a-9e4c-4e02-a62f-1e24f451e076',
    siteId: '926d1b17-05f7-47bb-b57b-75a922e69a20',
    sequenceNumber: 'WD-I-3',
    floorCode: floorTen,
    equipmentId: '93e71812-2f68-4a5b-acff-fa14a0f3b42e',
    type: 'fault',
    name: 'mid yearly cost',
    priority: 1,
    status: 'inProgress',
    lastStatus: 'inProgress',
    state: 'active',
    updatedDate: '2022-11-24T06:36:46.306Z',
    occurredDate: '2020-05-25T00:15:56.000Z',
    sourceType: SourceType.app,
    sourceName: 'Ruling Engine V3',
    isSourceIdRulingEngineAppId: false,
    externalId: '20200525001',
    occurrenceCount: 1,
    subRowInsightIds: [],
    impactScores: [
      {
        name: 'Daily Avoidable Cost',
        value: 300.6484684,
        unit: 'USD',
      },
      {
        name: 'Total Cost to Date',
        value: 150000.11,
        unit: 'USD',
      },
      {
        name: 'Daily Avoidable Energy',
        value: 50.6841684864,
        unit: 'kWh',
      },
      {
        name: 'Total Energy to Date',
        value: 15200.98798789,
        unit: 'kWh',
      },
    ],
    previouslyIgnored: 0,
    previouslyResolved: 0,
  },
  {
    isSourceIdRulingEngineAppId: true,
    id: '905ea4b4-967d-4de8-b60c-231b5b9e78ea',
    siteId: '926d1b17-05f7-47bb-b57b-75a922e69a20',
    sequenceNumber: '1MW-I-3633',
    floorCode: floorSeventyOne,
    equipmentId: '8c81269a-8f4f-464e-ac57-6804efceb603',
    type: 'fault',
    name: 'top yearly cost',
    priority: 1,
    status: 'open',
    lastStatus: 'open',
    state: 'active',
    sourceType: SourceType.app,
    occurredDate: '2022-05-23T12:16:20.103Z',
    updatedDate: '2022-06-09T19:16:27.614Z',
    externalId: 'Triangle.PsFloat1',
    occurrenceCount: 1,
    sourceName: 'InsightRulingEngine',
    subRowInsightIds: [],
    impactScores: [
      {
        name: 'Daily Avoidable Cost',
        value: 1234.656897,
        unit: 'USD',
      },
      {
        name: 'Total Cost to Date',
        value: 150000.11,
        unit: 'USD',
      },
      {
        name: 'Daily Avoidable Energy',
        value: 50.6841684864,
        unit: 'kWh',
      },
      {
        name: 'Total Energy to Date',
        value: 15200.98798789,
        unit: 'kWh',
      },
    ],
    previouslyIgnored: 0,
    previouslyResolved: 0,
  },
  {
    isSourceIdRulingEngineAppId: true,
    id: 'a0ecc7eb-b51a-4a70-bbae-37119f0fe990',
    siteId: '926d1b17-05f7-47bb-b57b-75a922e69a20',
    sequenceNumber: '1MW-I-3662',
    floorCode: floorSeven,
    equipmentId: '8c81269a-8f4f-464e-ac57-6804efceb603',
    type: 'note',
    name: insightNameHaveNoScore,
    priority: 3,
    status: 'open',
    lastStatus: 'open',
    state: 'active',
    sourceType: SourceType.app,
    occurredDate: '2022-06-15T11:07:35.987Z',
    updatedDate: '2022-06-15T11:20:21.595Z',
    externalId: '',
    occurrenceCount: 1,
    sourceName: 'InsightRulingEngine',
    impactScores: [],
    previouslyIgnored: 0,
    previouslyResolved: 0,
  },
]

const sampleSite = {
  id: '926d1b17-05f7-47bb-b57b-75a922e69a20',
  portfolioId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  name: 'site-one',
  code: 'code-1',
  suburb: 'suburb-1',
  address: 'street-1',
  state: 'state-1',
  postcode: '111-222-333',
  country: 'Canada',
  numberOfFloors: 0,
  logoUrl: 'url-1',
  logoOriginalSizeUrl: 'url-2',
  timeZoneId: 'utc',
  area: 'area-1',
  type: 'type-1',
  status: 'open',
  userRole: 'admin',
  timeZone: 'utc',
  features: {
    isTicketingDisabled: true,
    isInsightsDisabled: true,
    is2DViewerDisabled: true,
    isReportsEnabled: true,
    is3DAutoOffsetEnabled: true,
    isInspectionEnabled: true,
    isOccupancyEnabled: true,
    isPreventativeMaintenanceEnabled: true,
    isCommandsEnabled: true,
    isScheduledTicketsEnabled: true,
    isNonTenancyFloorsEnabled: true,
    isHideOccurrencesEnabled: true,
    isArcGisEnabled: true,
  },
  settings: {
    inspectionDailyReportWorkgroupId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  },
  isOnline: true,
  siteCode: 'code-2',
  siteContactName: 'contact-1',
  siteContactEmail: 'email-1',
  siteContactTitle: 'title-1',
  siteContactPhone: '111-222-333',
  webMapId: 'id-123',
  ticketStats: {
    overdueCount: 0,
    urgentCount: 0,
    highCount: 0,
    mediumCount: 0,
    lowCount: 0,
    openCount: 0,
  },
  insightsStats: {
    openCount: 0,
    urgentCount: 0,
    highCount: 0,
    mediumCount: 0,
    lowCount: 0,
  },
}
