import { rest } from 'msw'
import { v4 as uuidv4 } from 'uuid'

const cachedChecks = {}

const makeCheckRecord = (inspectionId, checkId) => {
  const checkType = cachedChecks[checkId].type
  return {
    id: uuidv4(),
    inspectionId,
    checkId,
    status: 'completed',
    submittedUserId: '26936cf4-c44a-4cb0-b7b1-5e39b375cec1',
    submittedDate: '2022-11-18T05:29:00.823Z',
    submittedSiteLocalDate: '2022-11-18T16:29:00.823Z',
    enteredBy: 'Investa-AU SiteAdmin',
    numberValue:
      checkType === 'numeric' || checkType === 'total' ? 2 : undefined,
    stringValue: checkType === 'list' ? 'a' : undefined,
    dateValue: checkType === 'date' ? '2022-11-16T21:00:00.000Z' : undefined,
    effectiveDate: '2022-11-16T21:00:00.000Z',
    notes: 'check 1',
    attachments: [
      {
        id: '4e81b897-d7f6-4666-82eb-7bfaf325ab0a',
        type: 'image',
        fileName: 'book.png',
        createdDate: '2022-11-18T05:29:00.594Z',
        previewUrl:
          '/au/api/images/2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b/sites/404bd33c-a697-4027-b6a6-677e30a53d07/checkRecords/831ff746-3c7c-452d-bf9b-64e10c20efc3/4e81b897-d7f6-4666-82eb-7bfaf325ab0a_1_w100_h100.jpg',
        url: '/au/api/images/2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b/sites/404bd33c-a697-4027-b6a6-677e30a53d07/checkRecords/831ff746-3c7c-452d-bf9b-64e10c20efc3/4e81b897-d7f6-4666-82eb-7bfaf325ab0a_0.jpg',
      },
    ],
  }
}

const makeCheck = (inspectionId, type, name, sortOrder, typeValue = '') => {
  const checkId = uuidv4()
  const check = {
    id: checkId,
    inspectionId,
    sortOrder,
    name,
    type,
    typeValue,
    decimalPlaces: 0,
    lastSubmittedRecord: {
      id: 'd2e6edf9-6db4-4fd8-b61e-2449058492de',
      inspectionId,
      checkId,
      inspectionRecordId: '75fe3f74-babf-43b4-a042-1dc41cf16026',
      status: 'completed',
      submittedUserId: '380d1cfc-61c3-4439-a829-4ae6ba259f2f',
      submittedDate: '2022-11-21T05:22:34.980Z',
      submittedSiteLocalDate: '2022-11-21T16:22:34.980Z',
      dateValue: '2022-11-23T16:00:00.000Z',
      notes: '',
      effectiveDate: '2022-11-21T05:00:00.000Z',
    },
  }
  cachedChecks[checkId] = check

  return cachedChecks[checkId]
}

/**
 * Make the inspection, checks and check records provided by the lastRecord
 * end point.
 */
const makeInspectionLastRecord = (inspectionId) => {
  const dateCheck = makeCheck(inspectionId, 'date', 'Date check', 2)
  const numericCheck = makeCheck(
    inspectionId,
    'numeric',
    'Numeric check',
    1,
    '100'
  )
  const totalCheck = makeCheck(inspectionId, 'total', 'Total check', 4, '30')
  const listCheck = makeCheck(inspectionId, 'list', 'List check', 9, 'a|b|c|d')
  const dependencyCheck1 = {
    ...makeCheck(inspectionId, 'list', 'Dependency list check', 5, 'e|f|g|h'),
    dependencyId: listCheck.id,
    dependencyValue: 'a',
  }
  const dependencyCheck2 = {
    ...makeCheck(inspectionId, 'date', 'Dependency date check', 6),
    dependencyId: dateCheck.id,
  }
  const inspectionRecordId = 'e08f8591-9e5b-4cfa-909b-beb660be8530'
  const checks = [
    dateCheck,
    numericCheck,
    totalCheck,
    listCheck,
    dependencyCheck1,
    dependencyCheck2,
  ]

  return {
    id: inspectionRecordId,
    inspectionId,
    inspection: {
      id: inspectionId,
      name: 'S-CN-L01-102',
      zoneId: '29558aea-c2e0-494b-b02f-7c1152f4c068',
      assetId: '00600000-0000-0000-0000-000000840474',
      floorCode: 'L1',
      assignedWorkgroupId: '24ab188e-3419-4b9e-adcd-308e2a9809f4',
      sortOrder: 0,
      frequencyInHours: 8,
      startDate: '2022-11-01T00:00:00.000Z',
      endDate: '2023-04-30T00:00:00.000Z',
      nextEffectiveDate: '0001-01-01T00:00:00.000Z',
      checks,
    },
    checkRecords: checks.map((check) => ({
      id: uuidv4(),
      inspectionId,
      checkId: check.id,
      inspectionRecordId,
      status: 'overdue',
      effectiveDate: '2022-12-13T05:00:00.000Z',
      attachments: [],
    })),
  }
}

export const handlers = [
  rest.get(
    '/:region/api/sites/:siteId/inspections/:inspectionId/lastRecord',
    (req, res, ctx) =>
      res(ctx.json(makeInspectionLastRecord(req.params.inspectionId)))
  ),
  rest.get(
    '/:region/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
    (req, res, ctx) =>
      res(
        ctx.json(
          new Array(5)
            .fill()
            .map(() =>
              makeCheckRecord(req.params.inspectionId, req.params.checkId)
            )
        )
      )
  ),
]
