import { rest } from 'msw'

const data = [
  {
    key: 1,
    value: 'Dashboard',
  },
  {
    key: 2,
    value: 'SearchAndExplore',
  },
  {
    key: 3,
    value: 'Report',
  },
  {
    key: 4,
    value: 'Ticket',
  },
  {
    key: 5,
    value: 'Inspection',
  },
  {
    key: 6,
    value: 'Marketplace',
  },
  {
    key: 7,
    value: 'TimeSeries',
  },
  {
    key: 8,
    value: 'Admin',
  },
  {
    key: 9,
    value: 'Insight',
  },
]

export const handlers = [
  rest.get('/:region/api/contactus/categories', (req, res, ctx) =>
    res(ctx.json(data))
  ),
]
