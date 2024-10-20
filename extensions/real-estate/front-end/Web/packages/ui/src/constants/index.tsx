// App views
export const ALL_LOCATIONS = 'All Locations' as const
export const ALL_SITES = 'All Sites' // an option picked by user on SiteSelect dropdown when viewing portfolio as a whole
export const PORTFOLIO = 'Portfolio'
/**
 * the following string enums are the names of Dashboard Report Categories
 * that will be used across the applications and each string enum represents
 * a type of Dashboard which we call "Category", please refer to:
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/68410
 *
 */
export enum DashboardReportCategory {
  OPERATIONAL = 'Operational',
  DATA_QUALITY = 'Data Quality', // Data Quality page by tab selection on Dashboard page
  OCCUPANCY = 'Occupancy',
  MANAGEMENT = 'Management',
  SUSTAINABILITY = 'Sustainability',
  TENANT = 'Tenant',
  SAVINGS = 'Savings',
  PRE_OPERATIONAL = 'Pre-Operational',
}

export const VIEW = 'view'

export enum InsightGroups {
  NONE = 'None',
  RULE = 'Rule',
  ASSET_TYPE = 'AssetType',
}

export enum InsightSorts {
  NEWEST = 'Newest',
  OLDEST = 'Oldest',
}

export enum DatePickerDayRangeOptions {
  ALL_DAYS_KEY = 'allDays',
  WEEK_DAYS_KEY = 'weekDays',
  WEEK_ENDS_KEY = 'weekEnds',
}

export enum DatePickerBusinessRangeOptions {
  ALL_HOURS_KEY = 'allHours',
  IN_BUSINESS_HOURS_KEY = 'inBusinessHours',
  OUT_BUSINESS_HOURS_KEY = 'outBusinessHours',
}
export enum InsightStatus {
  OPEN = 'open',
  ACKNOWLEDGED = 'acknowledged',
  CLOSED = 'closed',
}

export const InsightWorkflowTabName = [
  { label: 'active', value: InsightStatus.OPEN },
  { label: 'resolved', value: InsightStatus.CLOSED },
  { label: 'ignored', value: InsightStatus.ACKNOWLEDGED },
]

export const InsightImpactMetricsTabName = [
  { label: 'open', value: InsightStatus.OPEN },
  { label: 'acknowledged', value: InsightStatus.ACKNOWLEDGED },
  { label: 'closed', value: InsightStatus.CLOSED },
]

export const FILTER_PANEL_BREAKPOINT = 1200 as const
