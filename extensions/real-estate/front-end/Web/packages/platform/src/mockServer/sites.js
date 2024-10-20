export function makeSite(params) {
  return {
    portfolioId: '152b987f-0da2-4e77-9744-0e5c52f6ff3d',
    name: '60 Martin Place',
    code: '60MP',
    suburb: 'North Sydney',
    address: '40 MOUNT STREET, NORTH SYDNEY NSW',
    state: 'NSW',
    postcode: '',
    country: 'Australia',
    numberOfFloors: 0,
    timeZoneId: 'AUS Eastern Standard Time',
    area: '34200',
    type: 'Office',
    status: 'Operations',
    userRole: 'admin',
    timeZone: 'Australia/Sydney',
    features: {
      isTicketingDisabled: false,
      isInsightsDisabled: false,
      is2DViewerDisabled: false,
      isReportsEnabled: true,
      is3DAutoOffsetEnabled: false,
      isInspectionEnabled: false,
      isOccupancyEnabled: false,
      isPreventativeMaintenanceEnabled: false,
      isCommandsEnabled: false,
      isScheduledTicketsEnabled: false,
      isNonTenancyFloorsEnabled: false,
      isHideOccurrencesEnabled: false,
      isArcGisEnabled: false,
    },
    ticketStats: {
      overdueCount: 1,
      urgentCount: 0,
      highCount: 0,
      mediumCount: 1,
      lowCount: 0,
      openCount: 1,
    },
    insightsStats: {
      openCount: 0,
      urgentCount: 0,
      highCount: 0,
      mediumCount: 0,
      lowCount: 0,
    },
    ...params,
  }
}

export const siteIdWithDashboard = '404bd33c-a697-4027-b6a6-677e30a53d07'
export const siteIdWithoutDashboard = '926d1b17-05f7-47bb-b57b-75a922e69a20'
const sites = [
  makeSite({
    id: siteIdWithDashboard,
    name: '60 Martin Place',
    code: '60MP',
    latitude: -33.838319,
    longitude: 151.2046177,
  }),
  makeSite({
    id: siteIdWithoutDashboard,
    name: '420 George Street',
    code: '420GS',
    latitude: -40.838319,
    longitude: 158.2046177,
  }),
]
export default sites
