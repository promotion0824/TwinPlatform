import _ from 'lodash'
import {
  Activity,
  ActivityKey,
  ActivityType,
  InsightWorkflowActivity,
  SortBy,
} from '../insights/insights/types'

/**
 * This function returns the initials of your full name
 */
const getFullNameInitials = (fullName: string) =>
  _.chain(fullName)
    .words()
    .take(2)
    .map((word) => word.charAt(0))
    .join('')
    .upperCase()
    .value()

/**
 * This function with reorder the insight activities sub fields
 */
const reorderInsightActivity = (activities: Activity[]) => {
  const desiredOrder = [
    ActivityKey.PreviouslyIgnored,
    ActivityKey.PreviouslyResolved,
    ActivityKey.Status,
    ActivityKey.Priority,
    ActivityKey.ImpactScores,
    ActivityKey.OccurrenceStarted,
  ]
  return _.sortBy(activities, (activity) =>
    _.indexOf(desiredOrder, activity.key)
  )
}

/**
 * Helper function to get time from ISO date string
 */
const parseISODate = (isoDate: string): number => new Date(isoDate).getTime()

/**
 * This function groups attachments and merged them into one activity if it is uploaded
 * by the same user for the same ticket within a minute
 * For other activity type, the object remains unchanged
 */
const groupAttachments = (
  activities: InsightWorkflowActivity[],
  sortBy: SortBy
) => {
  const sortedActivities = _.orderBy(activities, 'activityDate', [sortBy])

  const groupedData = sortedActivities.reduce(
    (result: InsightWorkflowActivity[], currentActivity) => {
      const previousActivity = _.last(result)

      /**
       * If the difference between the current and previous activityDate is within 1 minute,
       * merge the current activities into the previous object
       * Otherwise, push the current object as it is to the result array
       */
      if (
        previousActivity &&
        currentActivity.activityType === ActivityType.TicketAttachment &&
        currentActivity.ticketId === previousActivity.ticketId &&
        currentActivity.userId === previousActivity.userId &&
        parseISODate(currentActivity.activityDate) -
          parseISODate(previousActivity.activityDate) <=
          60000
      ) {
        previousActivity.activities.push(...currentActivity.activities)
      } else {
        result.push(currentActivity)
      }

      return result
    },
    []
  )

  return groupedData
}

export { reorderInsightActivity, getFullNameInitials, groupAttachments }
