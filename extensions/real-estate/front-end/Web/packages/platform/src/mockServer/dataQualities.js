import { rest } from 'msw'
import sites from './sites'
import { dataQualityTestModelIdPrefix } from './twins'

export const handlers = [
  rest.get(
    '/:region/api/sites/:siteId/twins/:twinId/dataquality',
    (req, res, ctx) => res(ctx.json(twinDataQualities))
  ),
  rest.get(
    '/:region/api/customers/:customerId/portfolios/:portfolioId/sites/dataquality',
    (req, res, ctx) => res(ctx.json(sitesDataQualities))
  ),
  rest.get('/:region/api/sites/:siteId/floors/dataquality', (req, res, ctx) => {
    switch (req.url.searchParams.get('systemId')) {
      case `${dataQualityTestModelIdPrefix}0`:
        return res(ctx.json(floorsDataQualitiesForHVAC0))
      case `${dataQualityTestModelIdPrefix}1`:
        return res(ctx.json(floorsDataQualitiesForHVAC1))
      default:
        return res(ctx.json([]))
    }
  }),
]

const twinDataQualities = {
  attributePropertiesScore: 56,
  sensorsDefinedScore: 32,
  staticScore: 82,
  sensorsReadingDataScore: 48,
  connectivityScore: 68,
  overallScore: 24,
}

const sitesDataQualities = [
  {
    locationId: sites[0].id,
    dataQuality: {
      staticScore: 70,
      connectivityScore: 90,
      overallScore: 80,
    },
  },
]

const floorsDataQualitiesForHVAC0 = [
  {
    locationId: '10b63e33-2279-4eb5-a25b-b2b4a5dcde98', // L5 @ 60 Martin Street
    dataQuality: {
      staticScore: 20,
      connectivityScore: 70,
      overallScore: 45,
    },
  },
  {
    locationId: '56750a81-6329-434f-90bf-312402a80485', // B3 @ 60 Martin Street
    dataQuality: {
      staticScore: 30,
      connectivityScore: 50,
      overallScore: 40,
    },
  },
  {
    locationId: '8483a1f1-3c6a-4d64-9ddc-407848aa624a', // L10 @ 60 Martin Street
    dataQuality: {
      staticScore: 80,
      connectivityScore: 100,
      overallScore: 90,
    },
  },
]

const floorsDataQualitiesForHVAC1 = [
  {
    locationId: '10b63e33-2279-4eb5-a25b-b2b4a5dcde98', // L5 @ 60 Martin Street
    dataQuality: {
      staticScore: 80,
      connectivityScore: 100,
      overallScore: 90,
    },
  },
  {
    locationId: '56750a81-6329-434f-90bf-312402a80485', // B3 @ 60 Martin Street
    dataQuality: {
      staticScore: 50,
      connectivityScore: 70,
      overallScore: 60,
    },
  },
  {
    locationId: '8483a1f1-3c6a-4d64-9ddc-407848aa624a', // L10 @ 60 Martin Street
    dataQuality: {
      staticScore: 30,
      connectivityScore: 50,
      overallScore: 40,
    },
  },
]
