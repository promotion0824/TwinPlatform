import { ProviderRequiredError } from '@willow/common'
import { ParamsDict } from '@willow/common/hooks/useMultipleSearchParams'
import { Site } from '@willow/common/site/site/types'
import {
  DashboardReportCategory,
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
} from '@willow/ui'
import { createContext, useContext } from 'react'

import { EmbedGroup } from '../../components/Reports/ReportsLayout'
import { AllSites } from '../../providers/sites/SiteContext'

export type Portfolio = {
  baseMapSites: Site[]
  selectedBuilding: Site | AllSites
  selectedSite: Site
  selectedCountry: string
  selectedStates: string[]
  selectedLocation: string[]
  selectedTypes: string[]
  selectedStatuses: string[]
  setShouldFilterByMap: (shouldFilterByMap: boolean) => void
  setShouldResetMapBounds: (shouldResetMapBounds: boolean) => void
  search: string
  shouldFilterByMap: boolean
  shouldResetMapBounds: boolean
  submenuCategory: DashboardReportCategory
  sites: Site[]
  filteredSites: Site[]
  filteredSiteIds: string[]
  buildingScores: Array<{
    comfort?: number
    energy?: number
    siteId?: string
    performance?: number
  }>
  isBuildingScoresLoading: boolean
  isDefaultFilter: () => boolean
  quickOptionSelected: string
  dateRange: [string, string]
  category: DashboardReportCategory
  selectedDashboard?: string
  selectSite: (site?: Site) => void
  setSubmenuCategory: (category: DashboardReportCategory) => void
  toggleBuilding: (site: Site | AllSites) => void
  toggleCountry: (country: string) => void
  toggleState: (state: string) => void
  toggleLocation: (location: string, locationLevel?: number) => void
  toggleType: (siteType: string) => void
  toggleStatus: (status: string) => void
  setSearch: (newSearch: string) => void
  setSearchParams: (params: ParamsDict) => void
  setSiteIdsOnMap: (siteIds: string[]) => void
  handleDashboardReportClick: (
    dashboard: EmbedGroup,
    analyticsKey: string
  ) => void
  handleQuickOptionClick: (quickOption: string) => void
  handleQuickOptionChange?: (quickOptionSelected: string) => void
  handleDateRangeChange: (params: ParamsDict) => void
  selectedDayRange?: DatePickerDayRangeOptions
  selectedBusinessHourRange?: DatePickerBusinessRangeOptions
  handleDayRangeChange?: (selectedDayRange: DatePickerDayRangeOptions) => void
  handleBusinessHourRangeChange?: (
    selectedBusinessHourRange: DatePickerBusinessRangeOptions
  ) => void
  handleResetClick?: () => void
  handleBusinessHourChange: (
    selectedBusinessHourRange: DatePickerBusinessRangeOptions
  ) => void
  handleReportSelection: (report: EmbedGroup) => void
  resetFilters: () => void
  updateSearch: (newSearch: string) => void
  handleResetMapClick: () => void
}

export const PortfolioContext = createContext<Portfolio | undefined>(undefined)

export function usePortfolio() {
  const context = useContext(PortfolioContext)
  if (context == null) {
    throw new ProviderRequiredError('Porfolio')
  }
  return context
}
