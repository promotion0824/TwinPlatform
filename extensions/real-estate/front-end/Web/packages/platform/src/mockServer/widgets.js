// eslint-disable-next-line import/prefer-default-export
import { rest } from 'msw'
import { siteIdWithoutDashboard } from './sites'

export const handlers = [
  rest.get('/:region/api/sites/:id/dashboard', (req, res, ctx) => {
    if (req.params.id === siteIdWithoutDashboard) {
      return res(ctx.delay(1000), ctx.json({ widgets: [] }))
    }
  }),
]
