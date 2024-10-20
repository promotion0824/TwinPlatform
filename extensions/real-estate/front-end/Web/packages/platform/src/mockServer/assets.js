import { rest } from 'msw'

let isLevel66OnFire = true

function makeAsset(n) {
  return {
    id: n.toString(),
    equipmentId: n.toString(),
    name: `Asset ${n}`,
    identifier: `Asset ${n}`,
    hasLiveData: false,
    tags: [
      'centralPlaza',
      'elevator',
      'equip',
      'leak',
      'verticalTransport',
      'water',
    ],
    pointTags: [{ name: 'alarm' }, { name: 'leak' }, { name: 'sensor' }],
    isEquipmentOnly: true,
    properties: [
      {
        displayName: 'Comments',
        value: 'Central Plaza',
      },
      {
        displayName: 'External ID',
        value: 'EL-SE_06',
      },
      {
        displayName: 'Site ID',
        value: '4e5fc229-ffd9-462a-882b-16b4a63b2a8a',
      },
    ],
  }
}

export const handlers = [
  rest.get('/:region/api/sites/:siteId/assets/categories', (req, res, ctx) =>
    res(ctx.json(categoryTree))
  ),

  rest.get('/:region/api/sites/:siteId/assets', (req, res, ctx) => {
    const pageSize = parseInt(req.url.searchParams.get('pageSize') || 500, 10)
    const pageNumber = parseInt(req.url.searchParams.get('pageNumber') || 0, 10)
    const floorCode = req.url.searchParams.get('floorCode')
    const categoryId = req.url.searchParams.get('categoryId')
    const assets = []

    // Let's just say we have three full pages and one half page of results.
    const numItemsToReturn =
      pageNumber === 3 ? Math.floor(pageSize / 2) : pageSize

    for (let i = 0; i < numItemsToReturn; i++) {
      const parts = [pageNumber * pageSize + i]
      if (floorCode != null) {
        parts.push(floorCode)
      }
      if (categoryId != null) {
        parts.push(categoryId.substring(0, 8))
      }
      assets.push(makeAsset(parts.join(' ')))
    }

    function successResponse() {
      return res(ctx.delay(50), ctx.json(assets))
    }

    function failResponse() {
      return res(
        ctx.delay(50),
        ctx.status(500),
        ctx.json({
          error: 'Level 66 has been removed',
        })
      )
    }

    if (floorCode === 'L66') {
      if (isLevel66OnFire) {
        isLevel66OnFire = false
        return failResponse()
      }
      isLevel66OnFire = true
      return successResponse()
    }
    return successResponse()
  }),

  rest.get('/:region/api/sites/:siteId/assets/:uniqueId', (req, res, ctx) => {
    const { uniqueId } = req.params
    return res(ctx.json(makeAsset(uniqueId)))
  }),
]
