import { rest } from 'msw'
import { FLOORS_API_PREFIX } from '../../views/Admin/Portfolios/Sites/SiteModal/services/ThreeDimensionModule/ThreeDimensionModuleService'

const response = {
  id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  name: 'Architecture.nwd',
  visualId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtOTM0NjM4ZTMtNGJkNy00NzQ5LWJkNTItYmQ2ZTQ3ZDBmYmIyLXVhdC9NZWNoYW5pY2FsLUJMREctQkJfMjAyMTA3MjMwODU2MTUubndk',
  sortOrder: 0,
  canBeDeleted: true,
  typeName: 'string',
  groupType: 'string',
  moduleTypeId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  moduleGroup: {
    id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    name: 'string',
    sortOrder: 0,
    siteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  },
}
const floorHandlers = [
  rest.post(`/${FLOORS_API_PREFIX}/:siteId/module`, (req, res, ctx) => {
    const { params } = req
    const { siteId } = params
    if (!siteId) {
      return res(ctx.status(404))
    }

    return res(ctx.json(response))
  }),
  rest.get(`/${FLOORS_API_PREFIX}/:siteId/module`, (req, res, ctx) => {
    const { siteId } = req.params
    if (!siteId) {
      return res(ctx.status(404))
    }
    return res(ctx.json(response))
  }),
  rest.delete(`/${FLOORS_API_PREFIX}/:siteId/module`, (req, res, ctx) => {
    const { siteId } = req.params
    if (!siteId) {
      return res(ctx.status(404))
    }
    return res(ctx.status(204))
  }),
]

export default floorHandlers
