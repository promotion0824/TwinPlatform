import _ from 'lodash'
import { rest } from 'msw'
import { v4 as uuidv4 } from 'uuid'
import { csvify, wrapHandlerWithFallback } from './utils'
import sites from './sites'

export function makeTwinEtag() {
  // Don't know why but this is just what a digital twin etag looks like.
  return `W/"${uuidv4()}"`
}

const defaultModelId = 'dtmi:com:willowinc:BACnetController;1'
const hvacSystemId = 'dtmi:com:willowinc:HVACSupplyAirSystem;1'
export const dataQualityTestModelIdPrefix = 'INV-60MP-SA'
export const dataQualityTestModelNamePrefix = 'HVAC-'
const passengerBoardingBridgesModelId =
  'dtmi:com:willowinc:airport:PassengerBoardingBridge;1'

// Hidden and read only fields from
// https://willow.atlassian.net/wiki/spaces/MAR/pages/2054389868/Twin+Properties+Blacklist

const hiddenFields = [
  '/registrationID',
  '/registrationKey',
  '/lastValue',
  '/lastValueTime',
]

const readOnlyFields = [
  '/accessEventType',
  '/categorizationProperties',
  '/CO2',
  '/commissionedbyRef',
  '/communication',
  '/connectorID',
  '/customProperties',
  '/customTags',
  '/detected',
  '/humidity',
  '/id',
  '/installedbyRef',
  '/lastValue',
  '/lastValueTime',
  '/manufacturedbyRef',
  '/occupancy',
  '/registrationID',
  '/registrationKey',
  '/scaleFactor',
  '/serviceProviderRef',
  '/serviceResponsibilityRef',
  '/siteID',
  '/temperature',
  '/trendID',
  '/trendInterval',
  '/uniqueID',
]

function makeRelationships(twinId) {
  return [
    {
      source: {
        id: twinId,
        metadata: {
          modelId: 'dtmi:com:willowinc:Building;1',
        },
        siteID: '404bd33c-a697-4027-b6a6-677e30a53d07',
        name: 'Your twin',
      },
      target: {
        id: 'RELATIONSHIP-1',
        metadata: {
          modelId: 'dtmi:com:willowinc:Building;1',
        },
        siteID: '404bd33c-a697-4027-b6a6-677e30a53d07',
        name: 'Relationship 1',
      },
    },
  ]
}

function makeTwin(props) {
  return {
    etag: makeTwinEtag(),
    geometrySpatialReference: 'some value',
    modelNumber: 'CRAC-31-1',
    serialNumber: 'Y18D6S0',
    connectorID: "bet you can't edit this",
    registrationID: '',
    ownedByRef: {
      targetId: '',
      name: '',
      targetModelId: '',
    },
    metadata: {
      modelId: defaultModelId,
    },
    ...props,
  }
}

export function makeHandlers(twins) {
  return [
    rest.get('/:region/api/twins/search', (req, res, ctx) => {
      const items = Object.values(twins)

      // for testing with Digital Commissioning in Data Quality tab where
      // site view is selected
      if (req.url.searchParams.get('modelId') === hvacSystemId) {
        return res(
          ctx.json({
            twins: items.map((item, index) => ({
              siteId: item.siteID,
              id: `${dataQualityTestModelIdPrefix}${index}`,
              name: `${dataQualityTestModelNamePrefix}${index}`,
            })),
            queryId: '121query121',
          })
        )
      }

      if (
        req.url.searchParams.get('modelId') ===
          passengerBoardingBridgesModelId &&
        !req.url.searchParams.get('Page')
      ) {
        return res(
          ctx.json({
            nextPage:
              '/twins/search?ModelId=dtmi%3Acom%3Awillowinc%3Aairport%3APassengerBoardingBridge;1&QueryId=9133389ef6f84cce90ab25ec70e17ba8&Page=1',
            twins: dfwPOCtwins.slice(0, 100),
            queryId: '9133389ef6f84cce90ab25ec70e17ba8',
          })
        )
      }

      if (
        req.url.searchParams.get('ModelId') ===
          passengerBoardingBridgesModelId &&
        req.url.searchParams.get('Page') === '1'
      ) {
        return res(
          ctx.json({
            twins: dfwPOCtwins.slice(100),
            queryId: '4218a484659143d7b31aac9eb38f54c5',
          })
        )
      }

      return res(
        ctx.json({
          twins: items.map((item) => ({
            // PortalXL returns single twins with siteID, but in the lists of
            // twins we instead have siteId.
            ..._.omit(item, 'siteID'),
            siteId: item.siteID,
            modelId: defaultModelId,
            inRelationships: [],
            outRelationships: [],
          })),
          queryId: '123query123',
        })
      )
    }),

    rest.get(
      '/:region/api/twins/:twinId/relationships',
      async (req, res, ctx) => {
        const { twinId } = req.params
        return res(ctx.json(makeRelationships(twinId)))
      }
    ),
    rest.get(
      '/:region/api/sites/:siteId/twins/:twinId/relationships',
      async (req, res, ctx) => {
        const { twinId } = req.params
        return res(ctx.json(makeRelationships(twinId)))
      }
    ),

    rest.get('/:region/api/v2/twins/:twinId', async (req, res, ctx) => {
      const { twinId } = req.params

      if (twinId === foodDisplayCaseTwinId) {
        return res(ctx.json(foodDisplayCaseTwin))
      }

      const twin = twins[twinId]
      if (twin != null) {
        if (twin.type === 'error') {
          return res(ctx.status(twin.statusCode))
        }

        return res(
          ctx.json({
            twin: {
              id: twinId,
              ..._.omit(twin, ['statusCode', 'permissions']),
            },
            permissions: twin.permissions ?? {
              edit: true,
            },
          })
        )
      }
    }),

    rest.get(
      '/:region/api/v2/sites/:siteId/twins/:twinId',
      async (req, res, ctx) => {
        const { twinId } = req.params

        if (twinId === foodDisplayCaseTwinId) {
          return res(ctx.json(foodDisplayCaseTwin))
        }

        const twin = twins[twinId]
        if (twin != null) {
          if (twin.type === 'error') {
            return res(ctx.status(twin.statusCode))
          }

          return res(
            ctx.json({
              twin: {
                id: twinId,
                ..._.omit(twin, ['statusCode', 'permissions']),
              },
              permissions: twin.permissions ?? {
                edit: true,
              },
            })
          )
        }
      }
    ),

    rest.get(
      '/:region/api/sites/:siteId/twins/restrictedFields',
      (req, res, ctx) => {
        return res(
          ctx.json({
            hiddenFields,
            readOnlyFields,
          })
        )
      }
    ),

    rest.get(
      '/:region/api/sites/:siteId/twins/:twinId/points',
      (req, res, ctx) =>
        res(
          ctx.json([
            {
              name: 'VFD Inverter Temp Sensor',
              externalId: '-FACILITY-MANWEST-DFSDF',
              properties: {
                connectorID: {
                  value: 'c1',
                },
              },
              connectorName: 'CUS-CENTRALTOWER-BMS-BACNET',
              device: {
                id: 'd1',
                name: 'BACnet Device 13257',
              },
            },
            {
              name: 'Variable Weight',
              externalId: '-FACILITY-MANWEST-DFSDF',
              properties: {
                connectorID: {
                  value: 'c2',
                },
              },
              connectorName: 'CUS-CENTRALTOWER-VIBRATION',
            },
          ])
        )
    ),

    /**
     * Note: we only support `add` and `replace` JSON patch operations, and
     * we do not differentiate between add and replace (so both will work
     * regardless of whether the property already existed).
     *
     * We also only support adding / replacing at top level (so eg. if
     * path is "/prop/subprop" we will create a top-level prop with the key
     * "prop/subprop"), not a key "subprop" inside a key "prop".
     *
     * The real backend has proper support for JSON patch.
     */
    rest.patch('/:region/api/sites/:siteId/twins/:twinId', (req, res, ctx) => {
      const { twinId } = req.params
      const existingTwin = twins[twinId]

      if (existingTwin != null) {
        if (Object.values(req.body).includes('server-error')) {
          return res(ctx.status(500))
        }

        if (req.headers.get('If-Match') !== existingTwin.etag) {
          return res(
            ctx.status(412),
            ctx.json({
              message: 'The etag is out of date',
              statusCode: 412,
            })
          )
        }

        const fields = req.body.map((op) => op.path.substr(1))

        // Reject the request if we sent any hidden or read-only fields.
        const illegalFields = readOnlyFields.filter((f) => fields.includes(f))

        if (illegalFields.length > 0) {
          return res(
            ctx.status(403),
            ctx.json({
              message: `The following fields may not be edited: ${illegalFields.join(
                ', '
              )}`,
              statusCode: 403,
            })
          )
        }

        const originalTwin = _.cloneDeep(twins[twinId])
        for (const op of req.body) {
          twins[twinId][op.path.substr(1)] = op.value
        }

        if (!_.isEqual(twins[twinId], originalTwin)) {
          // The server only updates the etag if something was actually changed, so
          // we do the same.
          twins[twinId].etag = makeTwinEtag()
        }

        return res(ctx.json(_.pick(twins[twinId], ['id', 'metadata'])))
      }
    }),

    rest.post('/:region/api/twins/export', (req, res, ctx) => {
      let { twins } = req.body

      if (twins == null) {
        twins = [
          {
            siteId: sites[0].id,
            twinId: '123123',
          },
          {
            siteId: sites[1].id,
            twinId: '124124',
          },
        ]
      }

      return res(
        ctx.set('Content-Type', 'text/csv'),
        ctx.set(
          'Content-Disposition',
          'attachment; filename="twins-export.csv"'
        ),
        ctx.text(
          csvify([
            ['Site ID', 'Twin ID', 'Other stuff'],
            ...twins.map((twin) => [twin.siteId, twin.twinId, 'other stuff']),
          ])
        )
      )
    }),
  ]
}

