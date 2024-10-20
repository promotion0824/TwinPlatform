/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

const skills = [
  {
    id: '1',
    name: 'energy',
    category: 'energy',
  },
]

export const handlers = [
  rest.post('/:region/api/skills', (req, res, ctx) =>
    res(ctx.delay(0), ctx.json(skills))
  ),
]
