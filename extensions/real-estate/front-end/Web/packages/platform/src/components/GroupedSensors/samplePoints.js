import { DateTime } from 'luxon'

const now = DateTime.now()

export const pointsWithHosted = [
  {
    name: 'VFD Inverter Temp Sensor',
    externalId: '-FACILITY-MANWEST-DFSDF',
    trendId: '1',
    properties: { siteID: { value: '1234' } },
  },
  {
    name: 'Compressor 3 Fault Sensor',
    externalId: '-FACILITY-MANWEST-ACB3COOLINGOADPRVHOUR',
    trendId: '2',
    properties: { siteID: { value: '1234' } },
  },
  {
    name: 'Air Flow Proven Sensor',
    externalId: '-FACILITY-MANWEST-ACB3COOLINGOADPRVHOUR_.Presentvalue',
    trendId: '3',
    properties: { siteID: { value: '1234' } },
  },
  {
    name: 'Compressor 5 Fault Sensor',
    externalId: 'MANWEST-ACB3COOLINGOADPRVHOUR_.Presentvalue',
    trendId: '4',
    properties: { siteID: { value: '1234' } },
  },
  {
    name: 'VFD Inverter Temp Sensor',
    externalId: '-FACILITY-ACB3COOLINGOADPRVHOUR_.Presentvalue',
    trendId: '5',
    properties: { siteID: { value: '1234' } },
  },
  {
    name: 'Air Flow Proven Sensor',
    externalId: 'Air Flow Proven Sensor',
    properties: { siteID: { value: '1234' } },
  },
]

export const pointsWithoutHosted = [
  {
    name: 'Variable Weight',
    externalId: '-FACILITY-MANWEST-DFSDF',
    trendId: '7',
    properties: { siteID: { value: '1234' } },
  },
  {
    name: 'Padded Vibration Sensor',
    externalId: '-FACILITY-MANWEST-ACB3COOLINGOADPRVHOUR',
    trendId: '8',
    properties: { siteID: { value: '1234' } },
  },
]

export const liveDataPoints = {
  1: {
    id: '1',
    liveDataValue: '65.75',
    unit: 'DEGF',
    liveDataTimestamp: now.toISO(),
  },
  2: {
    id: '2',
    liveDataValue: '65.75',
    unit: 'DEGF',
    liveDataTimestamp: now.minus({ minutes: 59 }).toISO(),
  },
  3: {
    id: '3',
    liveDataValue: '65.75',
    unit: 'DEGF',
    liveDataTimestamp: now.minus({ hours: 1 }).toISO(),
  },
  4: {
    id: '4',
    liveDataValue: '65.75',
    unit: 'DEGF',
    liveDataTimestamp: now.minus({ hours: 23, minutes: 59 }).toISO(),
  },
  5: {
    id: '5',
    liveDataValue: '65.75',
    unit: 'DEGF',
    liveDataTimestamp: now.minus({ days: 1 }).toISO(),
  },
  7: {
    id: '7',
    liveDataValue: '103',
    unit: '%',
    liveDataTimestamp: '2022-02-13T01:45:00.000Z',
  },
  8: {
    id: '8',
    liveDataValue: '1236',
    liveDataTimestamp: '2022-02-13T01:45:00.000Z',
  },
}
