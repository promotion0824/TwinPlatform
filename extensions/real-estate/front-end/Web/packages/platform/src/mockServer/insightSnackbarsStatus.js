import { rest } from 'msw'

export const insightSnackbarsStatus = [
  {
    status: 'readyToResolve',
    count: 1,
    id: 'c1741f9f-e6a2-401c-8e51-05b2de263b5a',
    sourceName: 'willowActivate',
    sourceType: 'willow',
  },
  {
    status: 'resolved',
    count: 10,
    sourceName: 'willowActivate',
  },
  {
    status: 'resolved',
    count: 2,
  },
]

export const handlers = [
  rest.post('/:region/api/insights/snackbars/status', (_req, res, ctx) =>
    res(ctx.delay(0), ctx.json(insightSnackbarsStatus))
  ),
]
