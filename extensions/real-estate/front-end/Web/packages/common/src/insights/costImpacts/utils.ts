/* eslint-disable complexity */
import { Priority, priorities } from '@willow/common/Priority'
import { caseInsensitiveEquals, cookie } from '@willow/ui'
import _ from 'lodash'
import { DateTimeFormatOptions } from 'luxon'
import { Row } from 'react-table'
import { TFunction } from 'react-i18next'
import {
  ImpactScore,
  ImpactScoreSummary,
  Insight,
  InsightPointsDto,
  PointTwinDto,
  PointType,
  Region,
  WALMART_ALERT,
} from '../insights/types'
import { getPriorityValue } from './getInsightPriority'

/**
 * This function is used to format energy value based on language and
 * convert it to nearest possible roundoff value with appropriate unit
 */
export const formatEnergy = ({
  value,
  language,
}: {
  value: number
  language: string
}) => {
  const units = ['kWh', 'MWh', 'GWh', 'TWh', 'PWh']
  let convertedValue = value
  let unitIndex = 0

  while (convertedValue >= 1000 && unitIndex < units.length - 1) {
    convertedValue /= 1000
    unitIndex += 1
  }

  return `${Intl.NumberFormat(language, {
    notation: 'compact',
    compactDisplay: 'short',
    maximumFractionDigits: 0,
  }).format(convertedValue)} ${units[unitIndex]}`
}

/**
 * If isCompact is enabled, display rounded and compact formatted number
 * (e.g., 235,456 will be displayed as 235K in English and 235 k in French)
 * Otherwise, format the numeric value with unit and adjust decimal places based on decimalPlaces prop.
 * Reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/75508
 */
export const formatValue = ({
  value,
  language,
  unit,
  isCompact = false,
  decimalPlaces = 2,
}: {
  value: number
  language: string
  unit: string
  isCompact?: boolean
  decimalPlaces?: number
}) => {
  if (isCompact) {
    return `${Intl.NumberFormat(language, {
      notation: 'compact',
      compactDisplay: 'short',
      maximumFractionDigits: 0,
    }).format(value)} ${unit}`
  } else {
    return `${Intl.NumberFormat(language, {
      maximumFractionDigits: decimalPlaces,
      minimumFractionDigits: decimalPlaces,
    }).format(value)} ${unit}`
  }
}

/**
 * receive a date time value similar to: 2022-08-11T05:15:07.794Z;
 * when language is "en", output string in format of "mm-dd-yyyy, hh:mm",
 * where hh: mm is optional; date/month omits leading "0"
 * Azure Board reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/75508
 */
export const formatDateTime = ({
  value,
  language,
  dateFormatOption = {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  },
  timeFormatOption = {
    hour: 'numeric',
    minute: 'numeric',
    // "h23" start with hour 0 and go up 23 instead of 24
    // reference: https://tc39.es/proposal-intl-datetime-style/#sec-properties-of-intl-datetimeformat-instances
    hourCycle: 'h23',
  },
  includeTime = true,
  timeZone = 'utc',
}: {
  value?: string
  language: string
  dateFormatOption?: DateTimeFormatOptions
  timeFormatOption?: DateTimeFormatOptions
  includeTime?: boolean
  timeZone?: string
}) => {
  if (value != null) {
    const parsedValue = Date.parse(value)

    const dateFormat = new Intl.DateTimeFormat(language, {
      ...dateFormatOption,
      timeZone,
    })

    const timeFormat = new Intl.DateTimeFormat(language, {
      ...timeFormatOption,
      timeZone,
    })

    const formattedDateTime = dateFormat
      .format(parsedValue)
      .replaceAll('/', '-')

    return `${formattedDateTime}${
      includeTime ? `, ${timeFormat.format(parsedValue)}` : ''
    }`
  }

  return ''
}

/**
 * a single impactScore coming from insight.impactScores array
 * has type of { name: string, value: number, unit: string }
 * where name is dynamic. PM had decided to display the values in
 * single site dashboard, insight details modal, and insights table
 * only when name matches one of the following.
 *
 * Reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/75451
 */
export enum InsightCostImpactPropNames {
  dailyAvoidableCost = 'Daily Avoidable Cost',
  dailyAvoidableEnergy = 'Daily Avoidable Energy',
  dailyCostSavings = 'Daily Cost Savings',
  dailyEnergySavings = 'Daily Energy Savings',
  totalCostToDate = 'Total Cost to Date',
  totalEnergyToDate = 'Total Energy to Date',
  yearlyAvoidableEnergy = 'Yearly Avoidable Energy',
  yearlyAvoidableCost = 'Yearly Avoidable Cost',
  priorityScore = 'Priority',
  avoidableCostPerYear = 'Avoidable Cost per Year',
  avoidableEnergyPerYear = 'Avoidable Energy per Year',

