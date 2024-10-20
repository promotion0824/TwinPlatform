import { renderHook, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import useGetTickets from './useGetTickets'
import * as ticketsService from '../../services/Tickets/TicketsService'

const site1Id = '404bd33c-a697-4027-b6a6-677e30a53d07'

const assetTicket = {
  id: 'assetTicket',
  assignedTo: 'Unassigned',
  assigneeType: 'noAssignee' as ticketsService.TicketAssigneeType,
  category: 'Unspecified',
  createdDate: '2022-03-28T11:55:08.639Z',
  description: 'Mbu Ticket Aut.Test',
  externalId: '',
  floorCode: '',
  groupTotal: 3118,
  insightName: '',
  issueName: '',
  issueType: 'noIssue' as ticketsService.IssueType,
  priority: 3,
  reporterName: 'AA Van',
  sequenceNumber: '1MW-T-185646',
  siteId: site1Id,
  sourceName: 'Platform',
  statusCode: 1,
  summary: 'Test - Save button loader issue',
  updatedDate: '2022-03-28T11:55:08.639Z',
}

const siteTicket = {
  id: 'siteTicket',
  assignedTo: 'Unassigned',
  assigneeType: 'noAssignee' as ticketsService.TicketAssigneeType,
  category: 'Unspecified',
  createdDate: '2022-03-28T11:55:08.639Z',
  description: 'Mbu Ticket Aut.Test',
  externalId: '',
  floorCode: '',
  groupTotal: 3118,
  insightName: '',
  issueName: '',
  issueType: 'noIssue' as ticketsService.IssueType,
  priority: 3,
  reporterName: 'AA Van',
  sequenceNumber: '1MW-T-185646',
  siteId: 'site2',
  sourceName: 'Platform',
  statusCode: 1,
  summary: 'Test - Save button loader issue',
  updatedDate: '2022-03-28T11:55:08.639Z',
}

const allTicket = {
  id: 'allTicket',
  assignedTo: 'Unassigned',
  assigneeType: 'noAssignee' as ticketsService.TicketAssigneeType,
  category: 'Unspecified',
  createdDate: '2022-03-28T11:55:08.639Z',
  description: 'Mbu Ticket Aut.Test',
  externalId: '',
  floorCode: '',
  groupTotal: 3118,
  insightName: '',
  issueName: '',
  issueType: 'noIssue' as ticketsService.IssueType,
  priority: 3,
  reporterName: 'AA Van',
  sequenceNumber: '1MW-T-185646',
  siteId: 'site2',
  sourceName: 'Platform',
  statusCode: 1,
  summary: 'Test - Save button loader issue',
  updatedDate: '2022-03-28T11:55:08.639Z',
}

const handlers = [
  rest.get(`/api/sites/:siteId/assets/:assetId/tickets`, (_req, res, ctx) =>
    res(ctx.json([assetTicket]))
  ),
  rest.get(`/api/sites/:siteId/tickets`, (_req, res, ctx) =>
    res(ctx.json([siteTicket]))
  ),
  rest.get(`/api/tickets`, (_req, res, ctx) => res(ctx.json([allTicket]))),
]

const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('useGetTickets', () => {
  test('get site tickets', async () => {
    const { result } = renderHook(
      () => useGetTickets({ siteId: site1Id, tab: TicketTab.open }),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.data?.[0].id).toBe(siteTicket.id)
    })
  })

  test('get all sites tickets', async () => {
    const { result } = renderHook(
      () => useGetTickets({ tab: TicketTab.open }),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.data?.[0].id).toBe(allTicket.id)
    })
  })

  test('get asset tickets', async () => {
    const { result } = renderHook(
      () =>
        useGetTickets({
          siteId: 'siteId',
          assetId: 'assetId',
          tab: TicketTab.open,
        }),
      {
        wrapper: Wrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.data?.[0].id).toBe(assetTicket.id)
    })
  })
})
