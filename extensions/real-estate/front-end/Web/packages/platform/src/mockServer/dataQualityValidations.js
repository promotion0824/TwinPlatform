import { rest } from 'msw'

export const handlers = [
  rest.get(
    '/:region/api/sites/:siteId/twins/:twinId/dataquality/validations',
    (req, res, ctx) => res(ctx.json(dataQualityValidations))
  ),
]

const dataQualityValidations = {
  missingProperties: ['geometrySpatialReference'],
  missingSensors: ['Test 1', 'Test2'],
}
