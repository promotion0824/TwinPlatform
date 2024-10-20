import { getGranularityOptions, getDefaultGranularity } from './utils'

describe('Granularity utils', () => {
  describe('getGranularityOptions', () => {
    test('Get possible options for exactly 1 day', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-14T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { minutes: 5 },
        { minutes: 10 },
        { minutes: 15 },
        { minutes: 30 },
      ])
    })

    test('Get possible options for exactly 3 days', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-16T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { minutes: 5 },
        { minutes: 10 },
        { minutes: 15 },
        { minutes: 30 },
        { hours: 1 },
      ])
    })

    test('Get possible options for exactly 7 days', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-20T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { minutes: 10 },
        { minutes: 15 },
        { minutes: 30 },
        { hours: 1 },
        { hours: 2 },
        { hours: 4 },
        { hours: 12 },
        { days: 1 },
      ])
    })

    test('Get possible options for exactly 11 days', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-24T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { minutes: 15 },
        { minutes: 30 },
        { hours: 1 },
        { hours: 2 },
        { hours: 4 },
        { hours: 12 },
        { days: 1 },
      ])
    })

    test('Get possible options for exactly 35 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-07-05T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { minutes: 30 },
        { hours: 1 },
        { hours: 2 },
        { hours: 4 },
        { hours: 12 },
        { days: 1 },
      ])
    })

    test('Get possible options for exactly 91 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-08-30T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { hours: 2 },
        { hours: 4 },
        { hours: 12 },
        { days: 1 },
        { weeks: 1 },
      ])
    })

    test('Get possible options for exactly 182 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-11-30T15:00:00.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { hours: 4 },
        { hours: 12 },
        { days: 1 },
        { weeks: 1 },
        { months: 1 },
      ])
    })

    test('Get possible options for exactly 182 days and 1 second', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-11-30T15:00:01.000+02:00',
      ]
      expect(getGranularityOptions(times)).toStrictEqual([
        { hours: 12 },
        { days: 1 },
        { weeks: 1 },
        { months: 1 },
      ])
    })
  })

  describe('getDefaultGranularity', () => {
    test('Get default granularity for exactly 15 minutes', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-13T15:15:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ minutes: 5 })
    })

    test('Get default granularity for exactly 15 minutes and 1 second', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-13T15:15:01.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ minutes: 15 })
    })

    test('Get default granularity for exactly 11 days', () => {
      const times: [string, string] = [
        '2022-06-13T15:00:00.000+02:00',
        '2022-06-24T15:00:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ minutes: 15 })
    })

    test('Get default granularity for exactly 35 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-07-05T15:00:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ minutes: 30 })
    })

    test('Get default granularity for exactly 49 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-07-19T15:00:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ hours: 1 })
    })

    test('Get default granularity for exactly 91 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-08-30T15:00:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ hours: 2 })
    })

    test('Get default granularity for exactly 182 days', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-11-30T15:00:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ hours: 4 })
    })

    test('Get default granularity for axactly 182 days and 1 second', () => {
      const times: [string, string] = [
        '2022-06-01T15:00:00.000+02:00',
        '2022-11-30T15:01:00.000+02:00',
      ]

      expect(getDefaultGranularity(times)).toStrictEqual({ hours: 12 })
    })
  })
})
