/* eslint-disable complexity */
import { Insight, SourceType } from '../insights/types'
import {
  formatValue,
  formatDateTime,
  getTotalImpactScore,
  getRollUpTotalImpactScores,
  InsightCostImpactPropNames,
} from './utils'

const firstOccurredDate = '2023-10-02T09:56:12.164Z'
const insight: Insight = {
  id: 'ee6c98d3-3166-4f43-9618-d84c442d59be',
  siteId: '926d1b17-05f7-47bb-b57b-75a922e69a20',
  sequenceNumber: 'EUG-I-1',
  floorCode: 'L01',
  equipmentId: '483a09ea-ed9b-4596-b564-6f0ed60248ba',
  type: 'energy',
  name: 'Terminal Unit Zone',
  priority: 2,
  status: 'open',
  lastStatus: 'open',
  state: 'active',
  updatedDate: '2022-08-11T05:15:07.794Z',
  occurredDate: '2022-08-12T07:06:56.828Z',
  sourceType: SourceType.app,
  sourceName: 'Ruling Engine V3',
  isSourceIdRulingEngineAppId: false,
  externalId: '',
  occurrenceCount: 179,
  impactScores: [
    {
      name: InsightCostImpactPropNames.dailyAvoidableEnergy,
      value: 100,
      unit: 'kWh',
    },
    {
      name: InsightCostImpactPropNames.dailyAvoidableCost,
      value: 200,
      unit: 'USD',
    },
    {
      name: InsightCostImpactPropNames.totalEnergyToDate,
      value: 500,
      unit: 'kWh',
    },
    {
      name: InsightCostImpactPropNames.totalCostToDate,
      value: 1000,
      unit: 'USD',
    },
    {
      name: InsightCostImpactPropNames.priorityScore,
      value: 5,
      unit: '',
    },
  ],
  previouslyIgnored: 0,
  previouslyResolved: 0,
}

const timeZone = 'America/New_York'
const t = jest.fn().mockImplementation((text) => {
  switch (text) {
    case 'interpolation.impactSavings':
      return '{{ impact }} Savings'
    case 'interpolation.avoidableExpensePerYear':
      return 'Avoidable Expense Per Year'
    case 'plainText.cost':
      return 'Cost'
    case 'plainText.energy':
      return 'Energy'
    case 'plainText.avoidableCost':
      return 'Avoidable Cost'
    case 'plainText.energyUsage':
      return 'Energy Usage'
    case 'interpolation.savingsPerYear':
      return 'The estimated yearly {{ item }} that could be avoided if the suggested issue within this insight was resolved'
    case 'interpolation.totalSavingsPerYear':
      return 'The estimated yearly {{ item }} that could be avoided if the unresolved insights were resolved'
    case 'interpolation.savingsToDate':
      return 'The estimated {{ item }} expended on this issue since the date the insight first occurred'
    case 'plainText.savingsPerYearTooltip':
      return 'The estimated yearly savings that could be realized if this insight remains resolved for a year.'
    case 'interpolation.timelyCostToolTip':
      return 'The {{ timely }} cost that could be avoided \r\nif the suggested issue within this \r\ninsight was to be addressed'
    case 'plainText.yearly':
      return 'Yearly'
    case 'plainText.totalPerYear':
      return 'Total Per Year'
    case 'plainText.perYear':
      return 'Per Year'
    case 'interpolation.expenseToDate':
      return '{{ expense }} to date'
    case 'interpolation.sinceWhen':
      return 'since {{ when }}'
    case 'plainText.total':
      return 'Total'
    default:
      return text
  }
})
const language = 'en'

describe('formatValue without rounding off', () => {
  test.each([
    {
      value: 10,
      language: 'en',
      unit: 'kWh',
      expected: '10.00 kWh',
    },
    {
      value: 123.12,
      language: 'en',
      unit: 'kWh',
      expected: '123.12 kWh',
    },
    {
      value: 1234.56789,
      language: 'en',
      unit: 'hr',
      expected: '1,234.57 hr',
    },
    {
      value: 123456789.56789,
      language: 'en',
      unit: 'USD',
      expected: '123,456,789.57 USD',
    },
    {
      value: 10,
      language: 'fr',
      unit: 'kWh',
      expected: '10,00 kWh',
    },
    {
      value: 123.12,
      language: 'fr',
      unit: 'kWh',
      expected: '123,12 kWh',
    },
    {
      value: 1234.56789,
      language: 'fr',
      unit: 'hr',
      expected: '1 234,57 hr',
    },
    {
      value: 123456789.56789,
      language: 'fr',
      unit: 'USD',
      expected: '123 456 789,57 USD',
    },
  ])(
    'value of "$value" with language of "$language" and unit of "$unit" will be formatted to $expected',
    async ({ value, language, unit, expected }) => {
      expect(formatValue({ value, language, unit })).toBe(expected)
    }
  )
})

