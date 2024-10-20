import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useTicketStatuses } from '@willow/common/providers/TicketStatusesProvider/TicketStatusesProvider'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import {
  clickOption,
  supportDropdowns,
} from '@willow/ui/utils/testUtils/dropdown'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import _, { find } from 'lodash'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import ReactRouter from 'react-router'
import { v4 as uuidv4 } from 'uuid'
import SiteProvider from '../../../providers/sites/SiteStubProvider'
import Tickets from '../Tickets'

jest.mock(
  '@willow/common/providers/TicketStatusesProvider/TicketStatusesProvider'
)
const mockedUseTicketStatuses = jest.mocked(useTicketStatuses)

supportDropdowns()
const handler = [
  rest.get('/api/customers/:customerId/modelsOfInterest', (_req, res, ctx) =>
    res(ctx.json({}))
  ),
  rest.get('/api/sites/:siteId/tickets', (req, res, ctx) =>
    res(ctx.json(tickets))
  ),
  rest.get('/api/tickets', (req, res, ctx) => res(ctx.json(tickets))), // when "All Sites" is selected
  rest.get(
    '/api/sites/:siteId/insights/:insightId/commands',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/insights/:insightId/tickets', (_req, res, ctx) =>
    res(ctx.json([]))
  ),

  rest.get(
    '/api/sites/:siteId/insights/:insightId/occurrences',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/models', (req, res, ctx) => res(ctx.json({}))),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/activities',
    (req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/insights/:insightId/points', (req, res, ctx) =>
    res(
      ctx.json({
        insightPoints: [],
        impactScoresPoints: [],
      })
    )
  ),

  rest.get('/api/sites/:siteId/insights/:insightId', (req, res, ctx) => {
    const { insightId } = req.params
    return res(
      ctx.json({
        insightId,
        sequenceNumber: 'insight-1',
        occurredDate: new Date().toISOString(),
      })
    )
  }),
]
const server = setupServer(...handler)

// We do have a global setting for the media query, but it still fails the tests here,
// so we mock it here before each test to avoid the error.
beforeEach(() => {
  window.matchMedia = jest.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // deprecated
    removeListener: jest.fn(), // deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  }))
})

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  mockedUseTicketStatuses.mockReset()
  jest.resetAllMocks()
})
afterAll(() => server.close())

