/**
 * Insight impact values with name of one of following:
 * - Daily Avoidable Cost
 * - Daily Avoidable Energy
 * - Total Cost to date
 * - Total Energy to Date
 * will be displayed on Single Site Dashboard, Asset Details Modal, and Insights Table,
 * so only 2 types of metrics ("cost" and "energy") are relevant as per:
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/75451
 */
export enum InsightMetric {
  cost = 'cost',
  energy = 'energy',
}
