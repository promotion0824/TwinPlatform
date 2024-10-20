import { rest } from 'msw'
import allModels from './allModels'

export const handlers = [
  rest.get('/:region/api/sites/:siteId/models', (req, res, ctx) => {
    return res(ctx.json(allModels))
  }),
]
