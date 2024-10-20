/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { act, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { formatDateTime, TicketStatusesStubProvider } from '@willow/common'
import { makeInsight } from '@willow/common/insights/testUtils'
import {
  openDropdown,
  supportDropdowns,
} from '@willow/ui/utils/testUtils/dropdown'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import ReactRouter from 'react-router'
import { v4 as uuidv4 } from 'uuid'
import useMultipleSearchParams from '../../../../../../common/src/hooks/useMultipleSearchParams'
import { InsightTab } from '../../../../../../common/src/insights/insights/types'
import { diagnostics } from '../../../../mockServer/diagnostics'
import { diagnosticInsightSummary } from '../../../../mockServer/diagnosticSummary'
import { occurrences } from '../../../../mockServer/occurrences'
import { handlers as timeZonesHandlers } from '../../../../mockServer/timeZones'
import SitesProvider from '../../../../providers/sites/SitesStubProvider'
import SiteProvider from '../../../../providers/sites/SiteStubProvider'
import Layout from '../../../../views/Layout/Layout/Layout'
import InsightNode from '../InsightNode'

supportDropdowns()
jest.mock('../../../../../../common/src/hooks/useMultipleSearchParams')
const mockedUseMultipleSearchParams = jest.mocked(useMultipleSearchParams)

// the Fetch component somewhere deep in the component tree
// is making a call that is not relevant to the test suite
jest.mock('../../../../../../ui/src/hooks/useApi/useApi', () =>
  jest.fn(() => ({
    get: jest.fn(),
  }))
)

const handlers = [
  rest.put(
    'api/v2/sites/:siteId/insights/:insightId/status',
    (_req, res, ctx) => res(ctx.status(204))
  ),
  rest.get('/api/sites/:siteId/insights/:insightId', (req, res, ctx) => {
    const { siteId } = req.params
    return res(ctx.json(makeInsight({ id: insightId, siteId })))
  }),
  rest.get('/api/insights/:insightId', (_req, res, ctx) =>
    res(ctx.json(makeInsight({ id: insightId, siteId })))
  ),
  rest.post('/api/insights', (_req, res, ctx) =>
    res(ctx.json({ items: [makeInsight({ id: insightId, siteId })] }))
  ),
  rest.get('/api/sites/:siteId/insights/:insightId/tickets', (_req, res, ctx) =>
    res(ctx.json(tickets))
  ),
  rest.get('/api/sites/:siteId/tickets/:ticketId', (_req, res, ctx) =>
    res(ctx.json(tickets[0]))
  ),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/occurrences',
    (_req, res, ctx) => res(ctx.json(occurrences))
  ),
  rest.get('/api/sites/:siteId/models', (_req, res, ctx) => res(ctx.json({}))),
  rest.get('/api/sites/:siteId/insights/:insightId/points', (_req, res, ctx) =>
    res(
      ctx.json({
        insightPoints: [],
        impactScorePoints: [],
      })
    )
  ),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/activities',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/twins/tree', (_req, res, ctx) => res(ctx.json({}))),
  rest.get('/api/tickets/ticketCategoricalData', (_req, res, ctx) =>
    res(ctx.json({}))
  ),
  rest.get('/api/sites/:siteId/equipments/:equipmentId', (req, res, ctx) =>
    res(
      ctx.json({
        id: req.params.equipmentId,
        name: 'VAV-CN-L01-02',
        customerId: uuidv4(),
        siteId,
        points: [],
        tags: [],
        pointTags: [],
      })
    )
  ),
  rest.get('/api/contactus/categories', (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/sites/:siteId/insights/:insightId/points', (_req, res, ctx) =>
    res(
      ctx.json({
        insightPoints: [],
        impactScoresPoints: [],
      })
    )
  ),
  rest.get('/api/v2/sites/:siteId/twins/:twinId', (_req, res, ctx) =>
    res(ctx.json({}))
  ),
  rest.get(
    '/api/insights/:insightId/occurrences/diagnostics',
    (_req, res, ctx) => res(ctx.json(diagnostics))
  ),
  rest.get('/api/insights/:insightId/diagnostics/snapshot', (_req, res, ctx) =>
    res(ctx.json(diagnosticInsightSummary))
  ),
  ...timeZonesHandlers,
]

