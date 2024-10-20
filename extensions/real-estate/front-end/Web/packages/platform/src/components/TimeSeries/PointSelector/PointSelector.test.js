import { act, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useTicketStatuses } from '@willow/common'
import { supportDropdowns } from '@willow/ui/utils/testUtils/dropdown'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { find } from 'lodash'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import ReactRouter from 'react-router'
import { useSite } from '../../../providers/sites/SiteContext'
import { useSites } from '../../../providers/sites/SitesContext'
import { useTimeSeries } from '../TimeSeriesContext'
import { AssetModalTab } from './AssetModal/AssetModal'
import usePointSelector from './usePointSelector'

jest.mock('../../../providers/sites/SitesContext')
const mockedUseSites = jest.mocked(useSites)
jest.mock('../../../providers/sites/SiteContext')
const mockedUseSite = jest.mocked(useSite)
jest.mock('../TimeSeriesContext')
const mockedUseTimeSeries = jest.mocked(useTimeSeries)
jest.mock(
  '@willow/common/providers/TicketStatusesProvider/TicketStatusesProvider'
)
const mockedUseTicketStatus = jest.mocked(useTicketStatuses)

supportDropdowns()

const handler = [
  rest.get('/api/sites/:siteId/insights/:insightId', (req, res, ctx) => {
    const { insightId, siteId } = req.params
    const insight = getInsights([
      { id: insightIdOne, siteId, name: insightNameOne },
      { id: insightIdTwo, siteId, name: insightNameTwo },
    ]).find((i) => i.id === insightId)
    return res(ctx.json(insight))
  }),
  rest.get('/api/sites/:siteId/assets/:assetId', (req, res, ctx) => {
    const { assetId } = req.params
    return res(
      ctx.json(
        getAsset({
          id: assetId,
          name: `name-for-${assetId}`,
          identifier: `identifier-${assetId}`.toUpperCase(),
        })
      )
    )
  }),
  rest.get('/api/sites/:siteId/assets/:assetId/pinOnLayer', (_req, res, ctx) =>
    res(ctx.json(getPinOnLayer()))
  ),
  rest.get('/api/sites/:siteId/assets/:assetId/files', (_req, res, ctx) =>
    res(ctx.json([]))
  ),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/activities',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/occurrences',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/models', (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/sites/:siteId/assets/:assetId/insights', (req, res, ctx) => {
    const { siteId } = req.params
    return res(
      ctx.json(
        getInsights([
          {
            id: insightIdOne,
            siteId,
            name: insightNameOne,
            occurredDate: '2000-01-01',
          },
          { id: insightIdTwo, siteId, name: insightNameTwo },
        ])
      )
    )
  }),
  rest.post('/api/insights', (req, res, ctx) =>
    res(
      ctx.json({
        items: getInsights([
          {
            id: insightIdOne,
            siteId: siteIdOne,
            name: insightNameOne,
            occurredDate: '2000-01-01',
          },
          {
            id: insightIdTwo,
            siteId: siteIdOne,
            name: insightNameTwo,
          },
        ]),
      })
    )
  ),
  rest.get('/api/sites/:siteId/insights/:insightId/tickets', (_req, res, ctx) =>
    res(ctx.json([]))
  ),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/commands',
    (_req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/assets/:assetId/tickets', (req, res, ctx) => {
    const { siteId } = req.params
    return res(
      ctx.json(
        getTickets([
          { id: ticketIdOne, siteId, summary: ticketSummaryOne },
          { id: ticketIdTwo, siteId, summary: ticketSummaryTwo },
        ])
      )
    )
  }),
]

const server = setupServer(...handler)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.clearAllMocks()
})
afterAll(() => server.close())

const PointSelector = (props) => <div>{usePointSelector(props)}</div>

