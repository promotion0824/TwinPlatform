import { rest } from 'msw'

export const diagnostics = [
  {
    id: '47dd193e-4a5b-4140-ba43-553a4d6f8e89',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-422',
    twinId: 'INV-60MP-ACT-L11-001',
    twinName: 'ACT-L11-001',
    type: 'alert',
    priority: 3,
    lastStatus: 'open',
    occurrenceCount: 1,
    ruleId: 'inspection-value-out-of-range-',
    ruleName: 'Inspection Value Out of Range',
    parentId: 'f5b29541-b0aa-4dca-baaf-a8c95c1cd9a9',
    check: false,
    occurrenceLiveData: {
      pointId: '47dd193e-4a5b-4140-ba43-553a4d6f8e89',
      pointEntityId: '47dd193e-4a5b-4140-ba43-553a4d6f8e89',
      pointName: 'ACT-L11-001 Num w',
      pointType: 'Binary',
      unit: 'Bool',
      timeSeriesData: [
        {
          start: '2023-10-12T13:30:00.000Z',
          end: '2023-10-12T14:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T14:30:00.000Z',
          end: '2023-10-12T15:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T15:30:00.000Z',
          end: '2023-10-12T16:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T16:30:00.000Z',
          end: '2023-10-12T17:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T17:30:00.000Z',
          end: '2023-10-12T18:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T18:30:00.000Z',
          end: '2023-10-12T19:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T19:30:00.000Z',
          end: '2023-10-12T20:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T20:30:00.000Z',
          end: '2023-10-12T21:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T21:30:00.000Z',
          end: '2023-10-12T22:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T22:30:00.000Z',
          end: '2023-10-12T23:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T23:30:00.000Z',
          end: '2023-10-13T00:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T00:30:00.000Z',
          end: '2023-10-13T01:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T01:30:00.000Z',
          end: '2023-10-13T02:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T02:30:00.000Z',
          end: '2023-10-13T03:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T03:30:00.000Z',
          end: '2023-10-13T04:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T04:30:00.000Z',
          end: '2023-10-13T05:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T05:30:00.000Z',
          end: '2023-10-13T06:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T06:30:00.000Z',
          end: '2023-10-13T07:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T07:30:00.000Z',
          end: '2023-10-13T08:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T08:30:00.000Z',
          end: '2023-10-13T09:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T09:30:00.000Z',
          end: '2023-10-13T10:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T10:30:00.000Z',
          end: '2023-10-13T11:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T11:30:00.000Z',
          end: '2023-10-13T12:30:00.000Z',
          isFaulty: true,
        },
      ],
    },
  },
  {
    id: '2efdb120-46ce-4968-ab28-90bcbc483d29',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-392',
    twinId: 'VAV-CN-L11-01',
    type: 'fault',
    priority: 0,
    lastStatus: 'open',
    primaryModelId: 'dtmi:com:willowinc:Asset;1',
    occurrenceCount: 1,
    ruleId: '',
    ruleName: '',
    parentId: 'f5b29541-b0aa-4dca-baaf-a8c95c1cd9a9',
    check: true,
    occurrenceLiveData: {
      pointId: '2efdb120-46ce-4968-ab28-90bcbc483d29',
      pointEntityId: '2efdb120-46ce-4968-ab28-90bcbc483d29',
      pointName: 'sy-test-4',
      pointType: 'Binary',
      unit: 'Bool',
      timeSeriesData: [
        {
          start: '2023-10-12T13:30:00.000Z',
          end: '2023-10-12T14:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T14:30:00.000Z',
          end: '2023-10-12T15:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T15:30:00.000Z',
          end: '2023-10-12T16:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T16:30:00.000Z',
          end: '2023-10-12T17:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T17:30:00.000Z',
          end: '2023-10-12T18:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T18:30:00.000Z',
          end: '2023-10-12T19:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T19:30:00.000Z',
          end: '2023-10-12T20:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T20:30:00.000Z',
          end: '2023-10-12T21:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T21:30:00.000Z',
          end: '2023-10-12T22:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T22:30:00.000Z',
          end: '2023-10-12T23:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-12T23:30:00.000Z',
          end: '2023-10-13T00:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T00:30:00.000Z',
          end: '2023-10-13T01:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T01:30:00.000Z',
          end: '2023-10-13T02:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T02:30:00.000Z',
          end: '2023-10-13T03:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T03:30:00.000Z',
          end: '2023-10-13T04:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T04:30:00.000Z',
          end: '2023-10-13T05:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T05:30:00.000Z',
          end: '2023-10-13T06:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T06:30:00.000Z',
          end: '2023-10-13T07:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T07:30:00.000Z',
          end: '2023-10-13T08:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T08:30:00.000Z',
          end: '2023-10-13T09:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T09:30:00.000Z',
          end: '2023-10-13T10:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T10:30:00.000Z',
          end: '2023-10-13T11:30:00.000Z',
          isFaulty: false,
        },
        {
          start: '2023-10-13T11:30:00.000Z',
          end: '2023-10-13T12:30:00.000Z',
          isFaulty: false,
        },
      ],
    },
  },
  {
    id: '430949a6-f67c-42d3-942b-d184f97f47e7',
    siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    sequenceNumber: '60MP-I-170',
    twinId: 'INV-60MP-VAV-CN-L11-01',
    twinName: 'VAV-CN-L11-01',
    type: 'note',
    priority: 3,
    lastStatus: 'open',
    primaryModelId: 'dtmi:com:willowinc:Asset;1',
    occurrenceCount: 1,
    parentId: '47dd193e-4a5b-4140-ba43-553a4d6f8e89',
    check: false,
    occurrenceLiveData: {
      pointId: '430949a6-f67c-42d3-942b-d184f97f47e7',
      pointEntityId: '430949a6-f67c-42d3-942b-d184f97f47e7',
      pointName: 'VAV-CN-L11-01 test w',
      pointType: 'Binary',
      unit: 'Bool',
      timeSeriesData: [
        {
          start: '2023-10-12T13:30:00.000Z',
          end: '2023-10-12T14:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T14:30:00.000Z',
          end: '2023-10-12T15:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T15:30:00.000Z',
          end: '2023-10-12T16:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T16:30:00.000Z',
          end: '2023-10-12T17:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T17:30:00.000Z',
          end: '2023-10-12T18:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T18:30:00.000Z',
          end: '2023-10-12T19:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T19:30:00.000Z',
          end: '2023-10-12T20:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T20:30:00.000Z',
          end: '2023-10-12T21:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T21:30:00.000Z',
          end: '2023-10-12T22:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T22:30:00.000Z',
          end: '2023-10-12T23:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-12T23:30:00.000Z',
          end: '2023-10-13T00:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T00:30:00.000Z',
          end: '2023-10-13T01:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T01:30:00.000Z',
          end: '2023-10-13T02:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T02:30:00.000Z',
          end: '2023-10-13T03:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T03:30:00.000Z',
          end: '2023-10-13T04:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T04:30:00.000Z',
          end: '2023-10-13T05:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T05:30:00.000Z',
          end: '2023-10-13T06:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T06:30:00.000Z',
          end: '2023-10-13T07:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T07:30:00.000Z',
          end: '2023-10-13T08:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T08:30:00.000Z',
          end: '2023-10-13T09:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T09:30:00.000Z',
          end: '2023-10-13T10:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T10:30:00.000Z',
          end: '2023-10-13T11:30:00.000Z',
          isFaulty: true,
        },
        {
          start: '2023-10-13T11:30:00.000Z',
          end: '2023-10-13T12:30:00.000Z',
          isFaulty: true,
        },
      ],
    },
  },
]

// http://localhost:8080/au/api/insights/f5b29541-b0aa-4dca-baaf-a8c95c1cd9a9/occurrences/diagnostics?start=2023-10-12T13%3A30%3A00.000Z&end=2023-10-13T12%3A30%3A00.000Z&interval=00.01%3A00%3A00

export const handlers = [
  rest.get(
    '/:region/api/insights/:insightId/occurrences/diagnostics',
    (_req, res, ctx) => res(ctx.delay(0), ctx.json(diagnostics))
  ),
]
