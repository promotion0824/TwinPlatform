import { rest } from 'msw'

export const categories = [
  'fault',
  'energy',
  'alert',
  'note',
  'dataQuality',
  'commissioning',
  'comfort',
  'diagnostic',
  'predictive',
  'alarm',
]

export const handlers = [
  rest.get('/:region/api/categories', (req, res, ctx) =>
    res(ctx.delay(2000), ctx.json(categories))
  ),
]
