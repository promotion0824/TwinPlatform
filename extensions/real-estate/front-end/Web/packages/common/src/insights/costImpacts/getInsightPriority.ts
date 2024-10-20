import _, { isFinite, toNumber } from 'lodash'
import { InsightPriority } from '../insights/types'
import { InsightCostImpactPropNames, getImpactScore } from './utils'

export const calculatePriority = ({
  impactScores,
  language,
  insightPriority,
}: {
  impactScores?: { name?: string; value: number; unit?: string }[]
  language: string
  insightPriority?: InsightPriority
}) =>
  getPriorityValue({
    impactScorePriority: getImpactScore({
      impactScores,
      scoreName: InsightCostImpactPropNames.priorityScore,
      language,
    }),
    insightPriority,
  })

/**
 * Returns the impactScorePriority value [0, 100] if provided.
 * Or convert the legacy priority [1, 4] into the same scale of
 * impactScorePriority as a fallback priority.
 */
export const getPriorityValue = ({
  impactScorePriority,
  insightPriority,
}: {
  impactScorePriority?: string | number
  insightPriority?: InsightPriority
}): number | undefined => {
  if (impactScorePriority !== undefined && isValidScore(impactScorePriority)) {
    return _.clamp(_.toNumber(impactScorePriority), 0, 100)
  }

  if (_.isNumber(insightPriority)) {
    // for insights from 3rd parties that do not have impact score priority, we use priority as a fallback;
    // priority of 1 is mapped to 100 which is in range 76 - 100 and displayed as 'Critical' in the UI;
    // priority of 2 is mapped to 75 which is in range 51 - 75 and displayed as 'High' in the UI;
    // priority of 3 is mapped to 50 which is in range 26 - 50 and displayed as 'Medium' in the UI;
    // priority of 4 is mapped to 25 which is in range 0 - 25 and displayed as 'Low' in the UI;
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80910

    // to handle unexpected out of range priority value
    return (5 - _.clamp(insightPriority, 1, 5)) * 25
  }

  // for any data has neither impactScore nor priority
  return undefined
}

/**
 * Checks if a number is finite that cannot be NaN or Infinite or -Infinite;
 * or if a string can be converted as a number that is a finite number,
 * includes decimals.
 * Empty string is considered as invalid.
 */
export const isValidScore = (value: string | number) => {
  if (isFinite(value)) {
    return true
  }
  if (value === '') {
    return false
  }

  return isFinite(toNumber(value))
}