const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.restoreAllMocks()
  mockedUseMultipleSearchParams.mockClear()
})
afterAll(() => server.close())

describe('InsightNode', () => {
  test('expect to see no insight can be found message when api returns 404', async () => {
    server.use(
      rest.get('/api/insights/:insightId', (_req, res, ctx) =>
        res(ctx.status(404), ctx.json({ message: 'insight not found' }))
      )
    )
    jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
      insightId,
      siteId,
    })
    mockedUseMultipleSearchParams.mockImplementation(() => [{}, jest.fn()])
    render(<InsightNode />, {
      wrapper: Wrapper,
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await waitFor(() => {
      expect(screen.queryByText(noInsightCanBeFound)).not.toBeNull()
    })
  })

  test('expect to see error message when api call fails', async () => {
    server.use(
      rest.get('/api/insights/:insightId', (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'fetch error' }))
      )
    )
    jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
      insightId,
      siteId,
    })
    mockedUseMultipleSearchParams.mockImplementation(() => [{}, jest.fn()])
    render(<InsightNode />, {
      wrapper: Wrapper,
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await waitFor(() => {
      expect(screen.queryByText(errorOccurred)).not.toBeNull()
    })
  })

  test('should render correctly', async () => {
    server.use(
      rest.get('/api/insights/:insightId', (req, res, ctx) => {
        const { insightId } = req.params
        return res(
          ctx.json({
            ...makeInsight({ id: insightId, siteId }),
            lastStatus: 'resolved',
            asset: { name: 'vav-box' },
          })
        )
      })
    )
    jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
      insightId,
      siteId,
    })
    mockedUseMultipleSearchParams.mockImplementation(() => [{}] as any)
    render(<InsightNode />, {
      wrapper: Wrapper,
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    // expect Insight Workflow Status Pill to say "Resolved"
    await waitFor(() => {
      expect(screen.getByText(resolvedText)).toBeInTheDocument()
    })

    for (const tab of leftPanelTabs) {
      expect(
        within(screen.getByTestId(leftPanelTabsId)).getByText(tab)
      ).toBeInTheDocument()
    }
  })

  test.each([
    {
      params: {
        insightTab: InsightTab.Actions,
        ticketId: 'cb6467ba-a358-4204-ad09-6f61e8ee275a',
        action: 'ticket',
      },
      expectedText: 'Ticket',
    },
    {
      params: {
        insightTab: InsightTab.Actions,
        action: 'newTicket',
      },
      expectedText: 'Submit Ticket',
    },
    {
      params: {
        insightTab: InsightTab.Actions,
        action: 'resolve',
      },
      expectedText: 'Resolve Insight',
    },
  ])(
    `expect to see $params.action modal open when params are present in url`,
    async ({ params, expectedText }) => {
      jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
        insightId,
        siteId,
        ticketId,
      })
      mockedUseMultipleSearchParams.mockImplementation(() => [params] as any)
      render(<InsightNode />, {
        wrapper: Wrapper,
      })

      await waitFor(() =>
        expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
      )

      await waitFor(() =>
        expect(screen.queryAllByText(expectedText).length).toBeGreaterThan(0)
      )
    }
  )

  test.each([
    {
      params: {
        insightTab: InsightTab.Actions,
        ticketId: 'cb6467ba-a358-4204-ad09-6f61e8ee275a',
        action: 'ticket',
        insightId: '123',
        siteId: '456',
      },
    },
  ])('check badge number on activity and actions tab', async ({ params }) => {
    server.use(
      rest.get(
        '/api/sites/:siteId/insights/:insightId/activities',
        (_req, res, ctx) => res(ctx.json(activities))
      )
    )

    jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
      insightId,
      siteId,
      ticketId,
    })
    mockedUseMultipleSearchParams.mockImplementation(() => [params] as any)
    const { container } = render(<InsightNode />, {
      wrapper: Wrapper,
    })

    await waitFor(() =>
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    )

    const selectedTab = actions
    const expectedActivityBadgeCount = '2'
    const expectedActionsBadgeCount = '1'

    for (const tabText of tabTexts) {
      const tab = within(screen.getByTestId(leftPanelTabsId)).getByText(tabText)
      expect(tab).toBeInTheDocument()
      expect(tab.parentElement).toHaveAttribute(
        'aria-selected',
        (tabText === selectedTab).toString()
      )
      if (tabText === actions) {
        expect(
          within(tab.parentElement!).queryByText(expectedActionsBadgeCount)
        ).toBeInTheDocument()
      }
      if (tabText === activity) {
        expect(
          within(tab.parentElement!).queryByText(expectedActivityBadgeCount)
        ).toBeInTheDocument()
      }
    }
  })

  test('Occurrence tab "state" filter (Faulty, Healthy, Insufficient Data) should work as expected', async () => {
    jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
      insightId,
      siteId,
      ticketId,
    })
    mockedUseMultipleSearchParams.mockImplementation(
      () =>
        [
          {
            insightTab: InsightTab.Occurrences,
          },
        ] as any
    )
    const { container } = render(<InsightNode />, {
      wrapper: Wrapper,
    })
    server.use(
      rest.get(
        '/api/sites/:siteId/insights/:insightId/occurrences',
        (_req, res, ctx) => res(ctx.json(occurrences))
      )
    )

    await waitFor(() =>
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    )

    const faultyOccurrenceIds = occurrences
      .filter((o) => o.isValid && o.isFaulted)!
      .map((o) => o.id)
    const insufficientDataOccurrenceIds = occurrences.filter((o) => !o.isValid)
    const healthyOccurrenceIdsCount =
      occurrences.length -
      faultyOccurrenceIds.length -
      insufficientDataOccurrenceIds.length

    const allOccurrenceHeaders = screen
      .queryAllByTestId('occurrence-header')
      .map((header) => header.textContent)

    expect(
      allOccurrenceHeaders.filter((header) => header === faulted).length
    ).toBe(faultyOccurrenceIds.length)
    expect(
      allOccurrenceHeaders.filter((header) => header === insufficientData)
        .length
    ).toBe(insufficientDataOccurrenceIds.length)
    expect(
      allOccurrenceHeaders.filter((header) => header === healthy).length
    ).toBe(healthyOccurrenceIdsCount)

    // click on states filter
    const statesFilter = screen.queryByTestId('occurrence-state-filter-button')
    await act(async () => {
      userEvent.click(statesFilter!)
    })
    // expect to see all the options
    for (const state of ['faulted', 'insufficientData', 'healthy']) {
      const checkbox = screen.queryByTestId(`occurrence-state-filter-${state}`)
      expect(checkbox).toBeInTheDocument()
    }

    // select healthy state
    const healthyCheckbox = screen.queryByTestId(
      'occurrence-state-filter-healthy'
    )
    await act(async () => {
      userEvent.click(healthyCheckbox!)
    })
    // close the dropdown
    await act(async () => {
      userEvent.click(statesFilter!)
    })

    const allOccurrenceHeadersAfterHealthyFilter = screen
      .queryAllByTestId('occurrence-header')
      .map((header) => header.textContent)

    // expect to see only healthy occurrences
    await waitFor(() => {
      expect(
        allOccurrenceHeadersAfterHealthyFilter.filter(
          (header) => header === healthy
        ).length
      ).toBe(healthyOccurrenceIdsCount)
      expect(
        allOccurrenceHeadersAfterHealthyFilter.filter(
          (header) => header === faulted
        ).length
      ).toBe(0)
      expect(
        allOccurrenceHeadersAfterHealthyFilter.filter(
          (header) => header === insufficientData
        ).length
      ).toBe(0)
    })

    // open state filter, click on insufficientData and close the dropdown
    await act(async () => {
      userEvent.click(statesFilter!)
    })
    await act(async () => {
      userEvent.click(
        screen.queryByTestId('occurrence-state-filter-insufficientData')!
      )
    })
    await act(async () => {
      userEvent.click(statesFilter!)
    })
    const allOccurrenceHeadersAfterFilters = screen
      .queryAllByTestId('occurrence-header')
      .map((header) => header.textContent)

    // expect to see only healthy and insufficientData occurrences
    await waitFor(() => {
      expect(
        allOccurrenceHeadersAfterFilters.filter((header) => header === healthy)
          .length
      ).toBe(healthyOccurrenceIdsCount)
      expect(
        allOccurrenceHeadersAfterFilters.filter((header) => header === faulted)
          .length
      ).toBe(0)
      expect(
        allOccurrenceHeadersAfterFilters.filter(
          (header) => header === insufficientData
        ).length
      ).toBe(insufficientDataOccurrenceIds.length)
    })
  }, 50000)

  test('Diagnostic', async () => {
    const mockedSetSearchParams = jest.fn()
    server.use(
      rest.get('/api/sites/:siteId/insights/:insightId', (_req, res, ctx) =>
        res(
          ctx.json({
            ...makeInsight({ id: insightId, siteId }),
          })
        )
      )
    )
    jest.spyOn(ReactRouter, 'useParams').mockReturnValue({
      insightId,
      siteId,
    })
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {},
      mockedSetSearchParams,
    ])
    const { container, rerender } = render(<InsightNode enableDiagnostics />, {
      wrapper: Wrapper,
    })

    // expect all tabs are visible
    for (const tab of leftPanelTabs) {
      expect(
        within(screen.getByTestId(leftPanelTabsId)).getByText(tab)
      ).toBeInTheDocument()
    }
    // expect diagnostic tab is visible when enableDiagnostics is true
    expect(
      within(screen.getByTestId(leftPanelTabsId)).getByText(diagnosticText)
    ).toBeInTheDocument()

    // by default, summary tab should be active
    expect(screen.queryByText(summary)?.parentElement).toHaveAttribute(
      'data-active',
      'true'
    )

    // click on diagnostic tab and expect setSearchParams is called
    // and expect query string to be updated with diagnostic tab
    const diagnosticTab = screen.getByText(diagnosticText)
    await act(async () => {
      userEvent.click(diagnosticTab)
    })
    expect(mockedSetSearchParams).toBeCalledTimes(1)
    expect(mockedSetSearchParams).toBeCalledWith({
      insightTab: InsightTab.Diagnostics,
      period: null,
    })

    const { started: faultyStartISOString, ended: faultyEndISOString } =
      occurrences.find((o) => o.isValid && o.isFaulted)!

    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        insightTab: InsightTab.Diagnostics,
      },
      mockedSetSearchParams,
    ])
    rerender(<InsightNode enableDiagnostics />)
    // now that diagnostic tab is active
    expect(screen.queryByText(diagnosticText)?.parentElement).toHaveAttribute(
      'data-active',
      'true'
    )

    const [faultyStartRegionalString, faultyEndRegionalString] = [
      formatDateTime({
        value: faultyStartISOString,
        timeZone,
        language,
      }),
      formatDateTime({
        value: faultyEndISOString,
        timeZone,
        language,
      }),
    ]

    // expect faulty occurrence date time string to be visible
    expect(
      screen.queryByText(
        `${faultyStartRegionalString} - ${faultyEndRegionalString}`
      )
    ).toBeInTheDocument()

    // expect "Monitor All" button to be visible
    const monitorAllButton = screen.queryByText(monitorAllText)
    expect(monitorAllButton).toBeInTheDocument()
    await act(async () => {
      userEvent.click(monitorAllButton!)
    })

    // query string should be updated with faulty occurrence date time string
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        insightTab: InsightTab.Diagnostics,
        period: `${faultyStartISOString} - ${faultyEndISOString}`,
      },
      mockedSetSearchParams,
    ])
    rerender(<InsightNode enableDiagnostics />)
    await waitFor(() =>
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    )

    const graphContentContainer = screen.getByTestId('graph-content-container')
    await waitFor(() => {
      expect(graphContentContainer).not.toBeNull()
    })

    // all diagnostics are visible on the graph
    await waitFor(() => {
      for (const { id } of diagnostics) {
        expect(
          screen.queryByTestId(`tab-timeSeries-graph-${id}`)
        ).not.toBeNull()
      }
    })

    // calculate and assert padded start and end time strings to be visible
    // padded start time = faulty start time - 10% of difference between faulty start and end time
    // and padded end time = faulty end time + 10% of difference between faulty start and end time
    const differenceInMilliseconds =
      new Date(faultyEndISOString).getTime() -
      new Date(faultyStartISOString).getTime()
    const millisecondsDiffWithPadding = differenceInMilliseconds * 0.1
    const paddedStartISOString = new Date(
      new Date(faultyStartISOString).getTime() - millisecondsDiffWithPadding
    ).toISOString()
    const paddedEndISOString = new Date(
      new Date(faultyEndISOString).getTime() + millisecondsDiffWithPadding
    ).toISOString()
    const paddedRegionalDateTimeString = `${formatDateTime({
      value: paddedStartISOString,
      timeZone,
      language,
    })} - ${formatDateTime({
      value: paddedEndISOString,
      timeZone,
      language,
    })}`

    // open date picker and pick 7D option which is not a faulty occurrence period
    const miniTimeSeriesHeader = screen.getByTestId('mini-time-series-header')

    await waitFor(() => {
      // datetime values within a second are considered to be same
      // so "2023-10-13T14:47:59.999Z" is same as "2023-10-13T14:48:00.000Z"
      const [startDateTimeString, endDateTimeString] = (
        miniTimeSeriesHeader.querySelector('span > span')?.textContent || ''
      ).split(' - ')
      const [startDateOnHeader, endDateOnHeader] =
        paddedRegionalDateTimeString.split(' - ')
      expect(isWithinMinute(startDateTimeString, startDateOnHeader)).toBe(true)
      expect(isWithinMinute(endDateTimeString, endDateOnHeader)).toBe(true)
    })

    const datePicker = within(miniTimeSeriesHeader).getAllByRole('button')[0]
    openDropdown(datePicker)
    const sevenDaysButton = screen.queryByText('7D')
    await waitFor(() => {
      expect(sevenDaysButton).toBeInTheDocument()
    })
    await act(async () => {
      userEvent.click(sevenDaysButton!)
    })
    const now = new Date().toISOString()
    const startDateTimeCalled = mockedSetSearchParams.mock.calls
      .at(-1)[0]
      .period.split(' - ')[0]
    const endDateTimeCalled = mockedSetSearchParams.mock.calls
      .at(-1)[0]
      .period.split(' - ')[1]

    // expect now and endDateTimeCalled to be within 20 seconds of each other
    // because by the time the test reaches this point, the endDateTimeCalled
    // is already several seconds old
    expect(
      new Date(now).getTime() - new Date(endDateTimeCalled).getTime()
    ).toBeLessThan(20 * 1000)

    await waitFor(() =>
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    )

    // re-render with custom period and expect time range has no padding
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        insightTab: InsightTab.Diagnostics,
        period: `${startDateTimeCalled} - ${endDateTimeCalled}`,
      },
      mockedSetSearchParams,
    ])
    rerender(<InsightNode enableDiagnostics />)
    await waitFor(() =>
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    )
    const expectedDateTimeRangeWithNoPadding = `${formatDateTime({
      value: startDateTimeCalled,
      timeZone,
      language,
    })} - ${formatDateTime({
      value: endDateTimeCalled,
      timeZone,
      language,
    })}`
    expect(
      within(screen.getByTestId('mini-time-series-header')).queryByText(
        expectedDateTimeRangeWithNoPadding
      )
    ).toBeInTheDocument()

    // click on "close" button and expect all diagnostics to be removed from graph
    const closeButton = within(
      screen.queryAllByText(timeSeriesText).at(-1)!
    ).queryByText('close')
    await act(async () => {
      userEvent.click(closeButton!)
    })
    expect(mockedSetSearchParams).toBeCalledWith({
      period: null,
    })
    await waitFor(() => {
      for (const { id } of diagnostics) {
        expect(screen.queryByTestId(`tab-timeSeries-graph-${id}`)).toBeNull()
      }
    })
  }, 50000)
})

