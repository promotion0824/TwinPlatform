import { InsightPriority } from '../../insights/types'
import {
  calculatePriority,
  getPriorityValue,
  isValidScore,
} from '../getInsightPriority'
import { InsightCostImpactPropNames } from '../utils'

describe('getPriorityValue', () => {
  describe('should return impact score priority', () => {
    it('if both impact score and priority provided', () => {
      const result = getPriorityValue({
        impactScorePriority: '10',
        insightPriority: 3,
      })

      expect(result).toBe(10)
    })

    it('should return impact score if it has decimals', () => {
      const result = getPriorityValue({
        impactScorePriority: '10.01',
        insightPriority: 3,
      })

      expect(result).toBe(10.01)
    })

    it('with maximum value of 100 even if impact score priority > 100', () => {
      const result = getPriorityValue({
        impactScorePriority: '101',
        insightPriority: 3,
      })

      expect(result).toBe(100)
    })

    it('with minimum value of 0 even if impact score priority < 0', () => {
      const result = getPriorityValue({
        impactScorePriority: '-1',
        insightPriority: 3,
      })

      expect(result).toBe(0)
    })
  })

  describe('should return priority', () => {
    it('with scaled value if impact score as "--" provided', () => {
      const result = getPriorityValue({
        impactScorePriority: '--',
        insightPriority: 3,
      })

      expect(result).toBe(50)
    })

    it('with scaled value if impact score as undefined provided', () => {
      const result = getPriorityValue({
        impactScorePriority: '--',
        insightPriority: 3,
      })

      expect(result).toBe(50)
    })

    it('with minimum value as 0 even if the priority > 5', () => {
      const result = getPriorityValue({
        impactScorePriority: '--',
        insightPriority: 6 as InsightPriority,
      })

      expect(result).toBe(0)
    })

    it('with value 100 if priority is 0', () => {
      const result = getPriorityValue({
        impactScorePriority: '--',
        insightPriority: 0 as InsightPriority,
      })

      expect(result).toBe(100)
    })

    it('with maximum value as 100 even if the priority is < 0', () => {
      const result = getPriorityValue({
        impactScorePriority: '--',
        insightPriority: -1 as InsightPriority,
      })

      expect(result).toBe(100)
    })
  })

  it('should return undefined if impact score priority is not valid number and priority is not defined', () => {
    const result = getPriorityValue({
      impactScorePriority: 'invalid',
      insightPriority: undefined,
    })

    expect(result).toBe(undefined)
  })
})

describe('calculatePriority', () => {
  const generateImpactScores = (value: number, unit?: string) => [
    {
      name: InsightCostImpactPropNames.priorityScore,
      value,
      unit,
    },
  ]
  it('should return correct impact score priority if both priority impact score and priority provided', () => {
    const result = calculatePriority({
      impactScores: generateImpactScores(10),
      language: 'en',
      insightPriority: 4,
    })

    expect(result).toBe(10)
  })

  it('should return correct decimal impact score priority if priority impact score includes decimals', () => {
    const result = calculatePriority({
      impactScores: generateImpactScores(10.11),
      language: 'en',
      insightPriority: 4,
    })

    expect(result).toBe(10.11)
  })

  it('should return correct impact score priority if only priority impact score provided', () => {
    const result = calculatePriority({
      impactScores: generateImpactScores(10.0),
      language: 'en',
    })

    expect(result).toBe(10)
  })

  it('should return 0 if priority impact score is 0', () => {
    const result = calculatePriority({
      impactScores: generateImpactScores(0),
      language: 'en',
    })

    expect(result).toBe(0)
  })

  it('should return correct number value if priority impact score has an unit', () => {
    const result = calculatePriority({
      impactScores: generateImpactScores(10.0, 'percent'),
      language: 'en',
    })

    expect(result).toBe(10)
  })

  it('should return undefined if both priority impact score and priority are undefined', () => {
    const result = calculatePriority({
      impactScores: [],
      language: 'en',
    })

    expect(result).toBe(undefined)
  })
})

describe('isValidScore', () => {
  it('should be false if value is NaN', () => {
    expect(isValidScore(NaN)).toBe(false)
  })

  it('should be false if value is ""', () => {
    expect(isValidScore('')).toBe(false)
  })

  it('should be false if value is text string', () => {
    expect(isValidScore('test')).toBe(false)
  })

  it('should be true if value is an integer', () => {
    expect(isValidScore(1)).toBe(true)
  })

  it('should be true if value is a decimal number', () => {
    expect(isValidScore(10.0)).toBe(true)
  })

  it('should be true if value is an string integer', () => {
    expect(isValidScore('1')).toBe(true)
  })

  it('should be true if value is a string decimal', () => {
    expect(isValidScore('10.00')).toBe(true)
  })

  it('should be true if value is "0"', () => {
    expect(isValidScore('0')).toBe(true)
  })
})