  // the following are calculated filed names used in grouped table
  yearlyAvoidableCostValue = 'yearlyAvoidableCostValue',
  totalCostToDateValue = 'totalCostToDateValue',
  yearlyAvoidableEnergyValue = 'yearlyAvoidableEnergyValue',
  totalEnergyToDateValue = 'totalEnergyToDateValue',
}

/**
 * returns a sort function to be used as the "sortType" of
 * a column object which is used by react-table's useTable call
 */
export const sortImpactCost =
  (scoreName: string) =>
  (rowA: Row, rowB: Row, _columnId: string, desc: boolean) => {
    const scoreA = rowA.original?.impactScores?.find(
      (impactScore) => impactScore?.name === scoreName
    )?.value
    const scoreB = rowB.original?.impactScores?.find(
      (impactScore) => impactScore?.name === scoreName
    )?.value

    // do not sort when both scores are undefined or same value
    if ((scoreA == null && scoreB == null) || scoreA === scoreB) return 0

    // place a defined value before undefined value regardless of sorting order
    if (scoreB == null && scoreA != null) return desc ? 1 : -1
    if (scoreA == null && scoreB != null) return desc ? -1 : 1

    return scoreA > scoreB ? 1 : -1
  }

export const getPriorityByRange = (
  value: number | undefined
): Priority | null => {
  if (value === undefined) {
    // for data does not have priority nor impactScore
    return null
  }

  if (!_.inRange(value, 0, 101)) {
    // eslint-disable-next-line no-console
    console.error(`Priority not found for value: ${value}`)
  }

  if (value <= 25) {
    return priorities[3]
  } else if (value <= 50) {
    return priorities[2]
  } else if (value <= 75) {
    return priorities[1]
  } else {
    return priorities[0]
  }
}

/**
 * a sort function to be used as the "sortType" of priority column;
 * sort on row.values.priority if it is a number between 0 and 100
 * otherwise, convert row.original.priority, which is a number between 1 and 4,
 * to a number between 0 and 100 and sort on that number; note that 1 will be
 * converted to 100, 2 to 75, 3 to 50, and 4 to 25
 */
export const sortPriority = () => (rowA: Row, rowB: Row) => {
  const getPriorityFromRow = (row: Row) =>
    getPriorityValue({
      impactScorePriority: row.values?.priority,
      insightPriority: row.original.priority,
    })

  // With fallback value Infinity, it will sort priority as
  // Critical -> Low -> '--'
  // or
  // '--' -> Low -> Critical
  const priorityA = getPriorityFromRow(rowA) ?? -Infinity
  const priorityB = getPriorityFromRow(rowB) ?? -Infinity
  if (!Number.isFinite(priorityA) && !Number.isFinite(priorityB)) {
    return 0
  }

  return Math.sign(priorityA - priorityB)
}

// this util function returns a custom sort function to be used
// by Insight Table's useColumns hook's "sortType" property;
// the returned function first converts different rows' occurredDate
// in form of "Oct 5, 2023, 17:37" into a number and
// then compare the numbers to sort
export const sortOccurrenceDate = () => (rowA: Row, rowB: Row) =>
  new Date(rowA.original.occurredDate).valueOf() >
  new Date(rowB.original.occurredDate).valueOf()
    ? 1
    : -1

export const getImpactScore = ({
  impactScores,
  scoreName,
  language,
  multiplier = 1,
  decimalPlaces = 2,
}: {
  impactScores?: Array<{ name?: string; value: number; unit?: string }>
  scoreName: string
  language: string
  multiplier?: number
  decimalPlaces?: number
}): string => {
  const impactScoreItem = (impactScores ?? []).find(
    (impactScore) => impactScore?.name === scoreName
  )

  const region: Region = cookie.get('api')

  let unit = ''

  // priorityScore doesn't need unit, but the data sometimes is not accurate
  if (impactScoreItem?.name !== InsightCostImpactPropNames.priorityScore) {
    unit = impactScoreItem?.unit ?? ''

    // Replace $ with USD for USA region, Replace $ with AUD for AUS region
    // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/92288
    if (unit === '$' && region === 'au') {
      unit = 'AUD'
    } else if (unit === '$' && region === 'us') {
      unit = 'USD'
    }
  }

  if (
    !impactScoreItem ||
    impactScoreItem.value == null ||
    // Priority score of 0 is valid and means low priority;
    // But an energy consumption score of 0 or cost of 0 is meaningless so
    // we display "--"
    (impactScoreItem.name !== InsightCostImpactPropNames.priorityScore &&
      impactScoreItem.value === 0)
  ) {
    return '--'
  } else {
    return formatValue({
      value: multiplier * impactScoreItem.value,
      unit,
      language,
      decimalPlaces,
    })
  }
}

