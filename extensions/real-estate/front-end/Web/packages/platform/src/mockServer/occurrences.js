import { rest } from 'msw'

export const occurrences = [
  {
    id: '1e2902e6-4e2b-4bbd-8aab-ef69782be477',
    insightId: 'f5b29541-b0aa-4dca-baaf-a8c95c1cd9a9',
    isValid: false,
    isFaulted: false,
    started: '2023-09-10T00:15:00.000Z',
    ended: '2023-09-10T11:15:00.000Z',
    text: 'Insufficient data (Result has 10:45:00 of data) for 11.00 hours',
  },
  {
    id: 'f7f2a46e-8375-4ccf-9eb3-ac5ab0c15e1c',
    insightId: 'f5b29541-b0aa-4dca-baaf-a8c95c1cd9a9',
    isValid: true,
    isFaulted: false,
    started: '2023-09-10T11:15:00.000Z',
    ended: '2023-10-12T13:30:00.000Z',
    text: 'Healthy discharge air temp sensor min=61.4degF ave=65.0degF max=73.1degF',
  },
  {
    id: 'a7b24c6e-2bff-42cd-aedb-d858b1f028b9',
    insightId: 'f5b29541-b0aa-4dca-baaf-a8c95c1cd9a9',
    isValid: true,
    isFaulted: true,
    started: '2023-10-12T13:30:00.000Z',
    ended: '2023-10-13T12:30:00.000Z',
    text: 'Faulted 26 %discharge air temp sensor min=60.9degF ave=65.0degF max=70.1degF',
  },
]

export const handlers = [
  rest.get(
    '/:region/api/sites/:siteId/insights/:insightId/occurrences',
    (_req, res, ctx) => res(ctx.delay(0), ctx.json(occurrences))
  ),
]
