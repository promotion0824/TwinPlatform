export const ONLINE_TAB = 'Online'
export const OFFLINE_TAB = 'Offline'
export const ALL_SITES_TAB = 'All sites'

export function getFilteredConnectivityTableData({
  connectivityTableData = [],
  filters,
  selectedTab,
}) {
  return connectivityTableData
    .filter((record) => {
      if (selectedTab === OFFLINE_TAB) {
        return !record.isOnline
      }
      if (selectedTab === ONLINE_TAB) {
        return record.isOnline
      }
      // When selectedTab is ALL_SITES, return all records
      return true
    })
    .filter((record) =>
      record.name.toLowerCase().includes(filters.search.toLowerCase())
    )
    .filter(
      (record) =>
        filters.selectedCountry === null ||
        filters.selectedCountry.includes(record.country)
    )
    .filter(
      (record) =>
        filters.selectedStates.length === 0 ||
        filters.selectedStates.includes(record.state)
    )
    .filter(
      (record) =>
        filters.selectedCities.length === 0 ||
        filters.selectedCities.includes(record.city)
    )
    .filter(
      (record) =>
        filters.selectedAssetClasses.length === 0 ||
        filters.selectedAssetClasses.includes(record.assetClass)
    )
}

// A connector can either be enabled or disabled
export const ENABLED = 'ENABLED'
export const DISABLED = 'DISABLED'

// The following are enabled connector statuses that is considered operational
export const ONLINE = 'ONLINE'
export const DISRUPTED = 'DISRUPTED'
export const READY = 'READY'

// The following are enabled connector statuses that is considered not operational
export const OFFLINE = 'OFFLINE'
export const UNKNOWN = 'UNKNOWN'

export const ARCHIVED = 'ARCHIVED'

export const ONLINE_CONNECTOR_STATUSES = [ONLINE, DISRUPTED, READY]
export const OFFLINE_CONNECTOR_STATUSES = [OFFLINE, UNKNOWN, DISABLED]