describe('PointSelector', () => {
  test('AssetModal and InsightModal are visible when modalAssetId and insightId are defined respectively', async () => {
    const mockedPush = jest.fn()
    jest.spyOn(ReactRouter, 'useHistory').mockReturnValue({
      push: mockedPush,
      location: {
        search: '',
      },
    })
    mockedUseSites.mockImplementation(() => [
      { id: siteIdOne, features: { isHideOccurrencesEnabled: false } },
      { id: siteIdTwo, features: { isHideOccurrencesEnabled: false } },
    ])
    mockedUseSite.mockImplementation(() => ({
      features: { isTicketingDisabled: true },
    }))
    const mockedOnAssetChange = jest.fn()
    const mockedOnModalTabChange = jest.fn()
    const mockedOnInsightIdChange = jest.fn()

    mockedUseTimeSeries.mockImplementation(() => ({
      assets: getAssets([
        {
          siteId: siteIdOne,
          assetId: assetIdOne,
          pointId: pointIdOne,
          name: nameOne,
        },
        {
          siteId: siteIdTwo,
          assetId: assetIdTwo,
          pointId: pointIdTwo,
          name: nameTwo,
        },
      ]),
      points: [],
      toggleSitePointId: jest.fn(),
    }))

    const { rerender } = render(
      <PointSelector
        onModalTabChange={mockedOnModalTabChange}
        onAssetChange={mockedOnAssetChange}
        setIsAssetSelectorModalOpen={jest.fn()}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const rerenderWithProps = getRerenderWithProps({
      rerender,
      onModalTabChange: mockedOnModalTabChange,
      onAssetChange: mockedOnAssetChange,
      onInsightIdChange: mockedOnInsightIdChange,
    })

    // name of assets are visible while "go to" buttons are not
    // until user click on "more" button
    await assertStates({ texts: [nameOne, nameTwo], isVisible: true })
    await assertStates({ texts: goToButtons, isVisible: false })
    await act(async () => {
      userEvent.click(await screen.findByTestId(`more-button-${assetIdTwo}`))
    })
    await assertStates({ texts: goToButtons, isVisible: true })

    await act(async () => {
      userEvent.click(await screen.findByText(goToDetails))
    })
    expect(mockedOnAssetChange).toBeCalledWith({
      modalAssetId: assetIdTwo,
      modalTab: AssetModalTab.details,
    })

    rerenderWithProps({
      modalTab: AssetModalTab.details,
      modalAssetId: assetIdTwo,
    })

    await waitFor(async () => {
      expect(await screen.findByText(identifierTwo)).toBeInTheDocument()
    })
    // Asset Modal is open
    await assertStates({ texts: tabHeaders, isVisible: true })

    // click on next button will call onAssetChange with assetIdOne
    // and expect AssetModal to display info for asset one
    await act(async () => {
      userEvent.click(await screen.findByText(next))
    })
    expect(mockedOnAssetChange).toBeCalledWith({
      modalAssetId: assetIdOne,
      modalTab: AssetModalTab.details,
    })

    rerenderWithProps({
      modalTab: AssetModalTab.details,
      modalAssetId: assetIdOne,
    })

    await waitFor(async () => {
      expect(await screen.findByText(identifierOne)).toBeInTheDocument()
    })

    // similar to clicking on "next", click on prev
    // will call onAssetChange
    await act(async () => {
      userEvent.click(await screen.findByText(prev))
    })
    expect(mockedOnAssetChange).toBeCalledWith({
      modalAssetId: assetIdTwo,
      modalTab: AssetModalTab.details,
    })

    // click on insight header will fire onModalTabChange handler
    await act(async () => {
      userEvent.click(await screen.findByText(insightsHeader))
    })
    expect(mockedOnModalTabChange).toBeCalledWith(AssetModalTab.insights)

    // rerender with insight tab to show insights table
    rerenderWithProps({
      rerender,
      modalTab: AssetModalTab.insights,
      modalAssetId: assetIdOne,
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await waitFor(async () => {
      const insightNameOneComponent = await screen.findByText(insightNameOne)
      expect(insightNameOneComponent).toBeInTheDocument()
      await assertStates({ texts: insightNames, isVisible: true })
      userEvent.click(insightNameOneComponent)
    })
    expect(mockedPush).toBeCalled()
  })

  const mockedTicketStatus = [
    {
      color: 'yellow',
      customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
      status: 'Open',
      statusCode: 0,
      tab: 'Open',
    },
    {
      color: 'yellow',
      customerId: '2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b',
      status: 'Reassign',
      statusCode: 5,
      tab: 'Open',
    },
  ]
  const mockedGetByStatusCode = (code) =>
    find(mockedTicketStatus, { statusCode: code })

  test('AssetModal and TicketsModal are visible when modalAssetId and ticketId are defined respectively', async () => {
    jest.spyOn(ReactRouter, 'useHistory').mockReturnValue({
      push: jest.fn(),
      location: {
        pathname: 'time-series',
      },
    })
    mockedUseSites.mockImplementation(() => [
      { id: siteIdOne, features: { isHideOccurrencesEnabled: false } },
      { id: siteIdTwo, features: { isHideOccurrencesEnabled: false } },
    ])
    mockedUseSite.mockImplementation(() => ({
      features: { isTicketingDisabled: true },
    }))
    const mockedOnAssetChange = jest.fn()
    const mockedOnModalTabChange = jest.fn()
    const mockedOnTicketIdChange = jest.fn()

    mockedUseTimeSeries.mockImplementation(() => ({
      assets: getAssets([
        {
          siteId: siteIdOne,
          assetId: assetIdOne,
          pointId: pointIdOne,
          name: nameOne,
        },
        {
          siteId: siteIdTwo,
          assetId: assetIdTwo,
          pointId: pointIdTwo,
          name: nameTwo,
        },
      ]),
      points: [],
      toggleSitePointId: jest.fn(),
    }))

    mockedUseTicketStatus.mockImplementation(() => ({
      data: mockedTicketStatus,
      getByStatus: jest.fn(),
      getByStatusCode: mockedGetByStatusCode,
      isLoading: false,
      queryStatus: 'success',
    }))

    const { rerender } = render(
      <PointSelector
        onModalTabChange={mockedOnModalTabChange}
        onAssetChange={mockedOnAssetChange}
        setIsAssetSelectorModalOpen={jest.fn()}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const rerenderWithProps = getRerenderWithProps({
      rerender,
      onModalTabChange: mockedOnModalTabChange,
      onAssetChange: mockedOnAssetChange,
      onSelectedTicketIdChange: mockedOnTicketIdChange,
    })

    // name of assets are visible while "go to" buttons are not
    // until user click on "more" button
    await assertStates({ texts: [nameOne, nameTwo], isVisible: true })
    await assertStates({ texts: goToButtons, isVisible: false })
    await act(async () => {
      userEvent.click(await screen.findByTestId(`more-button-${assetIdTwo}`))
    })
    await assertStates({ texts: goToButtons, isVisible: true })

    await act(async () => {
      userEvent.click(await screen.findByText(goToDetails))
    })
    expect(mockedOnAssetChange).toBeCalledWith({
      modalAssetId: assetIdTwo,
      modalTab: AssetModalTab.details,
    })

    rerenderWithProps({
      modalTab: AssetModalTab.details,
      modalAssetId: assetIdTwo,
    })

    await waitFor(async () => {
      expect(await screen.findByText(identifierTwo)).toBeInTheDocument()
    })
    // Asset Modal is open
    await assertStates({ texts: tabHeaders, isVisible: true })

    // click on tickets header will fire onModalTabChange handler
    await act(async () => {
      userEvent.click(await screen.findByText(ticketsHeader))
    })
    expect(mockedOnModalTabChange).toBeCalledWith(AssetModalTab.tickets)

    // rerender with tickets tab to show tickets table
    rerenderWithProps({
      rerender,
      modalTab: AssetModalTab.tickets,
      modalAssetId: assetIdOne,
    })
    await waitFor(async () => {
      expect(await screen.findByText(ticketSummaryOne)).toBeInTheDocument()
    })
    await assertStates({ texts: ticketSummaries, isVisible: true })

    // click on a tickets row will fire onSelectedTicketIdChange handler
    await act(async () => {
      userEvent.click(await screen.findByText(ticketSummaryOne))
    })
    expect(mockedOnTicketIdChange).toBeCalledWith(ticketIdOne)

    // expect to see Ticket Detail Modal for ticket one
    rerenderWithProps({
      modalTab: AssetModalTab.tickets,
      selectedTicketId: ticketIdOne,
      modalAssetId: assetIdOne,
    })

    await assertTicketModalIsOpen(ticketSummaryOne)

    // click on "Next" button will fire onSelectedTicketIdChange handler
    // and expect to see ticket Detail Modal for ticket two
    await act(async () => {
      userEvent.click(await screen.findByText(next))
    })

    // expect to see Ticket Detail Modal for ticket two
    rerenderWithProps({
      modalTab: AssetModalTab.tickets,
      selectedTicketId: ticketIdTwo,
      modalAssetId: assetIdOne,
    })

    await assertTicketModalIsOpen(ticketSummaryTwo)
  })
})

const assertStates = async ({ texts, isVisible }) => {
  for await (const text of texts) {
    const regex = new RegExp(text, 'i')
    if (isVisible) {
      const textElements = await screen.findAllByText(regex)
      expect(textElements[0]).toBeInTheDocument()
    } else {
      expect(screen.queryByText(text)).toBeNull()
    }
  }
}

const Wrapper = ({ children }) => (
  <BaseWrapper
    initialEntries={['/time-series']}
    hasFeatureToggle={() => false}
    user={{
      customer: { features: { isConnectivityViewEnabled: false } },
      options: {
        ticketFilterSettings: {
          siteIdOne: {
            selectedPriorities: [],
            selectedSources: [],
            selectedCategories: [],
          },
          siteIdTwo: {
            selectedPriorities: [],
            selectedSources: [],
            selectedCategories: [],
          },
        },
      },
    }}
  >
    {children}
  </BaseWrapper>
)

const prev = 'plainText.previous'
const next = 'plainText.next'
const siteIdOne = 'siteId-1'
const siteIdTwo = 'siteId-2'
const assetIdOne = 'assetId-1'
const assetIdTwo = 'assetId-2'
const pointIdOne = 'pointId-1'
const pointIdTwo = 'pointId-2'
const insightIdOne = 'insightId-1'
const insightIdTwo = 'insightId-2'
const insightNameOne = 'insightName-1'
const insightNameTwo = 'insightName-2'
const insightNames = [insightNameOne, insightNameTwo]
const nameOne = 'name-1'
const nameTwo = 'name-2'
const identifierOne = `identifier-${assetIdOne}`.toUpperCase()
const identifierTwo = `identifier-${assetIdTwo}`.toUpperCase()
const goToDetails = 'plainText.goToAssetDetails'
const goToTickets = 'plainText.goToAssetTickets'
const goToInsights = 'plainText.goToAssetInsights'
const goToRelationships = 'plainText.goToAssetRelationships'
const goToButtons = [goToDetails, goToTickets, goToInsights, goToRelationships]
const detailsHeader = 'headers.details'
const ticketsHeader = 'headers.tickets'
const insightsHeader = 'headers.insights'
const relationshipsHeader = 'headers.relationships'
const tabHeaders = [
  detailsHeader,
  ticketsHeader,
  insightsHeader,
  relationshipsHeader,
]
const ticketIdOne = 'ticketId-1'
const ticketIdTwo = 'ticketId-2'
const ticketSummaryOne = 'Ticket Summary-1'
const ticketSummaryTwo = 'Ticket Summary-2'
const ticketSummaries = [ticketSummaryOne, ticketSummaryTwo]

const getAssets = (data) =>
  data.map(({ siteId, assetId, pointId, name }) => ({
    siteAssetId: `${siteId}_${assetId}`,
    siteId,
    assetId,
    data: {
      id: assetId,
      name, // EAF-L02-01
      customerId: '00000000-0000-0000-0000-000000000000',
      siteId,
      points: [
        {
          id: pointId,
          entityId: pointId,
          name: `i am point ${name}`,
          equipmentId: assetId,
          externalPointId: '7020BI7',
          hasFeaturedTags: false,
          siteId,
          sitePointId: `${siteId}_${pointId}`,
        },
      ],
      tags: [],
      pointTags: [],
    },
  }))

const getAsset = ({ id, name, identifier }) => ({
  id,
  name,
  hasLiveData: true,
  identifier,
})

const getPinOnLayer = () => ({
  title: 'some-title-not-relevant',
  liveDataPoints: [],
})

const getInsights = (data) =>
  data.map(({ id, name, occurredDate = '1999-01-01' }) => ({
    id,
    sequenceNumber: name,
    name,
    status: 'open',
    occurredDate,
    source: 'some-source',
    type: 'some-type',
  }))

const getTickets = (data) =>
  data.map(({ id, siteId, summary }) => ({
    assigneeType: 'some-assignee-type',
    groupTotal: 0,
    id,
    issueType: 'some-issue-type',
    priority: 1,
    siteId,
    statusCode: 0,
    sourceName: 'some-source',
    category: 'some-category',
    sequenceNumber: 'some-sequence-number',
    summary,
  }))

const assertTicketModalIsOpen = async (summary) => {
  await waitFor(() => {
    const ticketIdText = screen.queryByRole('grid')
    expect(ticketIdText).not.toBeNull()
    expect(within(ticketIdText).queryByText(summary)).toBeInTheDocument()
  })
}

/**
 * binds the mocked handlers to our customized rerender function
 * so we dont need to specify mocked handlers multiple times
 */
const getRerenderWithProps =
  ({
    rerender,
    onModalTabChange,
    onAssetChange,
    onInsightIdChange,
    setIsAssetSelectorModalOpen,
    onSelectedTicketIdChange,
  }) =>
  ({ modalTab, insightId, modalAssetId }) =>
    rerender(
      <PointSelector
        modalTab={modalTab}
        modalAssetId={modalAssetId}
        insightId={insightId}
        onModalTabChange={onModalTabChange ?? jest.fn()}
        onAssetChange={onAssetChange ?? jest.fn()}
        onInsightIdChange={onInsightIdChange ?? jest.fn()}
        setIsAssetSelectorModalOpen={setIsAssetSelectorModalOpen ?? jest.fn()}
        onSelectedTicketIdChange={onSelectedTicketIdChange ?? jest.fn()}
      />
    )