describe('formatValue with round off', () => {
  test.each([
    {
      value: 10,
      language: 'en',
      unit: 'kWh',
      expected: '10 kWh',
    },
    {
      value: 123.12,
      language: 'en',
      unit: 'kWh',
      expected: '123 kWh',
    },
    {
      value: 1234.56789,
      language: 'en',
      unit: 'USD',
      expected: '1K USD',
    },
    {
      value: 123456789.56789,
      language: 'en',
      unit: 'USD',
      expected: '123M USD',
    },
    {
      value: 10,
      language: 'fr',
      unit: 'kWh',
      expected: '10 kWh',
    },
    {
      value: 123.12,
      language: 'fr',
      unit: 'kWh',
      expected: '123 kWh',
    },
    {
      value: 234.56,
      language: 'fr',
      unit: 'USD',
      expected: '235 USD',
    },
    {
      value: 123456789.56789,
      language: 'en',
      unit: 'USD',
      expected: '123M USD',
    },
  ])(
    'value of "$value" with language of "$language" and unit of "$unit" will be formatted to $expected',
    async ({ value, language, unit, expected }) => {
      expect(formatValue({ value, language, unit, isCompact: true })).toBe(
        expected
      )
    }
  )
})

describe('formatDateTime', () => {
  test.each([
    {
      language: 'en',
      expected: '',
    },
    {
      language: 'fr',
      expected: '',
    },
    {
      value: '2022-08-11T05:15:07.794Z',
      language: 'en',
      expected: 'Aug 11, 2022, 05:15',
    },
    {
      value: '2022-06-02T17:45:18.862Z',
      language: 'en',
      expected: 'Jun 2, 2022, 17:45',
    },
    {
      value: '2000-11-02T01:01:18.862Z',
      language: 'en',
      expected: 'Nov 2, 2000, 01:01',
    },
    {
      value: '2020-05-25T00:15:56.000Z',
      language: 'en',
      expected: 'May 25, 2020, 00:15',
    },
    {
      value: '2020-05-25T00:15:56.000Z',
      language: 'en',
      expected: 'May 25, 2020',
      includeTime: false,
    },
    {
      value: '2022-08-11T05:15:07.794Z',
      language: 'fr',
      expected: '11 août 2022, 05:15',
    },
    {
      value: '2022-06-02T17:45:18.862Z',
      language: 'fr',
      expected: '2 juin 2022, 17:45',
    },
    {
      value: '2000-11-02T01:01:18.862Z',
      language: 'fr',
      expected: '2 nov. 2000, 01:01',
    },
    {
      value: '2020-05-25T00:15:56.000Z',
      language: 'fr',
      expected: '25 mai 2020, 00:15',
    },
    {
      value: '2020-05-25T00:15:56.000Z',
      language: 'fr',
      expected: '25 mai 2020',
      includeTime: false,
    },
  ])(
    'timestamp of $value with language of $language will be formatted to $expected',
    async ({ value, language, expected, includeTime }) => {
      expect(formatDateTime({ value, language, includeTime })).toBe(expected)
    }
  )
})

