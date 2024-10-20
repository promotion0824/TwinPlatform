import { screen } from '@testing-library/react'
import { Site } from '@willow/common/site/site/types'
import { DashboardReportCategory } from '@willow/ui'
import { AllSites } from '../../../../../providers/sites/SiteContext'
import { Portfolio } from '../../../PortfolioContext'

const getMockPortfolio = ({
  selectedBuilding = sixtyMartin as Site,
  filteredSiteIds = ['id-60'],
  selectedLocation = ['Australia', 'Queensland', 'Brisbane'],
  selectedTypes = ['Aviation'],
  selectedStatuses = ['Construction'],
}: {
  selectedBuilding?: Site | AllSites
  filteredSiteIds?: string[]
  selectedLocation?: string[]
  selectedTypes?: string[]
  selectedStatuses?: string[]
}) => {
  const mockedSelectSite = jest.fn()
  const mockedToggleBuilding = jest.fn()
  const mockedToggleLocation = jest.fn()
  const mockedToggleType = jest.fn()
  const mockedToggleStatus = jest.fn()

  const portfolio: Portfolio = {
    baseMapSites: [fiftyMartin, fortyMartin, thirtyMartin],
    selectedBuilding,
    filteredSiteIds,
    selectedLocation,
    selectedTypes,
    selectedStatuses,
    selectSite: mockedSelectSite,
    setShouldFilterByMap: jest.fn(),
    setShouldResetMapBounds: jest.fn(),
    setSiteIdsOnMap: jest.fn(),
    shouldFilterByMap: false,
    shouldResetMapBounds: false,
    toggleBuilding: mockedToggleBuilding,
    toggleLocation: mockedToggleLocation,
    toggleStatus: mockedToggleStatus,
    toggleType: mockedToggleType,
    selectedSite: sixtyMartin as Site,
    selectedCountry: 'Canada',
    selectedStates: [],
    search: '',
    submenuCategory: DashboardReportCategory.OPERATIONAL,
    sites: [fiftyMartin, fortyMartin, thirtyMartin],
    filteredSites: [fiftyMartin, fortyMartin, thirtyMartin],
    buildingScores: [],
    isBuildingScoresLoading: false,
    isDefaultFilter: jest.fn(),
    quickOptionSelected: '',
    dateRange: ['2000', '2001'],
    category: DashboardReportCategory.OPERATIONAL,
    selectedDashboard: '',
    setSubmenuCategory: jest.fn(),
    toggleCountry: jest.fn(),
    toggleState: jest.fn(),
    setSearch: jest.fn(),
    setSearchParams: jest.fn(),
    handleDashboardReportClick: jest.fn(),
    handleQuickOptionClick: jest.fn(),
    handleDateRangeChange: jest.fn(),
    handleDayRangeChange: jest.fn(),
    handleBusinessHourRangeChange: jest.fn(),
    handleBusinessHourChange: jest.fn(),
    handleResetClick: jest.fn(),
    handleReportSelection: jest.fn(),
    resetFilters: jest.fn(),
    updateSearch: jest.fn(),
    handleResetMapClick: jest.fn(),
  }

  return {
    portfolio,
    mockedSelectSite,
    mockedToggleBuilding,
    mockedToggleLocation,
    mockedToggleType,
    mockedToggleStatus,
  }
}

export const sixtyMartin = {
  id: 'id-60',
  name: '60 Martin Street',
  status: 'Operations',
}
export const fiftyMartin = { id: 'id-50', name: '50 Martin Street' } as Site
export const fortyMartin = { id: 'id-40', name: '40 Martin Street' } as Site
export const thirtyMartin = { id: 'id-30', name: '30 Martin Street' } as Site

export const dataQualityReport = {
  id: 'cid-1',
  type: 'sigmaReport',
  metadata: {
    embedPath: 'url-1',
    name: 'comm-report-1',
    embedLocation: 'dashboardsTab',
    category: DashboardReportCategory.DATA_QUALITY,
    embedGroup: [
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-1',
        name: 'placeholder-1',
        order: 0,
      },
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-2',
        name: 'placeholder-2',
        order: 1,
      },
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-3',
        name: 'placeholder-3',
        order: 2,
      },
    ],
  },
}

export const tenantReport = {
  id: 'tid-1',
  type: 'sigmaReport',
  metadata: {
    embedPath: 'url-1',
    name: 'tenant-report-1',
    embedLocation: 'dashboardsTab',
    category: DashboardReportCategory.TENANT,
    embedGroup: [
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-2',
        name: 'tenant-report-1',
        order: 0,
      },
    ],
  },
}

export const reportOne = {
  id: 'id-1',
  type: 'sigmaReport',
  metadata: {
    embedPath: 'url-1',
    name: 'report-1',
    embedLocation: 'dashboardsTab',
    category: DashboardReportCategory.OPERATIONAL,
    embedGroup: [
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-11',
        name: 'dashboard-11',
        order: 0,
        tenantFilter: true,
      },
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-12',
        name: 'dashboard-12',
        order: 1,
      },
      {
        embedPath: 'https://app.sigmacomputing.com/embed/path-13',
        name: 'dashboard-13',
        order: 2,
      },
    ],
  },
}
export const reportTwo = {
  id: 'id-2',
  type: 'sigmaReport',
  metadata: {
    embedPath: 'url-2',
    name: 'report-2',
    embedGroup: [
      {
        embedPath: 'path-2',
        name: 'dashboard-2',
        order: 2,
      },
    ],
  },
}
export const reportThree = {
  id: 'id-3',
  type: 'sigmaReport',
  metadata: {
    embedPath: 'url-3',
    name: 'report-3',
    embedGroup: [
      {
        embedPath: 'path-3',
        name: 'dashboard-3',
        order: 3,
      },
    ],
  },
}

export const translation = {
  'plainText.previousMonth': 'Previous month',
  'plainText.previousThreeMonth': 'Previous 3 months',
  'plainText.previousSixMonth': 'Previous 6 months',
  'plainText.errorOccurred': 'An Error Has Occurred',
  'plainText.noReport': 'No report found',
}

export default getMockPortfolio

export const checkDashboardReportStatus = async (
  dashboardReportNames: string[]
) => {
  for (const name of dashboardReportNames) {
    const dashboardReport = await screen.findByText(name)
    expect(dashboardReport).toBeInTheDocument()
  }
}
