/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

export const handlers = [
  rest.get('/:region/api/me/workgroups', (req, res, ctx) =>
    res(
      ctx.json([
        { id: '43ca4ba1-6757-4232-a2da-51fd8939ac2a', name: 'workgroup-1' },
      ])
    )
  ),
]
