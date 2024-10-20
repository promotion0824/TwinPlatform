import { formatDateTime } from '@willow/common'
import { caseInsensitiveEquals, useDateTime } from '@willow/ui'
import _ from 'lodash'
import { TFunction } from 'react-i18next'
import { InsightCostImpactPropNames } from '../insights/costImpacts/utils'
import {
  CardSummaryFilters,
  CardSummaryRule,
  ImpactScoreSummary,
  Insight,
  InsightTypesGroupedByDate,
  Occurrence,
} from '../insights/insights/types'
import { Site } from '../site/site/types'

// many existing insights have ruleId and primaryModelId of empty string,
// by adding empty string to insights without ruleId and primaryModelId
// we can ensure to group these 2 types of insights under
// same 'Ungrouped' group;
// it is used as select prop in useGetInsights, useGetAssetInsights hooks
// reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/79346
export default function selectInsights(data: Insight[], sites: Site[]) {
  return data?.map((insight) => ({
    ...insight,
    ruleId: insight?.ruleId ?? '',
    primaryModelId: insight?.primaryModelId ?? '',
    equipmentId: insight?.equipmentId ?? '',
    // default to updated date when lastResolvedDate and lastIgnoredDate are not defined
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/85439
    lastResolvedDate: insight?.lastResolvedDate ?? insight.updatedDate,
    lastIgnoredDate: insight?.lastIgnoredDate ?? insight.updatedDate,
    timeZone: sites.find((s) => s.id === insight?.siteId)?.timeZone,
  }))
}

// Filter impactScoreSummary....
// No longer reading from "impactScoreSummary" from API
// Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/92289
export function filterImpactScoreSummary(filteredRules: CardSummaryRule[]) {
  let dailyAvoidableCost = {
    fieldId: 'daily_avoidable_cost',
    name: InsightCostImpactPropNames.dailyAvoidableCost,
    value: 0,
    unit: 'USD',
  }
  let dailyAvoidableEnergy = {
    fieldId: 'daily_avoidable_energy',
    name: InsightCostImpactPropNames.dailyAvoidableEnergy,
    value: 0,
    unit: 'kWh',
  }
  let dailyCostSavings = {
    fieldId: 'daily_cost_savings',
    name: InsightCostImpactPropNames.dailyCostSavings,
    value: 0,
    unit: 'USD',
  }
  let dailyEnergySavings = {
    fieldId: 'daily_energy_savings',
    name: InsightCostImpactPropNames.dailyEnergySavings,
    value: 0,
    unit: 'kWh',
  }

  filteredRules.forEach((item) => {
    item.impactScores?.forEach((impactItem) => {
      if (impactItem.name === InsightCostImpactPropNames.dailyAvoidableCost) {
        dailyAvoidableCost = {
          ...dailyAvoidableCost,
          value: dailyAvoidableCost.value + impactItem.value,
        }
      }
      if (impactItem.name === InsightCostImpactPropNames.dailyAvoidableEnergy) {
        dailyAvoidableEnergy = {
          ...dailyAvoidableEnergy,
          value: dailyAvoidableEnergy.value + impactItem.value,
        }
      }

      if (impactItem.name === InsightCostImpactPropNames.dailyCostSavings) {
        dailyCostSavings = {
          ...dailyCostSavings,
          value: dailyCostSavings.value + impactItem.value,
        }
      }

      if (impactItem.name === InsightCostImpactPropNames.dailyEnergySavings) {
        dailyEnergySavings = {
          ...dailyEnergySavings,
          value: dailyEnergySavings.value + impactItem.value,
        }
      }
    })
  })

  const res = [
    dailyAvoidableCost,
    dailyAvoidableEnergy,
    dailyCostSavings,
    dailyEnergySavings,
  ]

  return res
}

// Grouping of insightTypes into specific date based buckets for quick display.....
export function selectInsightTypes(
  cards: CardSummaryRule[],
  filters: CardSummaryFilters,
  impactScoreSummary: ImpactScoreSummary[],
  dateTime: ReturnType<typeof useDateTime>,
  t: TFunction
) {
  const res: InsightTypesGroupedByDate = []

  // Hiding diagnostic insights from the card and table view
  // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/126614
  const filteredCards = cards.filter(
    (item) => !caseInsensitiveEquals(item.insightType, 'diagnostic')
  )

  const durations = [
    { label: t('plainText.last24Hours'), min: -1, max: 1 },
    { label: t('plainText.last7Days'), min: 1, max: 7 },
    { label: t('plainText.last30Days'), min: 7, max: 30 },
    { label: t('plainText.lastYear'), min: 30, max: 365 },
    { label: t('plainText.lastTwoYears'), min: 365, max: 730 },
    {
      label: t('interpolation.moreThanNumberOfYears', { number: 2 }),
      min: 730,
      max: Infinity,
    },
  ]

  durations.forEach((duration) => {
    res.push({
      title: duration.label,
      // Sort each of the cards in descending order for each duration (Last 24 Hours, last 7 Days, etc)
      insightTypes: filteredCards
        .sort(
          (a, b) =>
            new Date(b?.lastOccurredDate ?? 0).getTime() -
            new Date(a?.lastOccurredDate ?? 0).getTime()
        )
        .filter(
          (item) =>
            dateTime.now().differenceInDays(item?.lastOccurredDate) >
              duration.min &&
            dateTime.now().differenceInDays(item?.lastOccurredDate) <=
              duration.max
        ),
    })
  })

  return {
    insightTypesGroupedByDate: res,
    cards: filteredCards,
    filters,
    impactScoreSummary,
  }
}

