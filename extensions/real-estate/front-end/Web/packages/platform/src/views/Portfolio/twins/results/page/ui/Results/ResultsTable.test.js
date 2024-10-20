import { render, screen, waitFor } from '@testing-library/react'
import BaseWrapper, { wrapperIsReady } from '@willow/ui/utils/testUtils/Wrapper'
import ResultsTable from './ResultsTable'

function getWrapper() {
  return ({ children }) => (
    <BaseWrapper hasFeatureToggle={(feature) => feature !== 'cognitiveSearch'}>
      {children}
    </BaseWrapper>
  )
}

/* TODO: To test all aspects of ResultsTable with https://dev.azure.com/willowdev/Unified/_workitems/edit/76411 */
describe('ResultsTable', () => {
  test('expect all column headers to be visible', async () => {
    render(
      <ResultsTable
        endOfPageRef={(_node) => {}}
        useSearchResults={InjectedSearchResults}
      />,
      { wrapper: getWrapper() }
    )

    await waitFor(() => expect(wrapperIsReady(screen)).toBeTrue())

    // expect HeaderCheckbox to be visible
    expect(
      await screen.findByTitle('Toggle All Rows Selected')
    ).toBeInTheDocument()

    for (const columnHeader of columnHeaders) {
      expect(await screen.findByText(columnHeader)).toBeInTheDocument()
    }
  })
})

const columnHeaders = [
  'labels.name',
  'twinExplorer.table.type',
  'twinExplorer.table.location',
  'twinExplorer.table.files',
  'labels.floor',
  'twinExplorer.table.relatedTwins',
  'twinExplorer.table.sensors',
  'twinExplorer.table.actions',
]

const InjectedSearchResults = (
  exportSelected = jest.fn(),
  exportAll = jest.fn()
) => ({
  t: jest.fn().mockImplementation((text) => text),
  queryKey: 'twinSearch',
  sites,
  modelsOfInterest: [],
  hasNextPage: false,
  twins: [],
  isLoadingNextPage: false,
  exportSelected,
  exportAll,
})

const sites = [
  {
    id: 'a6b78f54-9875-47bc-9612-aa991cc464f3',
    portfolioId: '152b987f-0da2-4e77-9744-0e5c52f6ff3d',
    name: '126 Phillip Street',
    code: 'INV1236PHI',
    suburb: 'Sydney',
    address: '126 Phillip Street',
    state: 'QLD',
    postcode: '',
    country: 'Australia',
    numberOfFloors: 0,
    logoUrl:
      '/au/api/images/2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b/sites/a6b78f54-9875-47bc-9612-aa991cc464f3/logo/af00c410-5c9c-49d5-bd48-730dda6a6cff_1_w300_h420.jpg',
    logoOriginalSizeUrl:
      '/au/api/images/2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b/sites/a6b78f54-9875-47bc-9612-aa991cc464f3/logo/af00c410-5c9c-49d5-bd48-730dda6a6cff_0.jpg',
    latitude: -33.86696,
    longitude: 151.2117,
    timeZoneId: 'AUS Eastern Standard Time',
    area: '42,965 sqm',
    type: 'Office',
    status: 'Construction',
    userRole: 'admin',
    timeZone: 'Australia/Sydney',
    features: {
      isTicketingDisabled: false,
      isInsightsDisabled: false,
      is2DViewerDisabled: false,
      isReportsEnabled: true,
      is3DAutoOffsetEnabled: false,
      isInspectionEnabled: true,
      isOccupancyEnabled: false,
      isPreventativeMaintenanceEnabled: false,
      isCommandsEnabled: false,
      isScheduledTicketsEnabled: true,
      isNonTenancyFloorsEnabled: true,
      isHideOccurrencesEnabled: false,
      isArcGisEnabled: false,
    },
    webMapId: '',
    ticketStats: {
      overdueCount: 11,
      urgentCount: 79,
      highCount: 3,
      mediumCount: 107,
      lowCount: 22,
      openCount: 211,
    },
    insightsStats: {
      openCount: 0,
      urgentCount: 0,
      highCount: 0,
      mediumCount: 1,
      lowCount: 1,
    },
    location: [151.2117, -33.86696],
  },
]
