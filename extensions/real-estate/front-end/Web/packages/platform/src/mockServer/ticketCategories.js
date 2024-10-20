import { rest } from 'msw'

const data = {
  ticketStatus: [
    'Open',
    'Reassign',
    'InProgress',
    'LimitedAvailability',
    'Resolved',
    'Closed',
    'OnHold',
  ],
  ticketSubStatus: [
    'AdditionalResourcesRequired',
    'AwaitingParts',
    'DeferredToAfterHours',
    'DeviceInUse',
    'FurtherTroubleshootingRequired',
    'Others',
    'RescheduleOrAwaitingReturn',
  ],
  priorities: ['Urgent', 'High', 'Medium', 'Low'],
  assigneeTypes: ['NoAssignee', 'CustomerUser', 'WorkGroup'],
  jobTypes: [
    {
      id: '7f2eaef3-c3d8-403e-9e3f-d6fadc48ed46',
      name: 'JobType 1',
    },
    {
      id: '5bed99de-03fe-460f-a454-9d49f6cd3951',
      name: 'JobType 2',
    },
    {
      id: 'a500f81e-63af-46ca-a360-9a93a8d9d2e5',
      name: 'JobType 3',
    },
    {
      id: 'd5b8eea8-29fd-48bb-9ee6-f3e06b101414',
      name: 'Recall',
    },
  ],
  servicesNeeded: [
    {
      spaceTwinId: 'INV-60MP',
      serviceNeededList: [
        {
          id: 'a07c2f63-c570-486d-8535-db8182796c9a',
          name: 'Service1',
        },
        {
          id: 'b89e34d4-0c89-4f03-b957-0bad47df7453',
          name: 'Service2',
        },
      ],
    },
    {
      spaceTwinId: 'INV-420GST',
      serviceNeededList: [
        {
          id: 'ed80d2b9-c9ab-497a-8b47-d730c12cbf57',
          name: 'Service3',
        },
        {
          id: 'b89e34d4-0c89-4f03-b957-0bad47df7453',
          name: 'Service4',
        },
      ],
    },
  ],
  requestTypes: [
    'HVAC',
    'Plumbing',
    'Key Management',
    'Electrical',
    'Building Management',
    'Contractor',
    'Radio',
    'Conferences',
    'Facilities',
    'Maintenance',
    'Lighting',
    'Food Service',
    'Building Access',
    'Utilities',
    'Signage',
    'IT',
    'Construction Management',
    'General',
    'Transportation',
    'Training',
    'Unspecified',
    'Service Request',
    'Security',
    'Pest Control',
    'Installation',
    'Parking',
    'Cleaning',
    'Events',
    'Elevator',
    'Audio/Visual',
    'Removals',
  ],
}

export const handlers = [
  rest.get('/api/tickets/ticketCategoricalData', (req, res, ctx) =>
    res(ctx.json(data))
  ),
]
