import { render, screen, waitFor, within, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { makeActiveInsightTypes } from '../../../../../mockServer/insightTypes'
import { mockedInsights } from '../../../../../mockServer/allInsights'
import useMultipleSearchParams from '../../../../../../../common/src/hooks/useMultipleSearchParams'
import CardViewInsights from '../../CardViewInsights'
import SiteProvider from '../../../../../providers/sites/SiteStubProvider'
import SitesProvider from '../../../../../providers/sites/SitesStubProvider'

jest.mock('../../../../../../../common/src/hooks/useMultipleSearchParams')
const mockedUseMultipleSearchParams = jest.mocked(useMultipleSearchParams)

const handlers = [
  rest.post('/api/insights/cards', (_req, res, ctx) =>
    res(ctx.json(makeActiveInsightTypes()))
  ),

  rest.post('/api/insights/snackbars/status', (_req, res, ctx) =>
    res(ctx.json([]))
  ),

  rest.get('/api/customers/:customerId/modelsOfInterest', (_req, res, ctx) =>
    res(ctx.json(modelOfInterests))
  ),

  rest.post('/api/insights/filters', (_req, res, ctx) =>
    res(ctx.json({ filters: { ...mockedInsights.filters } }))
  ),
]
const server = setupServer(...handlers)
beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  localStorage.clear()
  mockedUseMultipleSearchParams.mockReset()
})
afterAll(() => server.close())

const Wrapper = ({ children }) => (
  <BaseWrapper
    user={{ customer: { id: 'customer-id', name: 'Customer' } }}
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'labels.priority': 'Priority',
            'plainText.noInsightsFound': noInsightsFoundMsg,
            'labels.resetFilters': resetFilters,
          },
        },
      },
      lng: 'en',
      fallbackLng: ['en'],
    }}
  >
    <SitesProvider sites={mockedSites}>
      <SiteProvider
        site={{
          features: { isInsightsDisabled: false },
        }}
      >
        {children}
      </SiteProvider>
    </SitesProvider>
  </BaseWrapper>
)

describe('All Insights', () => {
  afterEach(() => {
    jest.useRealTimers()
    mockedUseMultipleSearchParams.mockReset()
  })

  test('expect to see "No Insights Found" when /api/insights/all endpoint returns empty array', async () => {
    const mockedSetSearchParams = jest.fn()
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        groupBy: 'allInsights',
        view: 'list',
        status: ['Open', 'InProgress', 'New'],
      } as any,
      mockedSetSearchParams,
    ])
    setupServerWithNoInsights()

    const { container } = await setup()

    await waitFor(() => {
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    })

    const element = screen.queryByText(noInsightsFoundMsg)
    expect(element).toBeInTheDocument()
  })

  test('expect to see Category filter working as expected', async () => {
    const mockedSetSearchParams = jest.fn()
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        groupBy: 'allInsights',
        view: 'list',
        status: ['Open', 'InProgress', 'New'],
        selectedCategories: ['Energy'],
      } as any,
      mockedSetSearchParams,
    ])

    const energyInsights = {
      ...mockedInsights,
      insights: {
        before: 0,
        after: 0,
        items: mockedInsights.insights.items.filter(
          (insight) => insight.type === 'energy'
        ),
        total: 1,
      },
    }

    server.use(
      rest.post('/api/insights/all', (_req, res, ctx) =>
        res(ctx.json(energyInsights))
      )
    )

    const { container, rerender } = await setup()

    await waitFor(() => {
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    })

    // energy insights are displayed, note insights are not
    expect(screen.queryAllByText('Energy', { ignore: 'label' })).toHaveLength(1)
    expect(screen.queryAllByText('Note', { ignore: 'label' })).toHaveLength(0)

    const filters = screen.getAllByRole('group')
    const categoryFilter = filters.find((filter) =>
      within(filter!).queryByText('Energy')
    )

    const energyFilterInput = within(categoryFilter!).queryByText('Energy')

    const energyFilterDiv =
      energyFilterInput!.parentElement!.parentElement!.parentElement!

    // energy filter checked by default
    expect(energyFilterDiv!).toHaveAttribute('data-checked', 'true')

    // uncheck energy filter
    act(() => {
      userEvent.click(energyFilterInput!)
    })
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        groupBy: 'allInsights',
        view: 'list',
        status: ['Open', 'InProgress', 'New'],
      } as any,
      mockedSetSearchParams,
    ])

    server.use(
      rest.post('/api/insights/all', (_req, res, ctx) =>
        res(ctx.json(mockedInsights))
      )
    )

    rerender(<CardViewInsights />)

    await waitFor(() => {
      expect(
        container.getElementsByClassName('mantine-Loader-root').length
      ).not.toBe(1)
    })

    // both energy insights and note insights are shown
    expect(screen.queryAllByText('Energy', { ignore: 'label' })).toHaveLength(1)
    expect(screen.queryAllByText('Note', { ignore: 'label' })).toHaveLength(1)
  })

  test('expect "Reset Filters" button click to reset all filters and leave out "group by" as it is not a filter', async () => {
    const mockedSetSearchParams = jest.fn()
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        groupBy: 'allInsights',
        status: ['Open', 'InProgress', 'New'],
        selectedStatuses: ['InProgress'],
      } as any,
      mockedSetSearchParams,
    ])

    server.use(
      rest.post('/api/insights/all', (_req, res, ctx) =>
        res(ctx.json(mockedInsights))
      )
    )

    const { container } = await setup()

    await waitFor(() => {
      expect(container.querySelector('.mantine-Loader-root')).toBeNull()
    })

    const resetFilterButton = screen.queryByText(resetFilters)
    expect(resetFilterButton).toBeInTheDocument()

    act(() => {
      userEvent.click(resetFilterButton!)
    })

    expect(mockedSetSearchParams).toBeCalledWith({
      groupBy: 'allInsights',
      status: undefined,
      selectedStatuses: undefined,
      sourceType: undefined,
      updatedDate: undefined,
    })
  })
})

const setupServerWithNoInsights = () =>
  server.use(
    rest.post('/api/insights/all', (_req, res, ctx) =>
      res(
        ctx.json({
          insights: {
            before: 0,
            after: 0,
            total: 0,
            items: [],
          },
          filters: {
            insightTypes: [],
            sourceNames: [],
            activity: [],
            primaryModelIds: [],
            detailedStatus: [],
          },
          impactScoreSummary: [],
        })
      )
    )
  )

const noInsightsFoundMsg = 'No Insights Found'

const setup = async () => {
  const rendered = render(<CardViewInsights />, { wrapper: Wrapper })
  await waitFor(() =>
    expect(
      screen.queryByRole('img', { name: 'loading' })
    ).not.toBeInTheDocument()
  )
  return rendered
}

const modelId = 'dtmi:com:willowinc:Land;1'
const resetFilters = 'Reset Filters'
const modelOfInterest = {
  id: '5fded0a3-6bec-42bc-8e0a-7322e0308ede',
  modelId,
  name: 'Land',
  color: '#D9D9D9',
  text: 'La',
}

const modelOfInterests = [modelOfInterest]

const siteId = 'site-id-1'
const mockedSites = [
  {
    id: siteId,
    name: 'site 1',
    features: { isTicketingDisabled: false, isHideOccurrencesEnabled: true },
  },
]
