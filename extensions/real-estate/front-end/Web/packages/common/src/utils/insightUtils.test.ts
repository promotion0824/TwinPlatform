import { v4 as uuidv4 } from 'uuid'
import { selectOccurrences } from './insightUtils'
import { Occurrence } from '../insights/insights/types'

describe('selectOccurrences', () => {
  it('should return empty array if no occurrences', () => {
    const result = selectOccurrences([])
    expect(result).toHaveLength(0)
  })
  it('should merge overlapping occurrences', () => {
    const earliest = '2023-11-08T01:04:38.346Z'
    const latest = '2023-11-08T23:19:33.346Z'
    const occurrenceDates = [
      {
        started: '2023-11-08T06:24:38.346Z',
        ended: '2023-11-08T10:32:18.346Z',
      },
      {
        started: earliest,
        ended: '2023-11-08T23:12:49.346Z',
      },
      {
        started: '2023-11-08T01:23:23.346Z',
        ended: latest,
      },
    ]

    const occurrences = occurrenceDates.map((d) => makeOccurrence(d))
    const result = selectOccurrences(occurrences)
    expect(result).toHaveLength(1)
    expect(result[0].started).toEqual(earliest)
    expect(result[0].ended).toEqual(latest)
  })

  it('should merge overlapping occurrences of different types', () => {
    const earliestInsufficient = '2023-01-08T06:24:38.346Z'
    const latestInsufficient = '2023-11-08T23:19:33.346Z'
    const insufficentOccurrenceDates = [
      {
        started: earliestInsufficient,
        ended: '2023-11-08T10:32:18.346Z',
      },
      {
        started: '2023-10-08T01:23:23.346Z',
        ended: latestInsufficient,
      },
      {
        started: '2023-07-08T01:23:23.346Z',
        ended: '2023-11-08T18:12:45.346Z',
      },
    ]

    const earliestFaulty = '2023-05-05T12:04:38.346Z'
    const latestFaulty = '2023-11-08T23:19:33.346Z'
    const faultyOccurrenceDates = [
      {
        started: earliestFaulty,
        ended: '2023-11-08T10:32:18.346Z',
      },
      {
        started: '2023-07-08T01:23:23.346Z',
        ended: latestFaulty,
      },
      {
        started: '2023-09-08T01:23:23.346Z',
        ended: '2023-11-08T23:19:32.346Z',
      },
    ]

    const insufficentOccurrences = insufficentOccurrenceDates.map((d) =>
      makeOccurrence({ ...d, isValid: false })
    )
    const faultyOccurrences = faultyOccurrenceDates.map((d) =>
      makeOccurrence({ ...d, isFaulted: true, isValid: true })
    )
    const result = selectOccurrences([
      ...insufficentOccurrences,
      ...faultyOccurrences,
    ])
    expect(result).toHaveLength(2)
    expect(result.find((o) => o.started === earliestInsufficient)?.ended).toBe(
      latestInsufficient
    )
    expect(result.find((o) => o.started === earliestFaulty)?.ended).toBe(
      latestFaulty
    )
  })
})

const makeOccurrence = ({
  isFaulted = false,
  isValid = false,
  started = '2020-01-01T00:00:00Z',
  ended = '2020-01-01T00:00:00Z',
}): Occurrence => ({
  insightId: uuidv4(),
  id: uuidv4(),
  started,
  ended,
  isFaulted,
  isValid,
  text: 'some random text dont care',
})
