/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

export function makeHandlers() {
  return [
    // Note that the real server endpoint supports multiple twinIds but we
    // currently only support one.
    rest.get('/:region/api/sites/:siteId/equipments/:id', (req, res, ctx) => {
      const { siteId, id } = req.params
      return res(
        ctx.json({
          id,
          name: 'A1-GND-00123',
          customerId: '00000000-0000-0000-0000-000000000000',
          siteId,
          points: [],
          tags: [],
          pointTags: [],
        })
      )
    }),
  ]
}

export const handlers = makeHandlers()