/**
 * returns the total impact score along with unit and excludes priority score values.
 * This function also calculates yearly available cost and energy based on their daily values
 */
export const getRollUpTotalImpactScores = ({
  insight,
  timeZone,
  t,
  language,
  firstOccurredDate,
}: {
  insight: Insight
  timeZone: string
  t: TFunction
  language: string
  firstOccurredDate: string
}) => {
  const isInsightResolved = insight.lastStatus === 'resolved'
  return (insight.impactScores ?? []).map((impactScore: ImpactScore) => {
    let multiplier = 1
    let tooltip
    let name
    let suffix = ''
    let hidden = false

    switch (impactScore.name) {
      case InsightCostImpactPropNames.dailyAvoidableEnergy:
      case InsightCostImpactPropNames.dailyAvoidableCost: {
        const isDailyAvoidableCost =
          impactScore.name === InsightCostImpactPropNames.dailyAvoidableCost
        multiplier = 365
        name = isInsightResolved
          ? t('interpolation.impactSavings', {
              impact: isDailyAvoidableCost
                ? t('plainText.cost')
                : t('plainText.energy'),
            })
          : _.startCase(
              t('interpolation.avoidableExpensePerYear', {
                expense: isDailyAvoidableCost
                  ? t('plainText.cost')
                  : t('plainText.energy'),
              })
            )
        tooltip = isInsightResolved
          ? t('plainText.savingsPerYearTooltip')
          : t('interpolation.savingsPerYear', {
              item: isDailyAvoidableCost
                ? t('plainText.cost').toLowerCase()
                : t('plainText.energyUsage').toLowerCase(),
            })
        suffix = isInsightResolved
          ? t('plainText.totalPerYear')
          : t('plainText.perYear')
        break
      }
      case InsightCostImpactPropNames.totalEnergyToDate:
      case InsightCostImpactPropNames.totalCostToDate: {
        hidden = isInsightResolved
        name = t('interpolation.expenseToDate', {
          expense:
            impactScore.name === InsightCostImpactPropNames.totalCostToDate
              ? t('plainText.cost')
              : t('plainText.energy'),
        })
        tooltip = t('interpolation.savingsToDate', {
          item:
            impactScore.name === InsightCostImpactPropNames.totalCostToDate
              ? t('plainText.cost').toLowerCase()
              : t('plainText.energy'),
        })
        suffix = t('interpolation.sinceWhen', {
          when: formatDateTime({
            value: firstOccurredDate,
            language,
            timeZone,
            includeTime: false,
          }),
        })
        break
      }
      // Hiding priority details since we are not considering it from impactScores data
      case InsightCostImpactPropNames.priorityScore: {
        hidden = true
        break
      }
      default: {
        name = impactScore.name
        break
      }
    }
    // Checking cost and energy values
    const isEnergy = caseInsensitiveEquals(impactScore.unit, 'kwh')
    const isCost = caseInsensitiveEquals(impactScore.unit, 'usd')

    return {
      name,
      value:
        impactScore.value == null || impactScore.value === 0
          ? `-- ${_.startCase(suffix)}`
          : isEnergy
          ? `${formatEnergy({
              value: multiplier * impactScore.value,
              language,
            })} ${_.startCase(suffix)}`
          : `${formatValue({
              value: multiplier * impactScore.value,
              unit: impactScore.unit ?? '',
              language,
              decimalPlaces: isCost ? 0 : 2,
              isCompact: isCost,
            })} ${_.startCase(suffix)}`,
      tooltip,
      hidden,
    }
  })
}

/**
 * returns total impact score along with unit when isRollUpTotal
 * is passed else it returns the max cost of insight of grouped
 * insights at rule level based on score name
 */