const Wrapper = ({ children }) => (
  <BaseWrapper
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'labels.summary': summary,
            'plainText.actions': actions,
            'plainText.activity': activity,
            'headers.open': openText,
            'plainText.occurrences': occurrencesText,
            'plainText.submitTicket': submitTicket,
            'plainText.resolveInsight': 'Resolve Insight',
            'headers.ticket': 'Ticket',
            'headers.resolved': resolvedText,
            'plainText.diagnostics': diagnosticText,
            'plainText.monitorAll': monitorAllText,
            'headers.timeSeries': timeSeriesText,
            'plainText.7D': '7D',
            'interpolation.timelyEnergyToolTip': `energy tooltip {{ timely }}`,
            'interpolation.timelyCostToolTip': `cost tooltip {{ timely }}`,
            'plainText.noInsightCanBeFound': noInsightCanBeFound,
            'plainText.errorOccurred': errorOccurred,
            'plainText.faulted': faulted,
            'plainText.insufficientData': insufficientData,
            'plainText.healthy': healthy,
          },
        },
      },
      lng: 'en',
      fallbackLng: ['en'],
    }}
    user={{
      customer: { features: { isDynamicsIntegrationEnabled: false } } as any,
      saveOptions: jest.fn(),
      options: {
        exportedTimeMachineCsvs: [],
      } as any,
    }}
  >
    <TicketStatusesStubProvider
      data={[
        {
          customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
          statusCode: 30,
          status: 'Closed',
          tab: 'Closed',
          color: 'green',
        } as any,
      ]}
    >
      <SitesProvider sites={mockedSites}>
        <SiteProvider
          site={{
            features: { isTicketingDisabled: true },
          }}
        >
          <Layout>{children}</Layout>
        </SiteProvider>
      </SitesProvider>
    </TicketStatusesStubProvider>
  </BaseWrapper>
)

