import { rest } from 'msw'
import { v4 as uuidv4 } from 'uuid'

const makeInspectionCheck = (inspectionId, checkName) => {
  const checkId = uuidv4()

  return {
    id: checkId,
    inspectionId,
    name: checkName,
    type: 'Total',
    typeValue: '1',
    decimalPlaces: 1,
    isArchived: false,
    isPaused: false,
    canGenerateInsight: true,
    statistics: {
      checkRecordCount: 0,
      lastCheckSubmittedEntry: '',
      workableCheckStatus: 'overdue',
      nextCheckRecordDueTime: '2022-09-19T00:00:00.000Z',
    },
  }
}

const makeInspection = (siteId, inspectionName, numChecks = 1) => {
  const inspectionId = uuidv4()
  return {
    id: inspectionId,
    name: inspectionName,
    zoneId: '7666449e-6389-4541-9a24-8499c3fb78f3',
    floorCode: 'L5',
    siteId,
    assetId: '00600000-0000-0000-0000-000000740353',
    assignedWorkgroupId: 'efe565cb-a56f-4af5-876a-56e61f071e42',
    frequency: 8,
    unit: 'hours',
    startDate: '2022-01-14T10:15:00',
    sortOrder: 0,
    checks: numChecks
      ? Array(numChecks)
          .fill()
          .map((_, index) =>
            makeInspectionCheck(inspectionId, `Check ${index}`)
          )
      : [],
    checkRecordCount: 0,
    workableCheckCount: 2,
    completedCheckCount: 0,
    nextCheckRecordDueTime: '2022-09-19T00:00:00.000Z',
    assignedWorkgroupName: 'Test Group - DO NOT DELETE (Automation)',
    zoneName: '20220114',
    assetName: 'DBH-L05-01',
    checkRecordSummaryStatus: 'overdue',
  }
}

const siteInspectionMap = {
  '404bd33c-a697-4027-b6a6-677e30a53d07': [
    makeInspection(
      '404bd33c-a697-4027-b6a6-677e30a53d07',
      'House Distribution Board ABCDE',
      1
    ),
    makeInspection(
      '404bd33c-a697-4027-b6a6-677e30a53d07',
      'Air Terminals XYZ',
      2
    ),
  ],
  '926d1b17-05f7-47bb-b57b-75a922e69a20': [
    makeInspection(
      '926d1b17-05f7-47bb-b57b-75a922e69a20',
      'Automation Inspection 1234',
      3
    ),
  ],
}

export const handlers = [
  rest.get('/:region/api/inspections', (req, res, ctx) =>
    res(
      ctx.json([
        ...siteInspectionMap['404bd33c-a697-4027-b6a6-677e30a53d07'],
        ...siteInspectionMap['926d1b17-05f7-47bb-b57b-75a922e69a20'],
      ])
    )
  ),
  rest.get('/:region/api/sites/:siteId/inspections', (req, res, ctx) =>
    res(ctx.json(siteInspectionMap[req.params.siteId] || []))
  ),
  rest.get(
    '/:region/api/sites/:siteId/inspections/:inspectionId',
    (req, res, ctx) =>
      res(
        ctx.json(
          siteInspectionMap[req.params.siteId].find(
            (inspection) => inspection.id === req.params.inspectionId
          )
        )
      )
  ),
  rest.get(
    '/:region/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/history',
    (req, res, ctx) => res(ctx.json([]))
  ),
]