type NumericDateOccurrence = Omit<Occurrence, 'started' | 'ended'> & {
  started: number
  ended: number
}
/**
 * Occurrence data returned by the following endpoint:
 * `/sites/${siteId}/insights/${insightId}/occurrences`
 * constantly run into the an overlapping issue similar to the one described here:
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/96134
 * so we need to merge the overlapping occurrences into a single occurrence
 */
export const selectOccurrences = (data: Occurrence[]) => {
  const occurrencesWithType: NumericDateOccurrence[] = data.map((d) => {
    let type = 'healthy'
    if (d.isFaulted && d.isValid) {
      type = 'faulty'
    } else if (!d.isValid) {
      type = 'insufficient'
    }
    return {
      ...d,
      type,
      // started and ended are in ISO string format, we need to convert them to number
      // in order compare them effectively
      started: new Date(d.started).valueOf(),
      ended: new Date(d.ended).valueOf(),
    }
  })

  // group occurrences by type so we can merge them within the same type
  const groupedOccurrences = _.groupBy(occurrencesWithType, 'type')

  /**
   * each occurrence has started and ended value in number of milliseconds since 1970-01-01T00:00:00Z,
   * and mergeOccurrenceOnIntervals will merge the occurrences that are overlapping, so for example:
   * [
   *  { started: 1, ended: 3, ...rest }, // Occurrence 1
   *  { started: 2, ended: 4, ...rest }, // Occurrence 2
   * ]
   * will be merged into a single occurrence:
   * [
   *  { started: 1, ended: 4, ...rest }, // Occurrence 1
   * ]
   */
  const mergeOccurrenceOnIntervals = (
    occurrences: NumericDateOccurrence[]
  ): Occurrence[] => {
    const sortedOccurrences = occurrences.sort((a, b) => a.started - b.started)
    const mergedOccurrences = sortedOccurrences.length
      ? [sortedOccurrences[0]]
      : []
    for (let i = 1; i < sortedOccurrences.length; i++) {
      const currentOccurrence = sortedOccurrences[i]
      const previousOccurrence = mergedOccurrences[mergedOccurrences.length - 1]
      if (currentOccurrence.started <= previousOccurrence.ended) {
        previousOccurrence.ended = Math.max(
          previousOccurrence.ended,
          currentOccurrence.ended
        )
      } else {
        mergedOccurrences.push(currentOccurrence)
      }
    }
    return mergedOccurrences.map((o) => ({
      ...o,
      // convert started and ended back to ISO string format
      // as that is what the Occurrence component expects
      // TODO: improve this section to avoid converting back and forth between ISO string and number
      // https://dev.azure.com/willowdev/Unified/_workitems/edit/96251
      started: new Date(o.started).toISOString(),
      ended: new Date(o.ended).toISOString(),
    }))
  }

  return _.flatten(
    Object.values(groupedOccurrences).map((group) =>
      mergeOccurrenceOnIntervals(group)
    )
  )
}

/**
 * This functions takes array of occurrences and returns array of faulty occurrences
 * in form of { start: string, end: string, value: string, label: string } where
 * start/end are in UTC date time format, value is a string in format of `${start} - ${end}`
 * and label is a string in format of `${formattedStart} - ${formattedEnd}` and
 * formattedStart and formattedEnd are formatted date time strings according to the language and timeZone
 */
export const makeFaultyTimes = ({
  occurrences,
  language,
  timeZone,
}: {
  occurrences: Occurrence[]
  language: string
  timeZone?: string
}) =>
  _.uniqBy(
    _.orderBy(
      occurrences
        .filter(
          (occurrence: Occurrence) => occurrence.isFaulted && occurrence.isValid
        )
        .map(({ started, ended }) => ({
          start: started,
          end: ended,
          value: `${started} - ${ended}`,
          label: `${formatDateTime({
            value: started,
            language,
            timeZone,
          })} - ${formatDateTime({
            value: ended,
            language,
            timeZone,
          })}`,
        })),
      (o) => new Date(o.start).valueOf(),
      'desc'
    ),
    'value'
  )