const mockTwins = {
  123123: makeTwin({
    id: '123123',
    siteID: sites[0].id,
    uniqueID: '123123-u',
    name: 'Twin 1',
    floorName: 'Level 1',
  }),
  124124: makeTwin({
    id: '124124',
    siteID: sites[1].id,
    uniqueID: '124124-u',
    name: 'Twin 2',
    floorName: 'Level 2',
  }),
  404404: makeTwin({
    id: '404404',
    siteID: sites[0].id,
    name: '404 twin',
    type: 'error',
    statusCode: 404,
  }),
  500500: makeTwin({
    id: '500500',
    siteID: sites[0].id,
    name: '500 twin',
    type: 'error',
    statusCode: 500,
  }),
}

export const handlers = makeHandlers(mockTwins).map(wrapHandlerWithFallback)

/**
 * DFW POC:
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/90924
 */
const dfwPOCtwins = [
  {
    id: 'DFW-A10-PBB',
    name: 'PBB A10',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '571f721a-edcf-4a4b-a2a4-094bd2de6c7f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A10-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:22.7477410Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"571f721a-edcf-4a4b-a2a4-094bd2de6c7f","name":"PBB A10","description":"Passenger Boarding Bridge at Gate A10","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.907325,"longitude":-97.037938}}}',
  },
  {
    id: 'DFW-A11-PBB',
    name: 'PBB A11',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '153b59da-b023-450b-b412-42dd30527e5f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A11-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:28.1554763Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"153b59da-b023-450b-b412-42dd30527e5f","name":"PBB A11","description":"Passenger Boarding Bridge at Gate A11","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.907306,"longitude":-97.037573}}}',
  },
  {
    id: 'DFW-A13-PBB',
    name: 'PBB A13',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7de73e22-b9c6-421f-9ec4-b546bf78df41',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A13-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:28.4064471Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7de73e22-b9c6-421f-9ec4-b546bf78df41","name":"PBB A13","description":"Passenger Boarding Bridge at Gate A13","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.907408,"longitude":-97.036986}}}',
  },
  {
    id: 'DFW-A14-PBB',
    name: 'PBB A14',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a6078169-dc8d-4bb3-ac46-c35c090ca2c3',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A14-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:28.8205420Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a6078169-dc8d-4bb3-ac46-c35c090ca2c3","name":"PBB A14","description":"Passenger Boarding Bridge at Gate A14","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.90723,"longitude":-97.036535}}}',
  },
  {
    id: 'DFW-A15-PBB',
    name: 'PBB A15',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'cd27fc58-dc53-4626-a988-5d2aae320be7',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A15-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:28.9146340Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"cd27fc58-dc53-4626-a988-5d2aae320be7","name":"PBB A15","description":"Passenger Boarding Bridge at Gate A15","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.906987,"longitude":-97.036248}}}',
  },
  {
    id: 'DFW-A16-PBB',
    name: 'PBB A16',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '01e0098f-ea95-45aa-bd04-41831a04a727',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A16-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.0238517Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"01e0098f-ea95-45aa-bd04-41831a04a727","name":"PBB A16","description":"Passenger Boarding Bridge at Gate A16","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.906644,"longitude":-97.03594}}}',
  },
  {
    id: 'DFW-A17-PBB',
    name: 'PBB A17',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7dd05511-769b-4e70-ae97-a730de2d5a66',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A17-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.2290558Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7dd05511-769b-4e70-ae97-a730de2d5a66","name":"PBB A17","description":"Passenger Boarding Bridge at Gate A17","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.906372,"longitude":-97.03564}}}',
  },
  {
    id: 'DFW-A18-PBB',
    name: 'PBB A18',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '2f1266eb-1107-49c4-a6b7-6b13c64d354a',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A18-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.3949227Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"2f1266eb-1107-49c4-a6b7-6b13c64d354a","name":"PBB A18","description":"Passenger Boarding Bridge at Gate A18","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.906131,"longitude":-97.035497}}}',
  },
  {
    id: 'DFW-A19-PBB',
    name: 'PBB A19',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a0b7ebe2-4481-4199-b82d-82c01c76dcf7',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A19-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.5356283Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a0b7ebe2-4481-4199-b82d-82c01c76dcf7","name":"PBB A19","description":"Passenger Boarding Bridge at Gate A19","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.905791,"longitude":-97.03535}}}',
  },
  {
    id: 'DFW-A20-PBB',
    name: 'PBB A20',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6e695a98-f5cf-4c45-bb55-5db00bd60511',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A20-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.5928399Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6e695a98-f5cf-4c45-bb55-5db00bd60511","name":"PBB A20","description":"Passenger Boarding Bridge at Gate A20","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.905476,"longitude":-97.035194}}}',
  },
  {
    id: 'DFW-A21-PBB',
    name: 'PBB A21',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '8feb132c-4637-45f3-8a1c-b5edf7fe5671',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A21-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.6790564Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"8feb132c-4637-45f3-8a1c-b5edf7fe5671","name":"PBB A21","description":"Passenger Boarding Bridge at Gate A21","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.905021,"longitude":-97.035138}}}',
  },
  {
    id: 'DFW-A22-PBB',
    name: 'PBB A22',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '4ae16fa9-fe80-47e9-b571-ee2187b83171',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A22-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.7287225Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"4ae16fa9-fe80-47e9-b571-ee2187b83171","name":"PBB A22","description":"Passenger Boarding Bridge at Gate A22","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.904602,"longitude":-97.035151}}}',
  },
  {
    id: 'DFW-A23-PBB',
    name: 'PBB A23',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'd0cec4bd-18b2-4ea9-b3be-f67504b8b02f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A23-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.8211410Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"d0cec4bd-18b2-4ea9-b3be-f67504b8b02f","name":"PBB A23","description":"Passenger Boarding Bridge at Gate A23","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.904276,"longitude":-97.035099}}}',
  },
  {
    id: 'DFW-A24-PBB',
    name: 'PBB A24',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'd7b09ebb-34a2-4d55-a858-3dbab942cc2c',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A24-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.8953173Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"d7b09ebb-34a2-4d55-a858-3dbab942cc2c","name":"PBB A24","description":"Passenger Boarding Bridge at Gate A24","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.903875,"longitude":-97.035198}}}',
  },
  {
    id: 'DFW-A25-PBB',
    name: 'PBB A25',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '777fe3b8-08c5-4d7a-9db0-ff1917a0b6ae',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A25-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:29.9614363Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"777fe3b8-08c5-4d7a-9db0-ff1917a0b6ae","name":"PBB A25","description":"Passenger Boarding Bridge at Gate A25","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.903386,"longitude":-97.035547}}}',
  },
  {
    id: 'DFW-A28-PBB',
    name: 'PBB A28',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'df582a35-6c69-4230-b85e-37f726a6228d',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A28-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.0219358Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"df582a35-6c69-4230-b85e-37f726a6228d","name":"PBB A28","description":"Passenger Boarding Bridge at Gate A28","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.902993,"longitude":-97.035731}}}',
  },
  {
    id: 'DFW-A29-PBB',
    name: 'PBB A29',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '28476859-ee0b-4c4f-95ae-72bff61fdf2d',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A29-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.1208262Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"28476859-ee0b-4c4f-95ae-72bff61fdf2d","name":"PBB A29","description":"Passenger Boarding Bridge at Gate A29","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.90266,"longitude":-97.035898}}}',
  },
  {
    id: 'DFW-A33-PBB',
    name: 'PBB A33',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'fa4c1ccf-49fe-44f9-a0e9-ea8b6851e064',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A33-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.2063482Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"fa4c1ccf-49fe-44f9-a0e9-ea8b6851e064","name":"PBB A33","description":"Passenger Boarding Bridge at Gate A33","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.902334,"longitude":-97.036061}}}',
  },
  {
    id: 'DFW-A34-PBB',
    name: 'PBB A34',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a8ebe9e4-b6d7-40fa-ac31-f95a139d3a52',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A34-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.2663611Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a8ebe9e4-b6d7-40fa-ac31-f95a139d3a52","name":"PBB A34","description":"Passenger Boarding Bridge at Gate A34","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.902068,"longitude":-97.036545}}}',
  },
  {
    id: 'DFW-A35-PBB',
    name: 'PBB A35',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e626d27d-a6c7-4ac3-b73f-bf5f9a702c13',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A35-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.3213203Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e626d27d-a6c7-4ac3-b73f-bf5f9a702c13","name":"PBB A35","description":"Passenger Boarding Bridge at Gate A35","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.901835,"longitude":-97.036966}}}',
  },
  {
    id: 'DFW-A36-PBB',
    name: 'PBB A36',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b1dbaa39-9ed0-45f0-b1f8-fe03d0da417e',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A36-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.4088998Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b1dbaa39-9ed0-45f0-b1f8-fe03d0da417e","name":"PBB A36","description":"Passenger Boarding Bridge at Gate A36","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.901891,"longitude":-97.037406}}}',
  },
  {
    id: 'DFW-A37-PBB',
    name: 'PBB A37',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'd52f780d-5636-4605-ba22-0340f4187d78',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A37-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.4768043Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"d52f780d-5636-4605-ba22-0340f4187d78","name":"PBB A37","description":"Passenger Boarding Bridge at Gate A37","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.901902,"longitude":-97.037941}}}',
  },
  {
    id: 'DFW-A38-PBB',
    name: 'PBB A38',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3e9b83eb-dff6-45f0-aa6d-920a00fc8fa7',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A38-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.5278714Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3e9b83eb-dff6-45f0-aa6d-920a00fc8fa7","name":"PBB A38","description":"Passenger Boarding Bridge at Gate A38","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.901824,"longitude":-97.038324}}}',
  },
  {
    id: 'DFW-A39-PBB',
    name: 'PBB A39',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '98a32b77-8f2c-4482-a7c9-4756b9be1839',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A39-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.5955907Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"98a32b77-8f2c-4482-a7c9-4756b9be1839","name":"PBB A39","description":"Passenger Boarding Bridge at Gate A39","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.901854,"longitude":-97.038876}}}',
  },
  {
    id: 'DFW-A8-PBB',
    name: 'PBB A8',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a181f973-1564-410e-b0be-3f0293746a1f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A8-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.6642595Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a181f973-1564-410e-b0be-3f0293746a1f","name":"PBB A8","description":"Passenger Boarding Bridge at Gate A8","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.907703,"longitude":-97.038909}}}',
  },
  {
    id: 'DFW-A9-PBB',
    name: 'PBB A9',
    siteId: 'ca3bfb6c-2292-495f-ab5c-e11f1983ebef',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '4de14a71-e212-485b-9ec4-56dfadb87e9d',
    externalId: '',
    rawTwin:
      '{"id":"DFW-A9-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.7399588Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"4de14a71-e212-485b-9ec4-56dfadb87e9d","name":"PBB A9","description":"Passenger Boarding Bridge at Gate A9","siteID":"ca3bfb6c-2292-495f-ab5c-e11f1983ebef","coordinates":{"latitude":32.907428,"longitude":-97.038499}}}',
  },
  {
    id: 'DFW-B11-PBB',
    name: 'PBB B11',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7fef8c19-b532-4107-80f0-e38ee29c40c6',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B11-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.8135112Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7fef8c19-b532-4107-80f0-e38ee29c40c6","name":"PBB B11","description":"Passenger Boarding Bridge at Gate B11","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.90254,"longitude":-97.044419}}}',
  },
  {
    id: 'DFW-B12-PBB',
    name: 'PBB B12',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '73ebcfb1-a992-4c85-9c79-4c8eb31b0cd1',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B12-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.8985156Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"73ebcfb1-a992-4c85-9c79-4c8eb31b0cd1","name":"PBB B12","description":"Passenger Boarding Bridge at Gate B12","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.902775,"longitude":-97.044742}}}',
  },
  {
    id: 'DFW-B14-PBB',
    name: 'PBB B14',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0b4e081a-2d5e-4f62-b5f5-c779cdac5492',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B14-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:30.9616912Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0b4e081a-2d5e-4f62-b5f5-c779cdac5492","name":"PBB B14","description":"Passenger Boarding Bridge at Gate B14","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.903039,"longitude":-97.045039}}}',
  },
  {
    id: 'DFW-B15A-PBB',
    name: 'PBB B15A',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0276d645-8112-480c-8a8a-b136351edf7f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B15A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.0277142Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0276d645-8112-480c-8a8a-b136351edf7f","name":"PBB B15A","description":"Passenger Boarding Bridge at Gate B15A","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.903254,"longitude":-97.045215}}}',
  },
  {
    id: 'DFW-B16A-PBB',
    name: 'PBB B16A',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '87474aa3-cf01-4642-b072-ce5f6c553e72',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B16A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.0965487Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"87474aa3-cf01-4642-b072-ce5f6c553e72","name":"PBB B16A","description":"Passenger Boarding Bridge at Gate B16A","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.903594,"longitude":-97.045327}}}',
  },
  {
    id: 'DFW-B17-PBB',
    name: 'PBB B17',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'cb716b90-ffcb-41df-97cb-1fbd1ea24916',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B17-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.1678991Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"cb716b90-ffcb-41df-97cb-1fbd1ea24916","name":"PBB B17","description":"Passenger Boarding Bridge at Gate B17","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.90378,"longitude":-97.045497}}}',
  },
  {
    id: 'DFW-B18A-PBB',
    name: 'PBB B18A',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3bf815bd-d3ce-409b-9633-0742afecf9fa',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B18A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.2302623Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3bf815bd-d3ce-409b-9633-0742afecf9fa","name":"PBB B18A","description":"Passenger Boarding Bridge at Gate B18A","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.904034,"longitude":-97.045656}}}',
  },
  {
    id: 'DFW-B19-PBB',
    name: 'PBB B19',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'bb5a8c1b-fde1-483e-bbc8-cd8d5d00afba',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B19-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.2980743Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"bb5a8c1b-fde1-483e-bbc8-cd8d5d00afba","name":"PBB B19","description":"Passenger Boarding Bridge at Gate B19","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.904355,"longitude":-97.045689}}}',
  },
  {
    id: 'DFW-B1A-PBB',
    name: 'PBB B1A',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c43ae790-6222-4223-8850-8722c5c4ccbe',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B1A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.3610549Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c43ae790-6222-4223-8850-8722c5c4ccbe","name":"PBB B1A","description":"Passenger Boarding Bridge at Gate B1A","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.900857,"longitude":-97.042302}}}',
  },
  {
    id: 'DFW-B2-PBB',
    name: 'PBB B2',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '10234f54-1830-42f7-b389-695de0531843',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B2-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.4256352Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"10234f54-1830-42f7-b389-695de0531843","name":"PBB B2","description":"Passenger Boarding Bridge at Gate B2","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.901099,"longitude":-97.042284}}}',
  },
  {
    id: 'DFW-B22-PBB',
    name: 'PBB B22',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e971b4e5-af4b-46d7-b14c-4ce4250f504d',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B22-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.4940604Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e971b4e5-af4b-46d7-b14c-4ce4250f504d","name":"PBB B22","description":"Passenger Boarding Bridge at Gate B22","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.904887,"longitude":-97.045642}}}',
  },
  {
    id: 'DFW-B24-PBB',
    name: 'PBB B24',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7cae43f8-7c1f-4f1f-a78b-09533919f871',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B24-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.5512908Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7cae43f8-7c1f-4f1f-a78b-09533919f871","name":"PBB B24","description":"Passenger Boarding Bridge at Gate B24","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.905204,"longitude":-97.045701}}}',
  },
  {
    id: 'DFW-B25-PBB',
    name: 'PBB B25',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '136a567d-f9dd-4ad0-a66b-2277ef1a10fe',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B25-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.6225830Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"136a567d-f9dd-4ad0-a66b-2277ef1a10fe","name":"PBB B25","description":"Passenger Boarding Bridge at Gate B25","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.905464,"longitude":-97.045441}}}',
  },
  {
    id: 'DFW-B27-PBB',
    name: 'PBB B27',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '743881b4-01ef-4bd5-99ef-c34646540b35',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B27-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.7097064Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"743881b4-01ef-4bd5-99ef-c34646540b35","name":"PBB B27","description":"Passenger Boarding Bridge at Gate B27","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.905935,"longitude":-97.045367}}}',
  },
  {
    id: 'DFW-B28-PBB',
    name: 'PBB B28',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'dea8c5d9-863e-447f-95a2-233a67b5ca26',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B28-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.7704985Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"dea8c5d9-863e-447f-95a2-233a67b5ca26","name":"PBB B28","description":"Passenger Boarding Bridge at Gate B28","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.906097,"longitude":-97.045181}}}',
  },
  {
    id: 'DFW-B29-PBB',
    name: 'PBB B29',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '904de912-d5b8-4726-b485-666744908655',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B29-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.8248587Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"904de912-d5b8-4726-b485-666744908655","name":"PBB B29","description":"Passenger Boarding Bridge at Gate B29","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.906445,"longitude":-97.044992}}}',
  },
  {
    id: 'DFW-B30-PBB',
    name: 'PBB B30',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0846834f-812c-4474-a6ba-e05622fe77b8',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B30-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.8857490Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0846834f-812c-4474-a6ba-e05622fe77b8","name":"PBB B30","description":"Passenger Boarding Bridge at Gate B30","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.906842,"longitude":-97.044961}}}',
  },
  {
    id: 'DFW-B31-PBB',
    name: 'PBB B31',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'bb44ffac-1c8d-4855-b9a9-1a93e1b11220',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B31-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:31.9648387Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"bb44ffac-1c8d-4855-b9a9-1a93e1b11220","name":"PBB B31","description":"Passenger Boarding Bridge at Gate B31","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907394,"longitude":-97.04478}}}',
  },
  {
    id: 'DFW-B32-PBB',
    name: 'PBB B32',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'bdf7fcfb-779d-4263-96f3-3dbc2ef192f0',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B32-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.0228531Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"bdf7fcfb-779d-4263-96f3-3dbc2ef192f0","name":"PBB B32","description":"Passenger Boarding Bridge at Gate B32","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907421,"longitude":-97.045118}}}',
  },
  {
    id: 'DFW-B33-PBB',
    name: 'PBB B33',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f6a9af57-2bb1-4381-95d6-1ff574bfc988',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B33-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.0815944Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f6a9af57-2bb1-4381-95d6-1ff574bfc988","name":"PBB B33","description":"Passenger Boarding Bridge at Gate B33","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907486,"longitude":-97.045429}}}',
  },
  {
    id: 'DFW-B34-PBB',
    name: 'PBB B34',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '5ce98013-4f56-4732-a6a5-81ef0769fd9f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B34-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.1528954Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"5ce98013-4f56-4732-a6a5-81ef0769fd9f","name":"PBB B34","description":"Passenger Boarding Bridge at Gate B34","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.906948,"longitude":-97.04551}}}',
  },
  {
    id: 'DFW-B35-PBB',
    name: 'PBB B35',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7dee3cb5-6487-43a4-9a4b-6bee964d2b93',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B35-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.2080900Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7dee3cb5-6487-43a4-9a4b-6bee964d2b93","name":"PBB B35","description":"Passenger Boarding Bridge at Gate B35","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907471,"longitude":-97.0457}}}',
  },
  {
    id: 'DFW-B36-PBB',
    name: 'PBB B36',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '34b3acbd-6425-4a15-8c07-72af6925b169',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B36-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.2822164Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"34b3acbd-6425-4a15-8c07-72af6925b169","name":"PBB B36","description":"Passenger Boarding Bridge at Gate B36","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907001,"longitude":-97.045979}}}',
  },
  {
    id: 'DFW-B3A-PBB',
    name: 'PBB B3A',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a7859ce7-bf3f-454c-9489-0802e4266b89',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B3A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.3489143Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a7859ce7-bf3f-454c-9489-0802e4266b89","name":"PBB B3A","description":"Passenger Boarding Bridge at Gate B3A","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.901415,"longitude":-97.042201}}}',
  },
  {
    id: 'DFW-B4-PBB',
    name: 'PBB B4',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3bb0594d-75ac-4b7e-bb2f-824a6c29bbb5',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B4-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.4029486Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3bb0594d-75ac-4b7e-bb2f-824a6c29bbb5","name":"PBB B4","description":"Passenger Boarding Bridge at Gate B4","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.901922,"longitude":-97.042399}}}',
  },
  {
    id: 'DFW-B40-PBB',
    name: 'PBB B40',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '251caa82-0eff-4056-9198-2e1a5801c946',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B40-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.4688301Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"251caa82-0eff-4056-9198-2e1a5801c946","name":"PBB B40","description":"Passenger Boarding Bridge at Gate B40","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907208,"longitude":-97.044387}}}',
  },
  {
    id: 'DFW-B42-PBB',
    name: 'PBB B42',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f573abe8-83cf-4912-8de3-2b4453eb11ca',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B42-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.5333968Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f573abe8-83cf-4912-8de3-2b4453eb11ca","name":"PBB B42","description":"Passenger Boarding Bridge at Gate B42","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.90734,"longitude":-97.043987}}}',
  },
  {
    id: 'DFW-B44-PBB',
    name: 'PBB B44',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '2522d7be-2b19-42bf-a9e5-c0376ffd0128',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B44-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.6054688Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"2522d7be-2b19-42bf-a9e5-c0376ffd0128","name":"PBB B44","description":"Passenger Boarding Bridge at Gate B44","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907523,"longitude":-97.043377}}}',
  },
  {
    id: 'DFW-B46-PBB',
    name: 'PBB B46',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '80aa1df6-133c-48da-86ff-348fab89bd46',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B46-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.6604066Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"80aa1df6-133c-48da-86ff-348fab89bd46","name":"PBB B46","description":"Passenger Boarding Bridge at Gate B46","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907583,"longitude":-97.042889}}}',
  },
  {
    id: 'DFW-B47-PBB',
    name: 'PBB B47',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0ffca5e3-8c84-41bd-aff5-a4f29acd671b',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B47-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.7224504Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0ffca5e3-8c84-41bd-aff5-a4f29acd671b","name":"PBB B47","description":"Passenger Boarding Bridge at Gate B47","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907654,"longitude":-97.042569}}}',
  },
  {
    id: 'DFW-B48-PBB',
    name: 'PBB B48',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '1ad08664-86e2-4b94-9909-dc78a5a06801',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B48-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.7901936Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"1ad08664-86e2-4b94-9909-dc78a5a06801","name":"PBB B48","description":"Passenger Boarding Bridge at Gate B48","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.907628,"longitude":-97.042219}}}',
  },
  {
    id: 'DFW-B49-PBB',
    name: 'PBB B49',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '2194c296-7613-4208-bad4-a3b156cc9700',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B49-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.8638135Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"2194c296-7613-4208-bad4-a3b156cc9700","name":"PBB B49","description":"Passenger Boarding Bridge at Gate B49","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.9077,"longitude":-97.041875}}}',
  },
  {
    id: 'DFW-B5-PBB',
    name: 'PBB B5',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '91746637-a5d5-4a8c-b343-e40351018429',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B5-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:32.9485752Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"91746637-a5d5-4a8c-b343-e40351018429","name":"PBB B5","description":"Passenger Boarding Bridge at Gate B5","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.901991,"longitude":-97.04294}}}',
  },
  {
    id: 'DFW-B6-PBB',
    name: 'PBB B6',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'd6ad3fb8-92d8-4f20-80dc-34422ffb576b',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B6-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.0074628Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"d6ad3fb8-92d8-4f20-80dc-34422ffb576b","name":"PBB B6","description":"Passenger Boarding Bridge at Gate B6","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.902041,"longitude":-97.043353}}}',
  },
  {
    id: 'DFW-B7-PBB',
    name: 'PBB B7',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b295a73f-326e-4b83-9cb4-e7448968176e',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B7-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.0670501Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b295a73f-326e-4b83-9cb4-e7448968176e","name":"PBB B7","description":"Passenger Boarding Bridge at Gate B7","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.902063,"longitude":-97.043737}}}',
  },
  {
    id: 'DFW-B9-PBB',
    name: 'PBB B9',
    siteId: '89302684-93ea-4043-a77d-ffb1911cf388',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '89780f6d-4163-452c-b0dc-56be8637701a',
    externalId: '',
    rawTwin:
      '{"id":"DFW-B9-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.1398094Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"89780f6d-4163-452c-b0dc-56be8637701a","name":"PBB B9","description":"Passenger Boarding Bridge at Gate B9","siteID":"89302684-93ea-4043-a77d-ffb1911cf388","coordinates":{"latitude":32.902293,"longitude":-97.044089}}}',
  },
  {
    id: 'DFW-C10-PBB',
    name: 'PBB C10',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e6d158fa-ee8e-457a-b5ac-453d08e098f4',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C10-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.2079584Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e6d158fa-ee8e-457a-b5ac-453d08e098f4","name":"PBB C10","description":"Passenger Boarding Bridge at Gate C10","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.899962,"longitude":-97.036508}}}',
  },
  {
    id: 'DFW-C11-PBB',
    name: 'PBB C11',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b5cd3b1e-6556-46f3-bd78-85ab11316e5b',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C11-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.2612058Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b5cd3b1e-6556-46f3-bd78-85ab11316e5b","name":"PBB C11","description":"Passenger Boarding Bridge at Gate C11","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.899715,"longitude":-97.036252}}}',
  },
  {
    id: 'DFW-C12-PBB',
    name: 'PBB C12',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c66d517a-8a28-4cc5-8a74-57f431cc7ff2',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C12-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.3307866Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c66d517a-8a28-4cc5-8a74-57f431cc7ff2","name":"PBB C12","description":"Passenger Boarding Bridge at Gate C12","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.899554,"longitude":-97.035978}}}',
  },
  {
    id: 'DFW-C14-PBB',
    name: 'PBB C14',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f2e434f5-1040-4d8a-917a-32feb500d735',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C14-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.4023254Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f2e434f5-1040-4d8a-917a-32feb500d735","name":"PBB C14","description":"Passenger Boarding Bridge at Gate C14","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.899277,"longitude":-97.035707}}}',
  },
  {
    id: 'DFW-C15-PBB',
    name: 'PBB C15',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'eec23e93-5c69-4384-b40e-eede5884b519',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C15-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.4823219Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"eec23e93-5c69-4384-b40e-eede5884b519","name":"PBB C15","description":"Passenger Boarding Bridge at Gate C15","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.898887,"longitude":-97.035446}}}',
  },
  {
    id: 'DFW-C16-PBB',
    name: 'PBB C16',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'd9fee771-beb8-4986-8405-94d4b717cef5',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C16-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.5352069Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"d9fee771-beb8-4986-8405-94d4b717cef5","name":"PBB C16","description":"Passenger Boarding Bridge at Gate C16","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.898761,"longitude":-97.03521}}}',
  },
  {
    id: 'DFW-C17-PBB',
    name: 'PBB C17',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b7398e59-2900-4f11-8fd4-53642b0cadad',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C17-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.5930464Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b7398e59-2900-4f11-8fd4-53642b0cadad","name":"PBB C17","description":"Passenger Boarding Bridge at Gate C17","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.898446,"longitude":-97.035444}}}',
  },
  {
    id: 'DFW-C19-PBB',
    name: 'PBB C19',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '70207af8-fb35-4666-9223-017f1292b777',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C19-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.6768812Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"70207af8-fb35-4666-9223-017f1292b777","name":"PBB C19","description":"Passenger Boarding Bridge at Gate C19","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.897965,"longitude":-97.035268}}}',
  },
  {
    id: 'DFW-C2-PBB',
    name: 'PBB C2',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c75ac42d-614f-415d-bf53-6431966b236e',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C2-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.7706284Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c75ac42d-614f-415d-bf53-6431966b236e","name":"PBB C2","description":"Passenger Boarding Bridge at Gate C2","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.900454,"longitude":-97.038772}}}',
  },
  {
    id: 'DFW-C20-PBB',
    name: 'PBB C20',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'cb44c25d-752a-433b-8967-41e87f1dd5fb',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C20-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.8583149Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"cb44c25d-752a-433b-8967-41e87f1dd5fb","name":"PBB C20","description":"Passenger Boarding Bridge at Gate C20","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.897756,"longitude":-97.035066}}}',
  },
  {
    id: 'DFW-C21-PBB',
    name: 'PBB C21',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b7d640b1-b306-4f94-872a-65f601275e5f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C21-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.9111532Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b7d640b1-b306-4f94-872a-65f601275e5f","name":"PBB C21","description":"Passenger Boarding Bridge at Gate C21","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.897467,"longitude":-97.035239}}}',
  },
  {
    id: 'DFW-C22-PBB',
    name: 'PBB C22',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'cc530161-c5cd-4879-b9aa-eafe633f4d36',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C22-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:33.9759100Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"cc530161-c5cd-4879-b9aa-eafe633f4d36","name":"PBB C22","description":"Passenger Boarding Bridge at Gate C22","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.897122,"longitude":-97.03491}}}',
  },
  {
    id: 'DFW-C24-PBB',
    name: 'PBB C24',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '1698f435-ef8a-4d1c-9153-1142abbdf56e',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C24-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.0393891Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"1698f435-ef8a-4d1c-9153-1142abbdf56e","name":"PBB C24","description":"Passenger Boarding Bridge at Gate C24","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.896843,"longitude":-97.035269}}}',
  },
  {
    id: 'DFW-C26-PBB',
    name: 'PBB C26',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '9f0188df-c5ea-4c71-bd0d-d8d02fd22242',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C26-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.1041676Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"9f0188df-c5ea-4c71-bd0d-d8d02fd22242","name":"PBB C26","description":"Passenger Boarding Bridge at Gate C26","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.89643,"longitude":-97.035223}}}',
  },
  {
    id: 'DFW-C27-PBB',
    name: 'PBB C27',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0ecdb837-5fa1-40b4-b8b4-080dc205e03c',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C27-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.1701620Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0ecdb837-5fa1-40b4-b8b4-080dc205e03c","name":"PBB C27","description":"Passenger Boarding Bridge at Gate C27","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.89603,"longitude":-97.035561}}}',
  },
  {
    id: 'DFW-C28-PBB',
    name: 'PBB C28',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '1992d244-23c4-4250-b5ca-8e12b0a29c7b',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C28-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.2345691Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"1992d244-23c4-4250-b5ca-8e12b0a29c7b","name":"PBB C28","description":"Passenger Boarding Bridge at Gate C28","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.895781,"longitude":-97.03575}}}',
  },
  {
    id: 'DFW-C29-PBB',
    name: 'PBB C29',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6f0abc1f-9c8b-413f-855c-3b26f186c79e',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C29-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.3017634Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6f0abc1f-9c8b-413f-855c-3b26f186c79e","name":"PBB C29","description":"Passenger Boarding Bridge at Gate C29","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.895487,"longitude":-97.036149}}}',
  },
  {
    id: 'DFW-C30-PBB',
    name: 'PBB C30',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'edaa8155-9135-4c01-87c7-ff4f19b3b935',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C30-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.3523453Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"edaa8155-9135-4c01-87c7-ff4f19b3b935","name":"PBB C30","description":"Passenger Boarding Bridge at Gate C30","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.89518,"longitude":-97.036415}}}',
  },
  {
    id: 'DFW-C31-PBB',
    name: 'PBB C31',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7cf6bdc6-9e5a-4f14-8478-52f4d5792d73',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C31-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.4050794Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7cf6bdc6-9e5a-4f14-8478-52f4d5792d73","name":"PBB C31","description":"Passenger Boarding Bridge at Gate C31","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.894884,"longitude":-97.036652}}}',
  },
  {
    id: 'DFW-C35-PBB',
    name: 'PBB C35',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '985bddd9-0217-48e3-8557-11ea1a21a7a2',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C35-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.4646068Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"985bddd9-0217-48e3-8557-11ea1a21a7a2","name":"PBB C35","description":"Passenger Boarding Bridge at Gate C35","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.894823,"longitude":-97.037468}}}',
  },
  {
    id: 'DFW-C36-PBB',
    name: 'PBB C36',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f13749e5-f54a-4971-8074-0d144f214918',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C36-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.5285923Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f13749e5-f54a-4971-8074-0d144f214918","name":"PBB C36","description":"Passenger Boarding Bridge at Gate C36","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.894852,"longitude":-97.037906}}}',
  },
  {
    id: 'DFW-C37-PBB',
    name: 'PBB C37',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '2e462797-2447-457f-a6ff-95f238658975',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C37-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.5775814Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"2e462797-2447-457f-a6ff-95f238658975","name":"PBB C37","description":"Passenger Boarding Bridge at Gate C37","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.894807,"longitude":-97.038324}}}',
  },
  {
    id: 'DFW-C38-PBB',
    name: 'PBB C38',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c681ccb7-e452-4a96-8090-3a0190f69e42',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C38-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.6485190Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c681ccb7-e452-4a96-8090-3a0190f69e42","name":"PBB C38","description":"Passenger Boarding Bridge at Gate C38","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.894868,"longitude":-97.038636}}}',
  },
  {
    id: 'DFW-C39-PBB',
    name: 'PBB C39',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6ee7a8e9-03d2-40f6-a14b-22c3b26fa387',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C39-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.7108159Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6ee7a8e9-03d2-40f6-a14b-22c3b26fa387","name":"PBB C39","description":"Passenger Boarding Bridge at Gate C39","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.894772,"longitude":-97.038841}}}',
  },
  {
    id: 'DFW-C4-PBB',
    name: 'PBB C4',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '68b24ea6-13db-4995-b467-23896445c5db',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C4-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.7699923Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"68b24ea6-13db-4995-b467-23896445c5db","name":"PBB C4","description":"Passenger Boarding Bridge at Gate C4","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.900451,"longitude":-97.03821}}}',
  },
  {
    id: 'DFW-C6-PBB',
    name: 'PBB C6',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '618be836-7bf5-436d-bc9d-05ecf5429665',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C6-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.8351966Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"618be836-7bf5-436d-bc9d-05ecf5429665","name":"PBB C6","description":"Passenger Boarding Bridge at Gate C6","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.900378,"longitude":-97.037684}}}',
  },
  {
    id: 'DFW-C7-PBB',
    name: 'PBB C7',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'eb18b919-d093-4f39-98fa-962308850ec3',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C7-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.8920400Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"eb18b919-d093-4f39-98fa-962308850ec3","name":"PBB C7","description":"Passenger Boarding Bridge at Gate C7","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.900245,"longitude":-97.037224}}}',
  },
  {
    id: 'DFW-C8-PBB',
    name: 'PBB C8',
    siteId: '086b76ee-9527-4469-b262-f44f49c0ab85',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e92ce95d-d7f6-4cb1-922b-05b7ae1a3142',
    externalId: '',
    rawTwin:
      '{"id":"DFW-C8-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:34.9720716Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e92ce95d-d7f6-4cb1-922b-05b7ae1a3142","name":"PBB C8","description":"Passenger Boarding Bridge at Gate C8","siteID":"086b76ee-9527-4469-b262-f44f49c0ab85","coordinates":{"latitude":32.90021,"longitude":-97.036836}}}',
  },
  {
    id: 'DFWA-D1-PBB',
    name: 'PBB D1',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '958e3a40-beed-4c64-ae4d-0a43212a5b78',
    externalId: '33241.PBB.001',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D1-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.1356163Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"958e3a40-beed-4c64-ae4d-0a43212a5b78","name":"PBB D1","description":"Passenger Boarding Bridge at Gate D1","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.893049,"longitude":-97.042263},"geometryViewerID":"54a844c5-5f87-4fa1-8077-ae83d5889036","externalID":"33241.PBB.001","serialNumber":"75558TB4133248","modelNumber":"TB41/19.5-3"}}',
  },
  {
    id: 'DFWA-D10-PBB',
    name: 'PBB D10',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b8ea39a5-1b53-494e-8663-cc9210f0a56d',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D10-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.1985427Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b8ea39a5-1b53-494e-8663-cc9210f0a56d","name":"PBB D10","description":"Passenger Boarding Bridge at Gate D10","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895252,"longitude":-97.04},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998c6a"}}',
  },
  {
    id: 'DFWA-D11-PBB',
    name: 'PBB D11',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'ea8cf45f-a467-40dc-bf2b-851ee54d7563',
    externalId: 'DFW-PBB-D11',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D11-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.2935018Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"ea8cf45f-a467-40dc-bf2b-851ee54d7563","name":"PBB D11","description":"Passenger Boarding Bridge at Gate D11","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895316,"longitude":-97.04},"geometryViewerID":"30e78e4a-6132-4609-a5b7-c4b94d01ece1","externalID":"DFW-PBB-D11"}}',
  },
  {
    id: 'DFWA-D12-PBB',
    name: 'PBB D12',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '4a3660a4-5842-47d3-8797-2ba0d0d1918e',
    externalId: 'DFW-PBB-D12',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D12-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.3625637Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"4a3660a4-5842-47d3-8797-2ba0d0d1918e","name":"PBB D12","description":"Passenger Boarding Bridge at Gate D12","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895171,"longitude":-97.04},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998e70","externalID":"DFW-PBB-D12"}}',
  },
  {
    id: 'DFWA-D14-PBB',
    name: 'PBB D14',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '5aa1b721-2f7e-450b-94b2-83adacd729c6',
    externalId: 'DFW-PBB-D14',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D14-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.4195782Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"5aa1b721-2f7e-450b-94b2-83adacd729c6","name":"PBB D14","description":"Passenger Boarding Bridge at Gate D14","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895195,"longitude":-97.04},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998d5a","externalID":"DFW-PBB-D14"}}',
  },
  {
    id: 'DFWA-D15-PBB',
    name: 'PBB D15',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f4eebc33-bf7e-48c0-9b37-d96bf25d050d',
    externalId: 'DFW-PBB-D15',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D15-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.5032409Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f4eebc33-bf7e-48c0-9b37-d96bf25d050d","name":"PBB D15","description":"Passenger Boarding Bridge at Gate D15","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895283,"longitude":-97.05},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998d15","externalID":"DFW-PBB-D15"}}',
  },
  {
    id: 'DFWA-D16-PBB',
    name: 'PBB D16',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f07eeb1b-4fd4-4e9b-853b-22ff097c2ee8',
    externalId: 'DFW-PBB-D16',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D16-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.5559739Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f07eeb1b-4fd4-4e9b-853b-22ff097c2ee8","name":"PBB D16","description":"Passenger Boarding Bridge at Gate D16","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895244,"longitude":-97.05},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998b23","externalID":"DFW-PBB-D16"}}',
  },
  {
    id: 'DFWA-D16X-PBB',
    name: 'PBB D16X',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '9b77b15a-0052-4550-bbd3-0f0cc78eb7ee',
    externalId: 'DFW-PBB-D16X',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D16X-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.6528724Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"9b77b15a-0052-4550-bbd3-0f0cc78eb7ee","name":"PBB D16X","description":"Passenger Boarding Bridge at Gate D16X","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895393,"longitude":-97.05},"externalID":"DFW-PBB-D16X"}}',
  },
  {
    id: 'DFWA-D17-PBB',
    name: 'PBB D17',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '1b76dd4d-4eec-4082-ae97-fbb2608fb2dc',
    externalId: 'DFW-PBB-D17',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D17-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.7081580Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"1b76dd4d-4eec-4082-ae97-fbb2608fb2dc","name":"PBB D17","description":"Passenger Boarding Bridge at Gate D17","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.89552,"longitude":-97.05},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998b22","externalID":"DFW-PBB-D17"}}',
  },
  {
    id: 'DFW-D18-PBB',
    name: 'PBB D18',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '39ab922e-4b80-4350-8654-d61b1e6cb1a1',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D18-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.0434072Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"39ab922e-4b80-4350-8654-d61b1e6cb1a1","name":"PBB D18","description":"Passenger Boarding Bridge at Gate D18","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895845,"longitude":-97.045617}}}',
  },
  {
    id: 'DFW-D18A-PBB',
    name: 'PBB D18A',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '8472671e-7e5a-4ab0-861a-3da961812bab',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D18A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.1081072Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"8472671e-7e5a-4ab0-861a-3da961812bab","name":"PBB D18A","description":"Passenger Boarding Bridge at Gate D18A","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895943,"longitude":-97.04558}}}',
  },
  {
    id: 'DFWA-D2-PBB',
    name: 'PBB D2',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'bfabab53-9e87-4247-bce5-e59626107391',
    externalId: '33241.PBB.002',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D2-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.7783592Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"bfabab53-9e87-4247-bce5-e59626107391","name":"PBB D2","description":"Passenger Boarding Bridge at Gate D2","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.893191,"longitude":-97.04},"geometryViewerID":"d08da003-cff3-4b3e-aaf9-bda1f821b8ab","externalID":"33241.PBB.002","serialNumber":"75558TB4533252","modelNumber":"TB45/21.0-3"}}',
  },
  {
    id: 'DFW-D20-PBB',
    name: 'PBB D20',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '180a2abe-4cb3-4e1a-9c27-01d3634a4ccc',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D20-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.1859726Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"180a2abe-4cb3-4e1a-9c27-01d3634a4ccc","name":"PBB D20","description":"Passenger Boarding Bridge at Gate D20","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.896294,"longitude":-97.045557}}}',
  },
  {
    id: 'DFW-D21-PBB',
    name: 'PBB D21',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e6e8b975-586f-4a49-87d6-2cdaffb74783',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D21-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.2580425Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e6e8b975-586f-4a49-87d6-2cdaffb74783","name":"PBB D21","description":"Passenger Boarding Bridge at Gate D21","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.896635,"longitude":-97.045573}}}',
  },
  {
    id: 'DFW-D21A-PBB',
    name: 'PBB D21A',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e640aeda-0a6b-4f01-9c13-31230dff8d54',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D21A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.3153827Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e640aeda-0a6b-4f01-9c13-31230dff8d54","name":"PBB D21A","description":"Passenger Boarding Bridge at Gate D21A","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.896571,"longitude":-97.045444}}}',
  },
  {
    id: 'DFW-D22-PBB',
    name: 'PBB D22',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'afcf0bbb-c310-48a4-80bf-ac8e00d6fd4a',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D22-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.3803425Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"afcf0bbb-c310-48a4-80bf-ac8e00d6fd4a","name":"PBB D22","description":"Passenger Boarding Bridge at Gate D22","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.897128,"longitude":-97.045603}}}',
  },
  {
    id: 'DFW-D23-PBB',
    name: 'PBB D23',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3bcfec3b-bbf2-4e21-9f87-9a249ff2e556',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D23-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.4856433Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3bcfec3b-bbf2-4e21-9f87-9a249ff2e556","name":"PBB D23","description":"Passenger Boarding Bridge at Gate D23","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.897649,"longitude":-97.045612}}}',
  },
  {
    id: 'DFW-D25-PBB',
    name: 'PBB D25',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b4f0451a-6b7f-4e56-a373-eca5e0ae3f9e',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D25-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.5411125Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b4f0451a-6b7f-4e56-a373-eca5e0ae3f9e","name":"PBB D25","description":"Passenger Boarding Bridge at Gate D25","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.898387,"longitude":-97.045628}}}',
  },
  {
    id: 'DFW-D25A-PBB',
    name: 'PBB D25A',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'ae968332-4cd9-4c05-ae93-9c84ae84de00',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D25A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.6235214Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"ae968332-4cd9-4c05-ae93-9c84ae84de00","name":"PBB D25A","description":"Passenger Boarding Bridge at Gate D25A","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.898314,"longitude":-97.045462}}}',
  },
  {
    id: 'DFW-D27-PBB',
    name: 'PBB D27',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '15390a79-e11e-4dab-bba2-49c0c596ff50',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D27-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.6805398Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"15390a79-e11e-4dab-bba2-49c0c596ff50","name":"PBB D27","description":"Passenger Boarding Bridge at Gate D27","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.898813,"longitude":-97.045519}}}',
  },
  {
    id: 'DFW-D29-PBB',
    name: 'PBB D29',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a1966003-4b14-47f8-a3ad-51442b37efba',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D29-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.7659691Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a1966003-4b14-47f8-a3ad-51442b37efba","name":"PBB D29","description":"Passenger Boarding Bridge at Gate D29","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899504,"longitude":-97.045626}}}',
  },
  {
    id: 'DFWA-D3-PBB',
    name: 'PBB D3',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '14708b6e-3092-4ac8-bb98-62137ff4e7ac',
    externalId: '33241.PBB.003',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D3-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.8396821Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"14708b6e-3092-4ac8-bb98-62137ff4e7ac","name":"PBB D3","description":"Passenger Boarding Bridge at Gate D3","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.893762,"longitude":-97.04},"geometryViewerID":"ce263d77-c096-4b81-bcea-8dee9bc13fb6","externalID":"33241.PBB.003","serialNumber":"75558TB4533252","modelNumber":"TB45/21.0-3"}}',
  },
  {
    id: 'DFW-D30-PBB',
    name: 'PBB D30',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '474f6548-6c1e-4603-aeeb-66a1944ab1a1',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D30-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.8493796Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"474f6548-6c1e-4603-aeeb-66a1944ab1a1","name":"PBB D30","description":"Passenger Boarding Bridge at Gate D30","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899961,"longitude":-97.045675}}}',
  },
  {
    id: 'DFW-D31-PBB',
    name: 'PBB D31',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '51449a57-4681-4ea8-a051-87145d4e0e0b',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D31-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.9054408Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"51449a57-4681-4ea8-a051-87145d4e0e0b","name":"PBB D31","description":"Passenger Boarding Bridge at Gate D31","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899917,"longitude":-97.045319}}}',
  },
  {
    id: 'DFW-D33-PBB',
    name: 'PBB D33',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'bf9ac5e7-e12b-4885-84e4-c6baaf75d79c',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D33-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:35.9730651Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"bf9ac5e7-e12b-4885-84e4-c6baaf75d79c","name":"PBB D33","description":"Passenger Boarding Bridge at Gate D33","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899892,"longitude":-97.044749}}}',
  },
  {
    id: 'DFW-D34-PBB',
    name: 'PBB D34',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '19d18ff0-32ac-4015-9ad6-782ad324a4df',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D34-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.0711596Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"19d18ff0-32ac-4015-9ad6-782ad324a4df","name":"PBB D34","description":"Passenger Boarding Bridge at Gate D34","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.8999,"longitude":-97.044213}}}',
  },
  {
    id: 'DFW-D36-PBB',
    name: 'PBB D36',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '1ca065ea-c8c0-48c4-990e-eb3110a62ae4',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D36-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.1357199Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"1ca065ea-c8c0-48c4-990e-eb3110a62ae4","name":"PBB D36","description":"Passenger Boarding Bridge at Gate D36","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899963,"longitude":-97.04361}}}',
  },
  {
    id: 'DFW-D37-PBB',
    name: 'PBB D37',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'bca8495f-e36d-457a-bbd7-822ac6f175bf',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D37-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.2110418Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"bca8495f-e36d-457a-bbd7-822ac6f175bf","name":"PBB D37","description":"Passenger Boarding Bridge at Gate D37","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899866,"longitude":-97.043128}}}',
  },
  {
    id: 'DFW-D38-PBB',
    name: 'PBB D38',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7a524ac8-0e6a-4af8-b049-578275387e08',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D38-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.2701405Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7a524ac8-0e6a-4af8-b049-578275387e08","name":"PBB D38","description":"Passenger Boarding Bridge at Gate D38","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.899896,"longitude":-97.042633}}}',
  },
  {
    id: 'DFW-D3X-PBB',
    name: 'PBB D3X',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '49c126a0-9acb-4276-bf91-414ca6c04369',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D3X-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.3586382Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"49c126a0-9acb-4276-bf91-414ca6c04369","name":"PBB D3X","description":"Passenger Boarding Bridge at Gate D3X","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1"}}',
  },
  {
    id: 'DFWA-D4-PBB',
    name: 'PBB D4',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0d9b9893-1411-4aec-865e-d62216690444',
    externalId: '33241.PBB.004',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D4-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.9521969Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0d9b9893-1411-4aec-865e-d62216690444","name":"PBB D4","description":"Passenger Boarding Bridge at Gate D4","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.8939,"longitude":-97.04},"geometryViewerID":"e68df6e7-d2cf-4984-830d-db0fd158f559","externalID":"33241.PBB.004","serialNumber":"75558TB4133247","modelNumber":"TB41/19.5-3"}}',
  },
  {
    id: 'DFW-D40-PBB',
    name: 'PBB D40',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'ce1fca8c-37c9-40b6-ab62-60b91755f6ad',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D40-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.4351343Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"ce1fca8c-37c9-40b6-ab62-60b91755f6ad","name":"PBB D40","description":"Passenger Boarding Bridge at Gate D40","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.8999,"longitude":-97.042257}}}',
  },
  {
    id: 'DFW-D5-PBB',
    name: 'PBB D5',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'eee35034-4890-4ec7-a32e-a2031c47cf27',
    externalId: '',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFW-D5-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.5104548Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"eee35034-4890-4ec7-a32e-a2031c47cf27","name":"PBB D5","description":"Passenger Boarding Bridge at Gate D5","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895403,"longitude":-97.041938}}}',
  },
  {
    id: 'DFWA-D6-PBB',
    name: 'PBB D6',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '64187bf0-e0b1-4993-a906-db9d8c5c2526',
    externalId: 'DFW-PBB-D06',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D6-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:40.0114514Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"64187bf0-e0b1-4993-a906-db9d8c5c2526","name":"PBB D6","description":"Passenger Boarding Bridge at Gate D6","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895318,"longitude":-97.04},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998c0e","externalID":"DFW-PBB-D06"}}',
  },
  {
    id: 'DFWA-D7-PBB',
    name: 'PBB D7',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '4a925c1a-59b0-425a-bbe7-bb0c969bf17d',
    externalId: 'DFW-PBB-D07',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D7-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:40.0752177Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"4a925c1a-59b0-425a-bbe7-bb0c969bf17d","name":"PBB D7","description":"Passenger Boarding Bridge at Gate D7","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895217,"longitude":-97.04},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998c0f","externalID":"DFW-PBB-D07"}}',
  },
  {
    id: 'DFWA-D8-PBB',
    name: 'PBB D8',
    siteId: 'df9d03af-9951-4b88-96ae-635c3bad2cb1',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '29393297-0764-4f9b-aa40-e5c090caf50a',
    externalId: 'DFW-PBB-D08',
    floorId: '80ce2cfe-d65e-4e3a-9c18-bbc2cfa57728',
    floorName: 'Concourse Level - L3',
    rawTwin:
      '{"id":"DFWA-D8-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:40.1359385Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"29393297-0764-4f9b-aa40-e5c090caf50a","name":"PBB D8","description":"Passenger Boarding Bridge at Gate D8","siteID":"df9d03af-9951-4b88-96ae-635c3bad2cb1","coordinates":{"latitude":32.895356,"longitude":-97.04},"geometryViewerID":"6a14e9db-aefb-4e75-9d06-24bf14998c55","externalID":"DFW-PBB-D08"}}',
  },
  {
    id: 'DFW-E10-PBB',
    name: 'PBB E10',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '01c16511-8ad8-4ddc-877c-6dffb2740b76',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E10-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.5638601Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"01c16511-8ad8-4ddc-877c-6dffb2740b76","name":"PBB E10","description":"Passenger Boarding Bridge at Gate E10","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.892483575395914,"longitude":-97.0361392170728}}}',
  },
  {
    id: 'DFW-E11-PBB',
    name: 'PBB E11',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '131757b0-7ca2-49df-aa0b-0d4c9c5a80c0',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E11-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.6299381Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"131757b0-7ca2-49df-aa0b-0d4c9c5a80c0","name":"PBB E11","description":"Passenger Boarding Bridge at Gate E11","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89222569501986,"longitude":-97.035769072224}}}',
  },
  {
    id: 'DFW-E12-PBB',
    name: 'PBB E12',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3370e04c-b909-48bd-a5cd-5ed4b2133b4f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E12-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.6839310Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3370e04c-b909-48bd-a5cd-5ed4b2133b4f","name":"PBB E12","description":"Passenger Boarding Bridge at Gate E12","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89202411987788,"longitude":-97.03566714828752}}}',
  },
  {
    id: 'DFW-E13-PBB',
    name: 'PBB E13',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c7629ef0-af7b-47e1-a0ad-bb02a9072dcd',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E13-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.7491138Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c7629ef0-af7b-47e1-a0ad-bb02a9072dcd","name":"PBB E13","description":"Passenger Boarding Bridge at Gate E13","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.891669012846606,"longitude":-97.0354829041774}}}',
  },
  {
    id: 'DFW-E14-PBB',
    name: 'PBB E14',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a9c01b04-ce3b-435a-b4ff-583afec155db',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E14-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.8318622Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a9c01b04-ce3b-435a-b4ff-583afec155db","name":"PBB E14","description":"Passenger Boarding Bridge at Gate E14","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89123650856523,"longitude":-97.03534472687896}}}',
  },
  {
    id: 'DFW-E15-PBB',
    name: 'PBB E15',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6d243bf5-534f-406b-8693-d8d1de7e84fe',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E15-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.8911217Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6d243bf5-534f-406b-8693-d8d1de7e84fe","name":"PBB E15","description":"Passenger Boarding Bridge at Gate E15","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89083222034401,"longitude":-97.035322270743}}}',
  },
  {
    id: 'DFW-E16-PBB',
    name: 'PBB E16',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '75c6921e-9b37-4770-8b81-f5d79c66ab6d',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E16-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:36.9604971Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"75c6921e-9b37-4770-8b81-f5d79c66ab6d","name":"PBB E16","description":"Passenger Boarding Bridge at Gate E16","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89036594747595,"longitude":-97.03532782917344}}}',
  },
  {
    id: 'DFW-E17-PBB',
    name: 'PBB E17',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '4b062bd1-09b1-48da-acf7-5b305d7a5f67',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E17-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.0496135Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"4b062bd1-09b1-48da-acf7-5b305d7a5f67","name":"PBB E17","description":"Passenger Boarding Bridge at Gate E17","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88993107039149,"longitude":-97.03532251748749}}}',
  },
  {
    id: 'DFW-E18-PBB',
    name: 'PBB E18',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'b5095474-6d54-4759-b856-c14114a1c80e',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E18-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.1105174Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"b5095474-6d54-4759-b856-c14114a1c80e","name":"PBB E18","description":"Passenger Boarding Bridge at Gate E18","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88954853792441,"longitude":-97.03551264491476}}}',
  },
  {
    id: 'DFW-E2-PBB',
    name: 'PBB E2',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c9a245f4-27b2-41af-ab08-67ee78dfd947',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E2-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.1747818Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c9a245f4-27b2-41af-ab08-67ee78dfd947","name":"PBB E2","description":"Passenger Boarding Bridge at Gate E2","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89330185922747,"longitude":-97.03900393215126}}}',
  },
  {
    id: 'DFW-E20-PBB',
    name: 'PBB E20',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '0d873cc3-6be5-45a1-9085-28f8d3c4b8a0',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E20-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.2407799Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"0d873cc3-6be5-45a1-9085-28f8d3c4b8a0","name":"PBB E20","description":"Passenger Boarding Bridge at Gate E20","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88922598349574,"longitude":-97.03559586355622}}}',
  },
  {
    id: 'DFW-E21-PBB',
    name: 'PBB E21',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'aae23b02-a1aa-4af6-bd44-32e563614315',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E21-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.3107635Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"aae23b02-a1aa-4af6-bd44-32e563614315","name":"PBB E21","description":"Passenger Boarding Bridge at Gate E21","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88887979735697,"longitude":-97.03575048804902}}}',
  },
  {
    id: 'DFW-E22A-PBB',
    name: 'PBB E22A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '933e926d-f746-40a6-996d-5af0f3941f6f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E22A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.3703866Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"933e926d-f746-40a6-996d-5af0f3941f6f","name":"PBB E22A","description":"Passenger Boarding Bridge at Gate E22A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.887406956859486,"longitude":-97.03409636607364}}}',
  },
  {
    id: 'DFW-E22B-PBB',
    name: 'PBB E22B',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e0873a59-ae30-4964-a60c-6d7b2e884daf',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E22B-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.4346353Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e0873a59-ae30-4964-a60c-6d7b2e884daf","name":"PBB E22B","description":"Passenger Boarding Bridge at Gate E22B","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88725875716502,"longitude":-97.0343666054371}}}',
  },
  {
    id: 'DFW-E23A-PBB',
    name: 'PBB E23A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '11bbc8d8-9ea3-4996-98f9-d0251e301918',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E23A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.5060029Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"11bbc8d8-9ea3-4996-98f9-d0251e301918","name":"PBB E23A","description":"Passenger Boarding Bridge at Gate E23A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88710750807799,"longitude":-97.03450548465572}}}',
  },
  {
    id: 'DFW-E24A-PBB',
    name: 'PBB E24A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3cd369ac-b1f5-4c43-bd80-cb88adc65bab',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E24A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.5620526Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3cd369ac-b1f5-4c43-bd80-cb88adc65bab","name":"PBB E24A","description":"Passenger Boarding Bridge at Gate E24A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.887146532210146,"longitude":-97.03504069930425}}}',
  },
  {
    id: 'DFW-E26-PBB',
    name: 'PBB E26',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f460cedb-2c3f-4618-a93a-d3632a38e6f8',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E26-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.6202194Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f460cedb-2c3f-4618-a93a-d3632a38e6f8","name":"PBB E26","description":"Passenger Boarding Bridge at Gate E26","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.887602164217505,"longitude":-97.03497149371056}}}',
  },
  {
    id: 'DFW-E27A-PBB',
    name: 'PBB E27A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'ee3d8ad0-ee4f-4388-9a87-febbf3888133',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E27A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.7076144Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"ee3d8ad0-ee4f-4388-9a87-febbf3888133","name":"PBB E27A","description":"Passenger Boarding Bridge at Gate E27A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88796530848032,"longitude":-97.03466261258296}}}',
  },
  {
    id: 'DFW-E27B-PBB',
    name: 'PBB E27B',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '07c30f27-0297-4880-afd9-51769c1d0b6a',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E27B-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.7709745Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"07c30f27-0297-4880-afd9-51769c1d0b6a","name":"PBB E27B","description":"Passenger Boarding Bridge at Gate E27B","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88832797465793,"longitude":-97.0344256627099}}}',
  },
  {
    id: 'DFW-E29A-PBB',
    name: 'PBB E29A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3e67b088-1530-440d-9ad4-44decf07cf0b',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E29A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.8582194Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3e67b088-1530-440d-9ad4-44decf07cf0b","name":"PBB E29A","description":"Passenger Boarding Bridge at Gate E29A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88870772964734,"longitude":-97.03419998755469}}}',
  },
  {
    id: 'DFW-E30-PBB',
    name: 'PBB E30',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '56e5019e-d59d-47a9-b575-ae6adf4f79fd',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E30-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.9189391Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"56e5019e-d59d-47a9-b575-ae6adf4f79fd","name":"PBB E30","description":"Passenger Boarding Bridge at Gate E30","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88860042508016,"longitude":-97.03397771820757}}}',
  },
  {
    id: 'DFW-E31-PBB',
    name: 'PBB E31',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '38aca3f2-daaa-4c0d-85bc-cd14311fbdb8',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E31-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:37.9798992Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"38aca3f2-daaa-4c0d-85bc-cd14311fbdb8","name":"PBB E31","description":"Passenger Boarding Bridge at Gate E31","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.888541675888575,"longitude":-97.03594840104458}}}',
  },
  {
    id: 'DFW-E32-PBB',
    name: 'PBB E32',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '48e0b1e0-5554-4e7e-b712-eddbe71dec96',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E32-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.0417708Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"48e0b1e0-5554-4e7e-b712-eddbe71dec96","name":"PBB E32","description":"Passenger Boarding Bridge at Gate E32","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.888245663325414,"longitude":-97.0362594460013}}}',
  },
  {
    id: 'DFW-E33-PBB',
    name: 'PBB E33',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'a57e531a-0819-4670-bd35-7b2aa428f4ad',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E33-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.1140013Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"a57e531a-0819-4670-bd35-7b2aa428f4ad","name":"PBB E33","description":"Passenger Boarding Bridge at Gate E33","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88799315062822,"longitude":-97.03659260706144}}}',
  },
  {
    id: 'DFW-E34A-PBB',
    name: 'PBB E34A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'ad209ace-6250-4499-9507-9be1953877b0',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E34A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.1722583Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"ad209ace-6250-4499-9507-9be1953877b0","name":"PBB E34A","description":"Passenger Boarding Bridge at Gate E34A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.8877542797462,"longitude":-97.03699611554836}}}',
  },
  {
    id: 'DFW-E35A-PBB',
    name: 'PBB E35A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6e955d03-d5d4-49ff-aec3-b1ba9e93c9ab',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E35A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.2444618Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6e955d03-d5d4-49ff-aec3-b1ba9e93c9ab","name":"PBB E35A","description":"Passenger Boarding Bridge at Gate E35A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88760807016443,"longitude":-97.03716518086168}}}',
  },
  {
    id: 'DFW-E35B-PBB',
    name: 'PBB E35B',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e296f9ff-7a2a-42c7-8e0f-a78693a539e6',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E35B-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.3002197Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e296f9ff-7a2a-42c7-8e0f-a78693a539e6","name":"PBB E35B","description":"Passenger Boarding Bridge at Gate E35B","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.887534868727585,"longitude":-97.0374213318327}}}',
  },
  {
    id: 'DFW-E36-PBB',
    name: 'PBB E36',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '7bd0f308-a9a4-4c23-94cf-6e64a96f53ef',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E36-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.3587865Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"7bd0f308-a9a4-4c23-94cf-6e64a96f53ef","name":"PBB E36","description":"Passenger Boarding Bridge at Gate E36","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88756492062684,"longitude":-97.03778232360592}}}',
  },
  {
    id: 'DFW-E37A-PBB',
    name: 'PBB E37A',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6257e7ed-24c4-40c3-bd10-e9f46497d96f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E37A-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.4148575Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6257e7ed-24c4-40c3-bd10-e9f46497d96f","name":"PBB E37A","description":"Passenger Boarding Bridge at Gate E37A","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.887467379067495,"longitude":-97.03828785265692}}}',
  },
  {
    id: 'DFW-E37B-PBB',
    name: 'PBB E37B',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '1d2f5ac8-977b-4628-b1e8-eb30940a56b3',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E37B-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.4954007Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"1d2f5ac8-977b-4628-b1e8-eb30940a56b3","name":"PBB E37B","description":"Passenger Boarding Bridge at Gate E37B","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.88743965156204,"longitude":-97.03853324193408}}}',
  },
  {
    id: 'DFW-E38B-PBB',
    name: 'PBB E38B',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'c618d6b5-6211-4b0e-9f7b-b9be526b316f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E38B-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.5530102Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"c618d6b5-6211-4b0e-9f7b-b9be526b316f","name":"PBB E38B","description":"Passenger Boarding Bridge at Gate E38B","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.8875022472787,"longitude":-97.03893196758156}}}',
  },
  {
    id: 'DFW-E4-PBB',
    name: 'PBB E4',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'f2d4196a-52ee-4cb5-9423-5a51f5e85712',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E4-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.6318151Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"f2d4196a-52ee-4cb5-9423-5a51f5e85712","name":"PBB E4","description":"Passenger Boarding Bridge at Gate E4","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89316642176174,"longitude":-97.03843529623902}}}',
  },
  {
    id: 'DFW-E5-PBB',
    name: 'PBB E5',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: 'e60f42d2-5f7d-403a-aa83-3e7f23c0a142',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E5-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.6883272Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"e60f42d2-5f7d-403a-aa83-3e7f23c0a142","name":"PBB E5","description":"Passenger Boarding Bridge at Gate E5","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.893208889464766,"longitude":-97.03806896349812}}}',
  },
  {
    id: 'DFW-E6-PBB',
    name: 'PBB E6',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '644278b9-d498-4cdd-acfc-5ab8afdfa73f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E6-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.7463085Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"644278b9-d498-4cdd-acfc-5ab8afdfa73f","name":"PBB E6","description":"Passenger Boarding Bridge at Gate E6","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89313657957856,"longitude":-97.03760968064594}}}',
  },
  {
    id: 'DFW-E7-PBB',
    name: 'PBB E7',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '273c4264-7ea0-4541-9631-11bd03aaf4c9',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E7-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.8140326Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"273c4264-7ea0-4541-9631-11bd03aaf4c9","name":"PBB E7","description":"Passenger Boarding Bridge at Gate E7","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89303327963958,"longitude":-97.03721190890649}}}',
  },
  {
    id: 'DFW-E8-PBB',
    name: 'PBB E8',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '3462aa2d-4d90-4fec-af47-d9b19621161f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E8-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.8741050Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"3462aa2d-4d90-4fec-af47-d9b19621161f","name":"PBB E8","description":"Passenger Boarding Bridge at Gate E8","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89296104624623,"longitude":-97.0368500024863}}}',
  },
  {
    id: 'DFW-E9-PBB',
    name: 'PBB E9',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '58435efc-973d-43cc-8eef-f8b7b0864351',
    externalId: '',
    rawTwin:
      '{"id":"DFW-E9-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.9446495Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"58435efc-973d-43cc-8eef-f8b7b0864351","name":"PBB E9","description":"Passenger Boarding Bridge at Gate E9","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61","coordinates":{"latitude":32.89268965393577,"longitude":-97.03645974105928}}}',
  },
  {
    id: 'DFW-NB2-PBB',
    name: 'PBB NB2',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '6d611a38-0678-41ca-8a98-c9f58fb34938',
    externalId: '',
    rawTwin:
      '{"id":"DFW-NB2-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:38.9967679Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"6d611a38-0678-41ca-8a98-c9f58fb34938","name":"PBB NB2","description":"Passenger Boarding Bridge at Gate NB2","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61"}}',
  },
  {
    id: 'DFW-NB5-PBB',
    name: 'PBB NB5',
    siteId: '8a00b5e6-cb87-43f4-99a0-7bc5eea18a61',
    modelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
    uniqueId: '5891fa95-be47-4a83-894e-b16e8b08749f',
    externalId: '',
    rawTwin:
      '{"id":"DFW-NB5-PBB","modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","metadata":{"modelId":"dtmi:com:willowinc:airport:PassengerBoardingBridge;1","lastUpdatedOn":"2023-10-25T18:51:39.0588393Z"},"etag":null,"customProperties":{"geometrySpatialReference":"Passenger Boarding Bridges","uniqueID":"5891fa95-be47-4a83-894e-b16e8b08749f","name":"PBB NB5","description":"Passenger Boarding Bridge at Gate NB5","siteID":"8a00b5e6-cb87-43f4-99a0-7bc5eea18a61"}}',
  },
]