const { siteId } = diagnostics[0]
const { insightId } = occurrences[0]
const ticketId = 'cb6467ba-a358-4204-ad09-6f61e8ee275a'
const leftPanelTabsId = 'insightNodeLeftPanelTabs'
const summary = 'Summary'
const actions = 'Actions'
const activity = 'Activity'
const occurrencesText = 'Occurrences'
const leftPanelTabs = [summary, actions, activity, occurrencesText]
const openText = 'Open'
const submitTicket = 'Submit Ticket'
const tabTexts = [summary, actions, activity, occurrencesText]
const resolvedText = 'Resolved'
const diagnosticText = 'Diagnostic'
const timeZoneId = 'AUS Eastern Standard Time'
const timeZone = 'Australia/Sydney'
const language = 'en'
const monitorAllText = 'Monitor All'
const timeSeriesText = 'Time Series'
const noInsightCanBeFound = 'No Insight Can Be Found.'
const errorOccurred = 'An error has occurred'
const faulted = 'Faulted'
const insufficientData = 'Insufficient Data'
const healthy = 'Healthy'

const mockedSites = [
  {
    id: siteId,
    name: 'site 1',
    features: { isTicketingDisabled: false, isHideOccurrencesEnabled: true },
    timeZoneId,
    timeZone,
  },
]

