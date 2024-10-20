import axios from 'axios'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { getTickets, TicketsResponse } from './TicketsService'

const ERROR_MESSAGE = 'fetch error'

const siteId = '4e5fc229-ffd9-462a-882b-16b4a63b2a8a'
describe('Tickets service', () => {
  test('should return expected data', async () => {
    const responseData: TicketsResponse = [
      {
        assignedTo: 'Unassigned',
        assigneeType: 'noAssignee',
        category: 'Unspecified',
        createdDate: '2022-03-28T11:55:08.639Z',
        description: 'Mbu Ticket Aut.Test',
        externalId: '',
        floorCode: '',
        groupTotal: 3118,
        id: 'fcce50f6-6b48-4026-818b-07994619b2a9',
        insightName: '',
        issueName: '',
        issueType: 'noIssue',
        priority: 3,
        reporterName: 'AA Van',
        sequenceNumber: '1MW-T-185646',
        siteId: '4e5fc229-ffd9-462a-882b-16b4a63b2a8a',
        sourceName: 'Platform',
        statusCode: 5,
        summary: 'Test - Save button loader issue',
        updatedDate: '2022-03-28T11:55:08.639Z',
      },
    ]
    jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

    const response = await getTickets({ siteId, tab: TicketTab.open })

    expect(response).toMatchObject(responseData)
  })
  test('should return error when exception error happens', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(
      getTickets({ siteId, tab: TicketTab.open })
    ).rejects.toThrowError(ERROR_MESSAGE)
  })
})