const foodDisplayCaseTwinId = 'WIL-Retail-007-Case-1-7C'
const foodDisplayCaseTwin = {
  twin: {
    id: foodDisplayCaseTwinId,
    metadata: {
      modelId:
        'dtmi:com:willowinc:MediumTemperatureRefrigeratedFoodDisplayCase;1',
      contents: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      contentsRetailValue: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      contentsRetailValueUnit: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      contentsReplacementCost: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      contentsReplacementCostUnit: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      uniqueID: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      name: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      description: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      siteID: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
      modelNumber: {
        lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
    },
    etag: 'W/"f1df9d42-9fca-4a1f-a54d-1ac0c99e78ea"',
    contents: 'Seafood',
    contentsRetailValue: 2000,
    contentsRetailValueUnit: 'USD',
    contentsReplacementCost: 1600,
    contentsReplacementCostUnit: 'USD',
    uniqueID: 'c631c120-daa7-440d-9216-5b301ddae807',
    name: 'Case 1-7C',
    description: 'Fish Cooler',
    siteID: 'bbb0dd63-656e-46e7-b523-1af465d24aa9',
    modelNumber: 'FGV',
    evaporatorFan: {
      $metadata: {
        $lastUpdateTime: '2023-10-26T04:32:20.3774416Z',
      },
    },
  },
  permissions: {
    edit: true,
  },
}