const ticketName = '60MP-T-8243'
const tickets = [
  {
    id: 'cb6467ba-a358-4204-ad09-6f61e8ee275a',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    floorCode: 'L1',
    sequenceNumber: ticketName,
    priority: 3,
    statusCode: 30,
    issueType: 'noIssue',
    issueName: '',
    insightId,
    summary: 'VAV-CN-L01-02 Test List',
    description: 'ON\r\nAsset: VAV-CN-L01-02',
    reporterName: 'Sandesh Karwa',
    assignedTo: 'Unassigned',
    dueDate: '2023-06-23T04:00:00.000Z',
    createdDate: '2023-06-22T22:13:17.978Z',
    updatedDate: '2023-08-07T22:43:32.088Z',
    closedDate: '2023-08-07T22:43:32.088Z',
    category: 'Unspecified',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 0,
    assigneeType: 'noAssignee',
    attachments: [],
    comments: [],
  },
]

const activities = [
  {
    activityType: 'InsightActivity',
    activityDate: '2023-07-31T22:15:52.673Z',
    userId: 'e6b0ca7b-4d94-46c7-9c5a-3f3eba6afbf3',
    fullName: 'Siddharth Subramoni',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'New',
      },
      {
        key: 'Priority',
        value: '3',
      },
      {
        key: 'OccurrenceCount',
        value: '1',
      },
      {
        key: 'PreviouslyIgnored',
        value: 'False',
      },
      {
        key: 'PreviouslyResolved',
        value: 'False',
      },
      {
        key: 'ImpactScores',
        value:
          '[{"fieldId":"total_cost_to_date","name":"Total Cost to Date","value":0.0,"unit":"USD"},{"fieldId":"total_energy_to_date","name":"Total Energy to Date","value":0.0,"unit":"kWh"},{"fieldId":"daily_avoidable_energy","name":"Daily Avoidable Energy","value":0.0,"unit":"kWh"},{"fieldId":"priority","name":"Priority","value":0.0,"unit":""},{"fieldId":"daily_avoidable_cost","name":"Daily Avoidable Cost","value":0.0,"unit":"USD"}]',
      },
      {
        key: 'Reason',
      },
      {
        key: 'OccurrenceStarted',
      },
      {
        key: 'OccurrenceEnded',
      },
    ],
  },
  {
    activityType: 'InsightActivity',
    activityDate: '2023-08-09T20:03:40.258Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'InProgress',
      },
      {
        key: 'Priority',
        value: '3',
      },
      {
        key: 'OccurrenceCount',
        value: '1',
      },
      {
        key: 'PreviouslyIgnored',
        value: 'False',
      },
      {
        key: 'PreviouslyResolved',
        value: 'False',
      },
      {
        key: 'ImpactScores',
        value:
          '[{"fieldId":"daily_avoidable_cost","name":"Daily Avoidable Cost","value":0.0,"unit":"USD"},{"fieldId":"total_cost_to_date","name":"Total Cost to Date","value":0.0,"unit":"USD"},{"fieldId":"daily_avoidable_energy","name":"Daily Avoidable Energy","value":0.0,"unit":"kWh"},{"fieldId":"total_energy_to_date","name":"Total Energy to Date","value":0.0,"unit":"kWh"},{"fieldId":"priority","name":"Priority","value":0.0,"unit":""}]',
      },
      {
        key: 'Reason',
      },
      {
        key: 'OccurrenceStarted',
      },
      {
        key: 'OccurrenceEnded',
      },
    ],
  },
]

function isWithinMinute(sourceTimeString: string, targetTimeString: string) {
  const targetDate = new Date(sourceTimeString)
  const givenDate = new Date(targetTimeString)

  const differenceInMinutes =
    Math.abs(targetDate.valueOf() - givenDate.valueOf()) / 1000 / 60

  return differenceInMinutes <= 1
}