describe('Tickets', () => {
  test('Ticket table', async () => {
    const { container } = setup({
      tab: 'Open',
      onTabChange: jest.fn(),
    })

    await waitForLoadingToFinish(container)

    // Checking if ticket names are visible in the table
    await assertOnElementsState({
      textsAreVisible: ticketSummaries,
    })

    // Checking the total rows count present in the table
    await waitFor(() => {
      const totalRowText = `Total Rows: ${tickets.length}`
      expect(screen.queryByText(totalRowText)).toBeInTheDocument()
    })
  })

  test.each([
    {
      displayedName: 'Open',
      underlyingName: TicketTab.open,
    },
    {
      displayedName: 'Completed',
      underlyingName: TicketTab.resolved,
    },
    {
      displayedName: 'Closed',
      underlyingName: TicketTab.closed,
    },
  ])(
    'When selected tab is $displayedName, click on any other tabs other than $displayedName will fire handler',
    async ({ displayedName, underlyingName }) => {
      const mockOnTabChange = jest.fn()
      // actual tickets data is irrelevant to this test
      server.use(
        rest.get('/api/sites/:siteId/tickets', (_req, res, ctx) =>
          res(ctx.json([]))
        )
      )
      const { container } = setup({
        tab: underlyingName,
        onTabChange: mockOnTabChange,
      })

      await waitForLoadingToFinish(container)

      await assertOnElementsState({
        textsAreVisible: [displayedName],
      })

      for (const tab of tabs) {
        if (tab.underlyingName !== underlyingName) {
          act(() => {
            userEvent.click(screen.queryByText(tab.displayedName))
            expect(mockOnTabChange).toBeCalledWith(tab.underlyingName)
          })
        }
      }
    }
  )

  test('Assignee Filter should filter based on selection', async () => {
    const ticketsAssignedToRandomDudeA = [ticketSummaryOne]
    const ticketsAssignedToRandomDudeB = [ticketSummaryTwo]
    const { container } = setup({
      siteId: 'site-1',
      tab: 'Open',
      onTabChange: jest.fn(),
    })

    await waitForLoadingToFinish(container)

    // all tickets are visible by default
    await assertOnElementsState({
      textsAreVisible: ticketSummaries,
    })

    // open assignee dropdown
    const assigneeCombobox = screen.getByLabelText('Assignee')
    expect(assigneeCombobox).toBeInTheDocument()
    assigneeCombobox.click()

    // all assignees are visible
    await assertOnElementsState({
      textsAreVisible: randomUsers,
    })

    // click on one user
    await act(async () => {
      clickOption(randomDudeA)
    })

    // only tickets assigned to that user are visible
    await assertOnElementsState({
      textsAreVisible: ticketsAssignedToRandomDudeA,
    })
    await waitFor(() => {
      for (const ticketName of _.difference(
        ticketSummaries,
        ticketsAssignedToRandomDudeA
      )) {
        expect(screen.queryByText(ticketName)).toBeNull()
      }
    })

    // click on another user
    await act(async () => {
      clickOption(randomDudeB)
    })
    // tickets not belong to assigned from above are not visible
    await waitFor(() => {
      for (const ticketName of _.difference(ticketSummaries, [
        ...ticketsAssignedToRandomDudeA,
        ...ticketsAssignedToRandomDudeB,
      ])) {
        expect(screen.queryByText(ticketName)).toBeNull()
      }
    })

    // click on first user again
    await act(async () => {
      clickOption(randomDudeA)
    })
    // now only tickets assigned to 2nd user are visible
    await assertOnElementsState({
      textsAreVisible: ticketsAssignedToRandomDudeB,
    })
    await waitFor(() => {
      for (const ticketName of _.difference(
        ticketSummaries,
        ticketsAssignedToRandomDudeB
      )) {
        expect(screen.queryByText(ticketName)).toBeNull()
      }
    })
  })

  test('Ticket that has insight will show "Willow Insight" as source name', async () => {
    const mockedPush = jest.fn()
    jest.spyOn(ReactRouter, 'useHistory').mockReturnValue({
      push: mockedPush,
    })
    const { container } = setup({
      tab: 'Open',
      onTabChange: jest.fn(),
    })

    await waitForLoadingToFinish(container)

    // all tickets are visible by default
    await assertOnElementsState({
      textsAreVisible: ticketSummaries,
    })

    // assert that ticket-1 has insight, and "Willow Insight" will be shown as source name in the table
    const linkToOpenInsight = screen.queryByText(/willow insight/i, {
      selector: 'a',
    })
    expect(linkToOpenInsight).toBeInTheDocument()

    // click on "Willow Insight" will open Insight Drawer
    await act(async () => {
      userEvent.click(linkToOpenInsight)
    })

    expect(mockedPush).toBeCalledWith('/sites/site-1/insights/insight-1')
  })
})

const randomDudeA = 'Random Dude A'
const randomDudeB = 'Random Dude B'
const randomDudeC = 'Random Dude C'
const ticketOne = 'ticket-1'
const ticketTwo = 'ticket-2'
const ticketThree = 'ticket-3'
const randomUsers = [randomDudeA, randomDudeB, randomDudeC]
const ticketSummaryOne = 'Ticket-Summary-1'
const ticketSummaryTwo = 'Ticket-Summary-2'
const ticketSummaryThree = 'Ticket-Summary-3'
const ticketSummaries = [ticketSummaryOne, ticketSummaryTwo, ticketSummaryThree]

const mockedGetByStatusCode = (code) => find(ticketStatus, { statusCode: code })

