import { BadgeProps } from '@willowinc/ui'
import { Dispatch, SetStateAction } from 'react'
import { RenderMetricObject } from './ConnectivityMetric'

export type PortfolioParam = { portfolioId: string }

export type Portfolio = {
  features: Record<string, boolean>
  id: string
  name: string
  siteCount: number
}

export type SelectedTab = string
export type SetSelectedTab = Dispatch<string>
export type Filters = {
  // The following variables are the available options to choose from
  search: string
  countries: string[]
  states: string[]
  cities: string[]
  assetClasses: string[]
  // The following variables are options the users has chosen
  selectedCountry: string | null
  selectedStates: string[]
  selectedCities: string[]
  selectedAssetClasses: string[]
}
export type SetFilters = Dispatch<SetStateAction<Filters>>
export type ClearFilters = () => void
export type HasFiltersChanged = () => boolean

export type ConnectivityTableState = {
  isLoading: boolean
  isError: boolean
  isSuccess: boolean
}

type Connectivity = {
  assetClass: string
  city: string
  connectorStatus: string
  country: string
  dataIn: number
  isOnline: boolean
  name: string
  state: string
  color?: BadgeProps['color']
}

export type ConnectivityTableData = Partial<Connectivity>[]

export type ConnectivityContextType = {
  portfolioName: string
  renderMetricObject: RenderMetricObject
  connectivityTableData: ConnectivityTableData
  connectivityTableState: ConnectivityTableState
  selectedTab: SelectedTab
  setSelectedTab: SetSelectedTab

  filters: Filters
  setFilters: SetFilters
  clearFilters: ClearFilters
  hasFiltersChanged: HasFiltersChanged
}