export const getTotalImpactScore = ({
  groupInsights,
  scoreName,
  language,
  multiplier = 1,
  isRollUpTotal = false,
  decimalPlaces = 2,
}: {
  groupInsights: Insight[]
  scoreName: string
  language: string
  multiplier?: number
  isRollUpTotal?: boolean
  decimalPlaces?: number
}) => {
  let value: number | undefined
  let unit: string | undefined

  const region: Region = cookie.get('api')

  groupInsights.forEach((groupInsight: Insight) => {
    const scoreData = (groupInsight?.impactScores ?? []).find(
      (impactScore) => impactScore?.name === scoreName
    )
    if (unit == null || unit === '') {
      unit = scoreData?.unit
    }

    // Replace $ with USD for USA region, Replace $ with AUD for AUS region
    // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/92288
    if (unit === '$' && region === 'au') {
      unit = 'AUS'
    }

    if (unit === '$' && region === 'us') {
      unit = 'USD'
    }

    if (value == null) {
      value = scoreData?.value
    } else if (isRollUpTotal) {
      value = (scoreData?.value ?? 0) + value
    } else {
      value = Math.max(value, scoreData?.value ?? 0)
    }
  })

  // Checking if the value is kwh
  const isEnergy = caseInsensitiveEquals(unit, 'kwh')

  return {
    totalImpactScore:
      value == null || value === 0
        ? '--'
        : isEnergy && isRollUpTotal && decimalPlaces === 0
        ? formatEnergy({
            value: multiplier * value,
            language,
          })
        : formatValue({
            value: multiplier * value,
            unit: unit ?? '',
            language,
            isCompact: isRollUpTotal && decimalPlaces === 0,
            decimalPlaces,
          }),
    value,
    unit,
  }
}

export const getTotalImpactScoreSummary = ({
  impactScores,
  scoreName,
  language,
  multiplier = 1,
  isRollUpTotal = false,
  decimalPlaces = 2,
}: {
  impactScores: ImpactScoreSummary[]
  scoreName: string
  language: string
  multiplier?: number
  isRollUpTotal?: boolean
  decimalPlaces?: number
}) => {
  let value: number | undefined
  let unit: string | undefined

  const region: Region = cookie.get('api')

  const scoreData = (impactScores ?? []).find(
    (impactScore) => impactScore?.name === scoreName
  )
  if (unit == null || unit === '') {
    unit = scoreData?.unit
  }

  // Replace $ with USD for USA region, Replace $ with AUS for AUS region
  // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/92288
  if (unit === '$' && region === 'au') {
    unit = 'AUD'
  }

  if (unit === '$' && region === 'us') {
    unit = 'USD'
  }

  if (value == null) {
    value = scoreData?.value
  } else if (isRollUpTotal) {
    value = (scoreData?.value ?? 0) + value
  } else {
    value = Math.max(value, scoreData?.value ?? 0)
  }

  // Checking if the value is kwh
  const isEnergy = caseInsensitiveEquals(unit, 'kwh')

  return {
    totalImpactScore:
      value == null || value === 0
        ? '--'
        : isEnergy && isRollUpTotal && decimalPlaces === 0
        ? formatEnergy({
            value: multiplier * value,
            language,
          })
        : formatValue({
            value: multiplier * value,
            unit: unit ?? '',
            language,
            isCompact: isRollUpTotal && decimalPlaces === 0,
            decimalPlaces,
          }),
    value,
    unit,
  }
}

/**
 *  filterMap is used to replace labels of checkboxes as part of insights V3
 */
export const filterMap = new Map()
filterMap.set('inspection', 'plainText.willowInspection')

/**
 * to be used as select function when querying for insight points
 */
export const getInsightPoints = (data: InsightPointsDto) => {
  const getUniquePoints = ({
    points,
    type,
  }: {
    points?: PointTwinDto[]
    type: PointType
  }) =>
    // there is 2 types of points, insight points and impact score points
    // and according to instruction from BE, we need to fetch insight points' live data
    // from /sites/{siteId}/points/{pointId)/livedata with point.trendId,
    // and fetch live data for impact score points from
    // /sites/{siteId}/livedata/impactScores/{externalId} with point.externalId
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/85071
    {
      const isInsightPoint = type === PointType.InsightPoint
      return points
        ? _.uniqBy(points, isInsightPoint ? 'trendId' : 'externalId')
            ?.map<PointTwinDto>((point, index) => ({
              ...point,
              entityId:
                // TODO: to move away from using point.trendId once following BE story is complete
                // https://dev.azure.com/willowdev/Unified/_workitems/edit/87350
                isInsightPoint ? point.trendId : point.externalId,
              externalPointId: point.externalId,
              type,
              defaultOn: isInsightPoint,
            }))
            .filter((point) => point.entityId != null)
        : undefined
    }

  return {
    insightPoints: getUniquePoints({
      points: data.insightPoints,
      type: PointType.InsightPoint,
    }),
    impactScorePoints: getUniquePoints({
      points: data.impactScorePoints,
      type: PointType.ImpactScorePoint,
    }),
  }
}

export const checkIsWalmartAlert = (ruleId) =>
  caseInsensitiveEquals(ruleId, WALMART_ALERT)