const setup = (props) => {
  mockedUseTicketStatuses.mockImplementation(() => ({
    data: ticketStatus,
    isLoading: false,
    queryStatus: 'success',
    getByStatusCode: mockedGetByStatusCode,
  }))
  return render(<Tickets {...props} />, { wrapper: Wrapper })
}
const assertOnElementsState = async ({
  container = screen,
  textsAreVisible = [],
}) => {
  for (const text of textsAreVisible) {
    expect(await container.findByText(text)).toBeInTheDocument()
  }
}

const Wrapper = ({ children }) => (
  <BaseWrapper
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'labels.assignee': 'Assignee',
            'plainText.willow': 'willow',
            'headers.insight': 'insight',
            'plainText.insightDetails': 'insight details',
            'headers.open': 'Open',
            'headers.completed': 'Completed',
            'headers.closed': 'Closed',
          },
        },
      },
      lng: 'en',
      fallbackLng: ['en'],
    }}
  >
    <SiteProvider
      site={{
        features: { isTicketingDisabled: true },
      }}
    >
      {children}
    </SiteProvider>
  </BaseWrapper>
)

const waitForLoadingToFinish = async (container) => {
  await waitFor(() => {
    expect(
      container.getElementsByClassName('MuiDataGrid-overlayWrapper').length
    ).toBe(0)
  })
}

const makeTicket = ({
  siteId,
  sequenceNumber,
  floorCode = 'floor-x',
  priority = 3,
  statusCode = 20,
  issueType = 'asset',
  issueId = uuidv4(),
  insightName = '',
  insightId = '',
  description = 'ticket-description',
  reporterName = 'someone',
  assignedTo = 'someone',
  dueDate = '2022-11-08T16:00:00.000Z',
  createdDate = '2022-11-09T09:46:29.871Z',
  updatedDate = '2022-12-09T18:49:00.792Z',
  resolvedDate = '2022-12-09T18:49:00.792Z',
  category = 'Unspecified',
  sourceName = 'random-source',
  groupTotal = 1293,
  asigneeType = 'customerUser',
  asigneeId = '26936cf4-c44a-4cb0-b7b1-5e39b375cec1',
  summary,
}) => ({
  id: uuidv4(),
  siteId,
  floorCode,
  sequenceNumber,
  priority,
  statusCode,
  issueType,
  issueId,
  insightId,
  insightName,
  description,
  reporterName,
  assignedTo,
  dueDate,
  createdDate,
  updatedDate,
  resolvedDate,
  category,
  sourceName,
  groupTotal,
  asigneeType,
  asigneeId,
  summary,
})

const ticketStatus = [
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 0,
    status: 'Open',
    tab: 'Open',
    color: 'yellow',
  },
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 5,
    status: 'Reassign',
    tab: 'Open',
    color: 'yellow',
  },
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 10,
    status: 'InProgress',
    tab: 'Open',
    color: 'green',
  },
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 15,
    status: 'LimitedAvailability',
    tab: 'Open',
    color: 'yellow',
  },
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 20,
    status: 'Resolved',
    tab: 'Resolved',
    color: 'green',
  },
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 30,
    status: 'Closed',
    tab: 'Closed',
    color: 'green',
  },
  {
    customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
    statusCode: 35,
    status: 'OnHold',
    tab: 'Open',
    color: 'yellow',
  },
]

const tickets = [
  makeTicket({
    siteId: 'site-1',
    sequenceNumber: ticketOne,
    assignedTo: randomDudeA,
    sourceName: 'Platform',
    summary: ticketSummaryOne,
  }),
  makeTicket({
    siteId: 'site-1',
    sequenceNumber: ticketTwo,
    assignedTo: randomDudeB,
    sourceName: 'Platform',
    insightId: 'insight-1',
    summary: ticketSummaryTwo,
  }),
  makeTicket({
    siteId: 'site-1',
    sequenceNumber: ticketThree,
    assignedTo: randomDudeC,
    summary: ticketSummaryThree,
  }),
]

const tabs = [
  {
    displayedName: 'Open',
    underlyingName: TicketTab.open,
  },
  {
    displayedName: 'Completed',
    underlyingName: TicketTab.resolved,
  },
  {
    displayedName: 'Closed',
    underlyingName: TicketTab.closed,
  },
]
