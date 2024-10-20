import { rest } from 'msw'
import sites from './sites'

export const handlers = [
  rest.post('/:region/api/kpi/building_data', (req, res, ctx) =>
    res(ctx.json(data))
  ),
]

const data = [
  {
    name: 'OperationsScore_LastValue',
    values: [
      {
        xValue: sites[0].name,
      },
      {
        xValue: sites[1].name,
        yValue: 0.39,
      },
    ],
    yuom: '%',
  },
]
