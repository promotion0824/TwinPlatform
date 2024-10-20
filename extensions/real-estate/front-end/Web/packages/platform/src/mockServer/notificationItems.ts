/* eslint-disable import/prefer-default-export */
import {
  NotificationSource,
  NotificationStatus,
} from '@willow/common/notifications/types'
import { rest } from 'msw'

const result = {
  after: 0,
  before: 0,
  total: 2,
  items: [
    {
      id: '1',
      userId: '1234',
      title: 'Air Handling Unit AC-10-1',
      source: 'insight',
      properties: {
        id: 'a7cffbc0-4dbe-4ba3-9906-213e58b4a064',
        twinId: 'WIL-104BDFD-AC-10-1',
        twinName: 'Air Handling Unit AC-10-1',
        twinModelId: 'dtmi:com:willowinc:AirHandlingUnit;1',
        category: 'energy',
        priority: 1,
      },
      createdDateTime: new Date(Date.now() - 10 * 60 * 1000).toISOString(),
      state: 'new',
    },
    {
      id: '2',
      source: 'insight',
      title: 'Thermal Energy Meter Stuck',
      userId: '1234',
      properties: {
        id: '190f0f2b-3840-4b7f-a223-161edd07bbcf',
        twinId: 'WIL-104BDFD-TEM-CHW-RETAILB',
        twinName: 'Air Handling Unit AC-2-0-1',
        twinModelId: 'dtmi:com:willowinc:ThermalMeter;1',
        category: 'dataQuality',
        priority: 2,
      },
      state: 'open',
      createdDateTime: '2024-07-31T16:23:10.647Z',
    },
  ],
}

export const handlers = [
  rest.post(`/:region/api/notifications/all`, (_req, res, ctx) =>
    res(ctx.delay(2000), ctx.json(result))
  ),

  rest.put(`/:region/api/notifications/status`, (_req, res, ctx) =>
    res(ctx.delay(4000), ctx.status(200))
  ),

  rest.put(
    `/:region/api/users/:userId/notifications/status/all`,
    (req, res, ctx) => res(ctx.delay(4000), ctx.status(200))
  ),
]

/**
 * Util function to be used only in test to make a notification object.
 */
export const makeNotification = ({
  id,
  source,
  propertyBagJson,
  createdDateTime,
  state,
  title,
}: {
  id: string
  source: NotificationSource
  propertyBagJson: string
  createdDateTime?: string
  state?: NotificationStatus
  title: string
}) => ({
  id,
  userId: '1234',
  title,
  source,
  propertyBagJson,
  createdDateTime: createdDateTime ?? Date.now(),
  state: state ?? NotificationStatus.New,
})

export const makeNotificationsStats = (notifications) => {
  const newNotificationsCount = notifications.filter(
    ({ state }) => state === NotificationStatus.New
  ).length

  return [
    {
      state: NotificationStatus.New,
      count: newNotificationsCount,
    },
    {
      state: NotificationStatus.Open,
      count: notifications.length - newNotificationsCount,
    },
  ]
}
