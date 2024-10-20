import { WeatherbitCode, WeatherbitIconCode } from './weatherbitTypes'

export type SiteFeatures = {
  isTicketingDisabled: boolean
  isInsightsDisabled: boolean
  is2DViewerDisabled: boolean
  isReportsEnabled: boolean
  is3DAutoOffsetEnabled: boolean
  isInspectionEnabled: boolean
  isOccupancyEnabled: boolean
  isPreventativeMaintenanceEnabled: boolean
  isCommandsEnabled: boolean
  isScheduledTicketsEnabled: boolean
  isNonTenancyFloorsEnabled: boolean
  isHideOccurrencesEnabled: boolean
  isArcGisEnabled: boolean
}

export type SiteWeather = {
  /** Weatherbit description code. */
  code: WeatherbitCode
  /** Weatherbit icon code. */
  icon: WeatherbitIconCode
  /** Temperature in celsius. */
  temperature: number
}

/** Insight counts by priority */
export interface InsightStats {
  openCount: number
  urgentCount: number
  highCount: number
  mediumCount: number
  lowCount: number
}

/** Ticket counts by a mix of priority and status. */
export type TicketStats = {
  overdueCount: number
  urgentCount: number
  highCount: number
  mediumCount: number
  lowCount: number
  openCount: number
}

/**
 * Ticket counts by status.
 *
 * These status counts will be computed as summary counts on the backend.
 * They will consistently correspond to each type defined as TicketStatus['tab'].
 */
export interface TicketStatsByStatus {
  closedCount: number
  openCount: number
  resolvedCount: number
}

export type Site = {
  id: string
  portfolioId?: string
  name: string
  code: string
  suburb: string
  address: string
  state: string
  postcode: string
  country: string
  numberOfFloors: number
  logoUrl: string
  logoOriginalSizeUrl: string
  latitude?: number
  longitude?: number
  timeZoneId: string
  area: string
  type: string
  status: string
  userRole: string
  timeZone: string
  features: SiteFeatures
  settings: {
    inspectionDailyReportWorkgroupId?: string
  }
  isOnline?: boolean
  constructionYear?: number
  siteCode: string
  siteContactName: string
  siteContactEmail: string
  siteContactTitle: string
  siteContactPhone: string
  webMapId: string
  createdDate?: string
  ticketStats: TicketStats
  ticketStatsByStatus?: TicketStatsByStatus
  insightsStats: InsightStats
  insightsStatsByStatus?: {
    ignoredCount: number
    inProgressCount: number
    newCount: number
    openCount: number
    resolvedCount: number
  }
  weather?: SiteWeather
}

export const siteAdminUserRole = 'admin'

/** structure of each site returned by /v2/me/sites */
export type PagedSite = {
  id: string
  portfolioId?: string
  name?: string
  code?: string
  numberOfFloors: number
  logoUrl?: string
  logoOriginalSizeUrl?: string
  latitude?: number
  longitude?: number
  timeZoneId?: string
  status?: string
  timeZone?: string
  features: {
    isTicketingDisabled: boolean
    isInsightsDisabled: boolean
    is2DViewerDisabled: boolean
    isReportsEnabled: boolean
    is3DAutoOffsetEnabled: boolean
    isInspectionEnabled: boolean
    isOccupancyEnabled: boolean
    isPreventativeMaintenanceEnabled: boolean
    isCommandsEnabled: boolean
    isScheduledTicketsEnabled: boolean
    isNonTenancyFloorsEnabled: boolean
    isHideOccurrencesEnabled: boolean
    isArcGisEnabled: boolean
  }
  weather: SiteWeather
  webMapId?: string
  arcGisLayers?: {
    id?: string
    title?: string
    type?: string
    url?: string
  }[]

  // those three is not in schema at the moment but expected in current code.
  // BE is working on adding them to schema
  type?: string
  suburb?: string
  state?: string
  insightsStats: InsightStats
  insightsStatsByStatus?: {
    ignoredCount: number
    inProgressCount: number
    newCount: number
    openCount: number
    resolvedCount: number
  }
  ticketStats: TicketStats
  ticketStatsByStatus?: TicketStatsByStatus
}

/** Response of POST /v2/me/sites */
export type PagedSites = {
  before: number
  after: number
  total: number
  items: PagedSite[]
}

export type PagedSiteResult = PagedSite & { location?: [number, number] }
