import _ from 'lodash'
import { rest } from 'msw'
import { v4 as uuidv4 } from 'uuid'
import { Status, Tab } from '@willow/common/ticketStatus'

const getTicketStatuses = (customerId) =>
  [
    {
      status: Status.onHold,
      color: 'yellow',
      tab: Tab.open,
      statusCode: 35,
    },
    {
      status: Status.open,
      color: 'yellow',
      tab: Tab.open,
      statusCode: 0,
    },
    {
      status: Status.inProgress,
      color: 'green',
      tab: Tab.open,
      statusCode: 10,
    },
    {
      status: Status.limitedAvailability,
      color: 'yellow',
      tab: Tab.open,
      statusCode: 15,
    },
    {
      status: Status.reassign,
      color: 'yellow',
      tab: Tab.open,
      statusCode: 5,
    },
    {
      status: Status.resolved,
      color: 'green',
      tab: Tab.resolved,
      statusCode: 20,
    },
    {
      status: Status.closed,
      color: 'green',
      tab: Tab.closed,
      statusCode: 30,
    },
  ].map((ticketStatus) => ({ customerId, ...ticketStatus }))

/**
 * Cached list of tickets created via makeTicket, mapped by ticketId as key.
 */
const cachedTickets = {}

const makeTicket = (siteId, tab, isScheduled) => {
  const availableStatuses = getTicketStatuses().filter((t) => t.tab === tab)
  const ticket = {
    id: uuidv4(),
    siteId,
    floorCode: '',
    sequenceNumber: 'INV1236PHI-T-113',
    priority: _.random(1, 4),
    statusCode:
      availableStatuses[_.random(0, availableStatuses.length - 1)].statusCode,
    issueType: 'noIssue',
    issueName: '',
    insightName: '',
    summary: `test ${_.uniqueId()}`,
    description: 'test',
    reporterName: 'QA INVESTA',
    assignedTo: '126-Phillip_QAWorkGroup',
    createdDate: '2022-10-21T03:26:48.201Z',
    updatedDate: '2022-10-21T03:29:03.323Z',
    resolvedDate: '2022-10-21T03:29:03.322Z',
    categoryId: '82e75ebf-a5f3-47fc-94fd-94babae2f6ee',
    category: 'General',
    sourceName: 'Platform',
    externalId: '',
    groupTotal: 1056,
    assigneeType: 'workGroup',
    assigneeId: '90dab6fa-a8d6-42eb-ab8c-b263f3b3b9a8',
    ...(isScheduled
      ? {
          scheduledDate: '2022-10-01T04:00:01.007Z',
          tasks: [
            {
              id: 'f99aea88-ec46-44ee-966d-bd447471a3f1',
              taskName: '1',
              type: 'Checkbox',
              isCompleted: false,
            },
            {
              id: '0e04027d-cebc-4eb1-9694-db0a1d67c41b',
              taskName: '2',
              type: 'Checkbox',
              isCompleted: false,
            },
          ],
        }
      : {}),
  }

  cachedTickets[ticket.id] = ticket

  return ticket
}

export const handlers = [
  rest.get('/:region/api/sites/:siteId/tickets', (req, res, ctx) =>
    res(
      ctx.delay(2000),
      ctx.json(
        new Array(20)
          .fill()
          .map(() =>
            makeTicket(
              req.params.siteId,
              req.url.searchParams.get('tab'),
              req.url.searchParams.get('scheduled')
            )
          )
      )
    )
  ),
  rest.get('/:region/api/sites/:siteId/tickets/:ticketId', (req, res, ctx) =>
    res(
      ctx.json({
        ...cachedTickets[req.params.ticketId],
        cause: '',
        solution: '',
        notes: '',
        reporterPhone: '',
        reporterEmail: '',
        reporterCompany: '',
        assigneeType: 'noAssignee',
        createdDate: '2022-11-29T06:06:46.513Z',
        updatedDate: '2022-11-29T06:07:48.773Z',
        sourceType: 'app',
        sourceId: '63e3e0ae-2ebb-11ed-a261-0242ac120002',
        occurrence: 0,
        template: false,
        comments: [],
        attachments: [],
        tasks: [],
        diagnostics: [
          {
            id: 'c1741f9f-e6a2-401c-8e51-05b2de263b5a',
            name: 'Rule-Name-06',
          },
          {
            id: 'c1741f9f-e6a2-401c-8e51-05b2de263b5b',
            name: 'Rule-Name-07',
          },
        ],
      })
    )
  ),
  rest.get(
    '/:region/api/customers/:customerId/ticketStatuses',
    (req, res, ctx) => res(ctx.json(getTicketStatuses(req.params.customerId)))
  ),
]