describe('getTotalImpactScore', () => {
  const groupInsights: Insight[] = [
    {
      id: 'ee6c98d3-3166-4f43-9618-d84c442d59be',
      siteId: '926d1b17-05f7-47bb-b57b-75a922e69a20',
      sequenceNumber: 'EUG-I-1',
      floorCode: 'L01',
      equipmentId: '483a09ea-ed9b-4596-b564-6f0ed60248ba',
      type: 'energy',
      name: 'Terminal Unit Zone',
      priority: 2,
      status: 'open',
      lastStatus: 'open',
      state: 'active',
      updatedDate: '2022-08-11T05:15:07.794Z',
      occurredDate: '2022-08-12T07:06:56.828Z',
      sourceType: SourceType.app,
      sourceName: 'Ruling Engine V3',
      isSourceIdRulingEngineAppId: false,
      externalId: '',
      occurrenceCount: 179,
      impactScores: [],
      previouslyIgnored: 0,
      previouslyResolved: 0,
    },
  ]
  test('groupInsights is empty', () => {
    const result = getTotalImpactScore({
      groupInsights: [],
      scoreName: 'Terminal Unit Zone',
      language: 'en',
    })
    expect(result).toMatchObject({
      totalImpactScore: '--',
      value: undefined,
      unit: undefined,
    })
  })

  test('total impact score with unit when scoreName is not found in impactScores', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [{ name: 'score2', value: 10, unit: 'USD' }],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
    })
    expect(result).toMatchObject({
      totalImpactScore: '--',
      value: undefined,
      unit: undefined,
    })
  })

  test('total impact score with unit when value is 0 will be displayed as "--"', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 0, unit: 'USD' },
          ],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
    })
    expect(result).toMatchObject({
      totalImpactScore: '--',
      value: 0,
      unit: 'USD',
    })
  })

  test('total impact score with unit when scoreName is found in impactScores and returns the max cost at insight level', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 10.222137, unit: '' },
          ],
        },
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 20.311235, unit: 'USD' },
          ],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
    })
    expect(result).toMatchObject({
      totalImpactScore: '20.31 USD',
      value: 20.311235,
      unit: 'USD',
    })
  })

  test('total impact score with unit when scoreName is found in impactScores and return the total impact cost', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 20.1234, unit: '' },
          ],
        },
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 12.311235, unit: 'USD' },
          ],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
      isRollUpTotal: true,
    })
    expect(result).toMatchObject({
      totalImpactScore: '32.43 USD',
      value: 32.434635,
      unit: 'USD',
    })
  })

  test('value of "234567.89" with unit of "USD" is expected to be formatted to "235K USD"', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 234567.89, unit: 'USD' },
          ],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
      decimalPlaces: 0,
      isRollUpTotal: true,
    })
    expect(result).toMatchObject({
      totalImpactScore: '235K USD',
      value: 234567.89,
      unit: 'USD',
    })
  })

  test('value of "635567.89" with unit of "USD" is expected to be formatted to "636K USD"', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 635567.89, unit: 'USD' },
          ],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
      decimalPlaces: 0,
      isRollUpTotal: true,
    })
    expect(result).toMatchObject({
      totalImpactScore: '636K USD',
      value: 635567.89,
      unit: 'USD',
    })
  })

  test('value of "4850295.2" with unit of "kWh" is expected to be formatted to "5 GWh"', () => {
    const result = getTotalImpactScore({
      groupInsights: [
        {
          ...groupInsights[0],
          impactScores: [
            { name: groupInsights[0].name, value: 4850295.2, unit: 'kWh' },
          ],
        },
      ],
      scoreName: groupInsights[0].name ?? '',
      language: 'en',
      decimalPlaces: 0,
      isRollUpTotal: true,
    })
    expect(result).toMatchObject({
      totalImpactScore: '5 GWh',
      value: 4850295.2,
      unit: 'kWh',
    })
  })
})

describe('getRollUpTotalImpactScores', () => {
  test('should return an array of roll-up total impact scores', () => {
    const result = getRollUpTotalImpactScores({
      insight,
      timeZone,
      t,
      language,
      firstOccurredDate,
    })

    expect(result).toEqual([
      {
        hidden: false,
        name: 'Avoidable Expense Per Year',
        tooltip:
          'The estimated yearly {{ item }} that could be avoided if the suggested issue within this insight was resolved',
        value: '37 MWh Per Year',
      },
      {
        hidden: false,
        name: 'Avoidable Expense Per Year',
        tooltip:
          'The estimated yearly {{ item }} that could be avoided if the suggested issue within this insight was resolved',
        value: '73K USD Per Year',
      },
      {
        hidden: false,
        name: '{{ expense }} to date',
        tooltip:
          'The estimated {{ item }} expended on this issue since the date the insight first occurred',
        value: '500 kWh Since When',
      },
      {
        hidden: false,
        name: '{{ expense }} to date',
        tooltip:
          'The estimated {{ item }} expended on this issue since the date the insight first occurred',
        value: '1K USD Since When',
      },
      {
        hidden: true,
        name: undefined,
        tooltip: undefined,
        value: '5.00  ',
      },
    ])
  })

  test('should handle zero impact score values for both cost and energy values', () => {
    const insightWithZeroValues: Insight = {
      ...insight,
      impactScores: [
        {
          name: 'Name 1',
          value: 0,
          unit: 'kWh',
        },
        {
          name: 'Name 2',
          value: 0,
          unit: 'USD',
        },
      ],
    }

    const result = getRollUpTotalImpactScores({
      insight: insightWithZeroValues,
      timeZone,
      t,
      language,
      firstOccurredDate,
    })

    expect(result).toEqual([
      {
        hidden: false,
        name: 'Name 1',
        tooltip: undefined,
        value: '-- ',
      },
      {
        hidden: false,
        name: 'Name 2',
        tooltip: undefined,
        value: '-- ',
      },
    ])
  })
})
