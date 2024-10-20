/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

const activities = [
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'Closed',
      },
      {
        key: 'Comments',
        value: 'Fixed the issue by changing schedule run time.',
      },
    ],
  },
  {
    ticketId: '02824fb9-ae23-4fc3-8854-51d61da76b04',
    activityType: 'TicketAttachment',
    activityDate: '2023-07-21T06:45:15.634Z',
    userId: '8d69ad0b-631a-4efc-bae9-bfe93bbf18c8',
    fullName: 'Investa  Customer Admin',
    sourceType: 'willow',
    activities: [
      {
        key: 'FileName',
        value: 'AC.jpg',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'Closed',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'Complete',
      },
      {
        key: 'Comments',
        value: 'Fixed the issue by changing schedule run time.',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'InProgress',
      },
    ],
  },
  {
    ticketId: '4432406c-2171-4723-bc47-c5fd929ff59c',
    activityType: 'TicketComment',
    activityDate: '2023-04-26T18:58:16.871Z',
    userId: '8d69ad0b-631a-4efc-bae9-bfe93bbf18c8',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Comments',
        value: 'Fixed the issue by changing schedule run time.',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'AssigneeName',
        value: 'John S',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Priority',
        value: 'Medium',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Description',
        value: 'Fixed the issue by changing schedule run time.',
      },
      {
        key: 'DueDate',
        value: '2023-06-14T23:09:53.092Z',
      },
      {
        key: 'Priority',
        value: 'Medium',
      },
      {
        key: 'Status',
        value: 'Open',
      },
      {
        key: 'AssigneeId',
        value: '',
      },
      {
        key: 'AssigneeName',
        value: 'John S',
      },
      {
        key: 'Comments',
        value: 'Fixed the issue by changing schedule run time.',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Description',
        value: 'Fixed the issue by changing schedule run time.',
      },
    ],
  },
  {
    ticketId: 'dbcb7b45-9349-4f1a-8c55-04c4add31b23',
    activityType: 'TicketModified',
    activityDate: '2023-07-07T19:07:06.041Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'DueDate',
        value: '2023-06-14T23:09:53.092Z',
      },
    ],
  },
  {
    activityType: 'InsightActivity',
    activityDate: '2023-06-22T21:50:27.434Z',
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
        value: '2',
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
          '[{"fieldId":"daily_avoidable_cost","name":"Daily Avoidable Cost","value":0.0,"unit":"USD"},{"fieldId":"total_cost_to_date","name":"Total Cost to Date","value":0.0,"unit":"USD"},{"fieldId":"daily_avoidable_energy","name":"Daily Avoidable Energy","value":0.0,"unit":"kWh"},{"fieldId":"priority","name":"Priority","value":0.0,"unit":""},{"fieldId":"total_energy_to_date","name":"Total Energy to Date","value":0.0,"unit":"kWh"}]',
      },
    ],
  },
  {
    ticketId: '85cc3ea6-811d-479a-8646-d3b414424597',
    activityType: 'NewTicket',
    activityDate: '2023-06-14T23:09:53.092Z',
    userId: '8d69ad0b-631a-4efc-bae9-bfe93bbf18c8',
    fullName: 'Bob B',
    sourceType: 'willow',
    activities: [
      {
        key: 'Description',
        value: 'Fixed the issue by changing schedule run time.',
      },
      {
        key: 'DueDate',
        value: '2023-06-14T23:09:53.092Z',
      },
      {
        key: 'Priority',
        value: 'Medium',
      },
      {
        key: 'Status',
        value: 'Open',
      },
      {
        key: 'AssigneeId',
        value: '',
      },
      {
        key: 'AssigneeName',
        value: 'John S',
      },
      {
        key: 'Comments',
        value: 'Fixed the issue by changing schedule run time.',
      },
    ],
  },
  {
    ticketId: '02824fb9-ae23-4fc3-8854-51d61da76b04',
    activityType: 'TicketAttachment',
    activityDate: '2023-07-21T06:45:16.153Z',
    userId: '8d69ad0b-631a-4efc-bae9-bfe93bbf18c8',
    fullName: 'Investa  Customer Admin',
    sourceType: 'willow',
    activities: [
      {
        key: 'FileName',
        value: 'air-condition.jpeg',
      },
    ],
  },
  {
    activityType: 'InsightActivity',
    activityDate: '2023-06-22T21:50:27.434Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'new',
      },
      {
        key: 'Priority',
        value: '3',
      },
      {
        key: 'OccurrenceCount',
        value: '2',
      },
      {
        key: 'PreviouslyIgnored',
        value: 'True',
      },
      {
        key: 'PreviouslyResolved',
        value: 'True',
      },
      {
        key: 'ImpactScores',
        value:
          '[{"fieldId":"daily_avoidable_cost","name":"Daily Avoidable Cost","value":100.0,"unit":"USD"},{"fieldId":"total_cost_to_date","name":"Total Cost to Date","value":200.0,"unit":"USD"},{"fieldId":"daily_avoidable_energy","name":"Daily Avoidable Energy","value":300.0,"unit":"kWh"},{"fieldId":"priority","name":"Priority","value":0.0,"unit":""},{"fieldId":"total_energy_to_date","name":"Total Energy to Date","value":400.0,"unit":"kWh"}]',
      },
    ],
  },
  {
    activityType: 'InsightActivity',
    activityDate: '2023-06-22T21:50:27.434Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'Resolved',
      },
      {
        key: 'Reason',
        value: 'some random text',
      },
    ],
  },
  {
    activityType: 'InsightActivity',
    activityDate: '2023-06-22T21:50:27.434Z',
    userId: '4d595b3f-024b-4ba3-9e7d-220a8695280c',
    fullName: 'Sean Yang',
    sourceType: 'willow',
    activities: [
      {
        key: 'Status',
        value: 'Ignored',
      },
    ],
  },
]

export const handlers = [
  rest.get(
    '/:region/api/sites/:siteId/insights/:insightId/activities',
    (req, res, ctx) => res(ctx.delay(2000), ctx.json(activities))
  ),
]
