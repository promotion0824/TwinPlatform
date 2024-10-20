/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

export const diagnosticInsightSummary = {
  id: '891eba84-b740-47b1-9347-07afbbef600f',
  name: 'VAV-CN-L08-01 List w',
  ruleName: 'Rule-Name-05',
  started: '2023-11-01T20:15:28.221Z',
  ended: '2023-11-05T00:00:00.000Z',
  diagnostics: [
    {
      id: '8e25eb2c-7ae6-4a69-aa74-ebd094a46e90',
      name: '2 HOUR SET 1 COMPLETED WITH INSIGHTS',
      ruleName: 'Rule-Name-06',
      check: true,
      started: '2023-11-01T20:15:28.221Z',
      ended: '2023-11-05T00:00:00.000Z',
      diagnostics: [],
    },
    {
      id: '208666c2-1eed-4620-a687-7ca8898b54fe',
      name: '2 HOUR SET 1 COMPLETED WITH INSIGHTS',
      ruleName: 'Rule-Name-07',
      check: false,
      started: '2023-11-01T20:15:28.221Z',
      ended: '2023-11-05T00:00:00.000Z',
      diagnostics: [],
    },
    {
      id: '6722183e-a107-4b7d-a395-bbd68ef82aa1',
      name: '2 HOUR SET 1 COMPLETED WITH INSIGHTS',
      ruleName: 'Rule-Name-08',
      check: false,
      started: '2023-11-01T20:15:28.221Z',
      ended: '2023-11-05T00:00:00.000Z',
      diagnostics: [],
    },
    {
      id: '686b6e2f-b5c9-47f2-873f-a79b8f199299',
      name: '2 HOUR SET 1 COMPLETED WITH INSIGHTS',
      ruleName: 'Rule-Name-09',
      check: false,
      started: '2023-11-01T20:15:28.221Z',
      ended: '2023-11-05T00:00:00.000Z',
      diagnostics: [],
    },
    {
      id: '1a1034b9-016d-4ee1-a840-973d0b6152c7',
      name: '2 HOUR SET 1 COMPLETED WITH INSIGHTS',
      ruleName: 'Rule-Name-10',
      check: false,
      started: '2023-11-01T20:15:28.221Z',
      ended: '2023-11-05T00:00:00.000Z',
      diagnostics: [],
    },
    {
      id: '4bc78f34-5e5d-4910-8d4e-f21eb9f4e960',
      name: '2 HOUR SET 1 COMPLETED WITH INSIGHTS',
      ruleName: 'Rule-Name-11',
      check: false,
      started: '2023-11-01T20:15:28.221Z',
      ended: '2023-11-05T00:00:00.000Z',
      diagnostics: [],
    },
  ],
}

export const handlers = [
  rest.get(
    '/:region/api/insights/:insightId/diagnostics/snapshot',
    (req, res, ctx) => res(ctx.delay(2000), ctx.json(diagnosticInsightSummary))
  ),
]
