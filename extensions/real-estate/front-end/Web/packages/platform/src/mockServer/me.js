import { rest } from 'msw'
import sites from './sites'

export const handlers = [
  rest.get('/:region/api/me/sites', (req, res, ctx) => res(ctx.json(sites))),
]
