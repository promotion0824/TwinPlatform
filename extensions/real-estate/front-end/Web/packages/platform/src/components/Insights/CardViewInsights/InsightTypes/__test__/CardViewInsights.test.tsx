import {
  act,
  getByText,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { assertPathnameContains } from '@willow/common/utils/testUtils/LocationDisplay'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import * as useMultipleSearchParams from '../../../../../../../common/src/hooks/useMultipleSearchParams'
import { mockedInsights } from '../../../../../mockServer/allInsights'
import allModels from '../../../../../mockServer/allModels'
import { makeActiveInsightTypes } from '../../../../../mockServer/insightTypes'
import SiteProvider from '../../../../../providers/sites/SiteStubProvider'
import SitesProvider from '../../../../../providers/sites/SitesStubProvider'
import routes from '../../../../../routes'
import CardViewInsights from '../../CardViewInsights'

const handlers = [
  rest.post<{
    filterSpecifications: { field: string; value: string | string[] }[]
  }>('/api/insights/cards', (_req, res, ctx) => {
    const searchedRuleName = _req.body?.filterSpecifications.find(
      (s) => s?.field === 'ruleName'
    )?.value
    const insightTypes = makeActiveInsightTypes()

    const priorityFilter = _req.body?.filterSpecifications.find(
      (s) => s?.field === 'priority'
    )?.value

    if (searchedRuleName) {
      // not too concerned by reassigning the items array as this is in test
      insightTypes.cards.items = insightTypes.cards.items.filter((card) =>
        card?.ruleName?.includes(searchedRuleName as string)
      )
      insightTypes.cards.total = insightTypes.cards.items.length
    }

    if (priorityFilter) {
      insightTypes.cards.items = insightTypes.cards.items.filter((card) =>
        (priorityFilter as string[]).map((p) => +p).includes(card.priority)
      )
      insightTypes.cards.total = insightTypes.cards.items.length
    }

    return res(ctx.json(insightTypes))
  }),

  rest.get('/api/customers/:customerId/modelsOfInterest', (_req, res, ctx) =>
    res(ctx.json(modelOfInterests))
  ),

  rest.post('/api/insights/filters', (_req, res, ctx) =>
    res(
      ctx.json({
        filters: {
          ...mockedInsights.filters,
          detailedStatus: ['InProgress', 'ReadyToResolve', 'New', 'Open'],
        },
      })
    )
  ),

  rest.post('/api/insights/snackbars/status', (_req, res, ctx) =>
    res(ctx.json([]))
  ),

  rest.get('/api/sites/:siteId/models', (_req, res, ctx) =>
    res(ctx.json(allModels))
  ),
]
const server = setupServer(...handlers)
beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  localStorage.clear()
  jest.clearAllMocks()
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
            'plainText.insightsNotEnabled': insightsNotEnabledLabel,
            'plainText.noInsightTypesFound': noInsightTypesFoundMsg,
            'plainText.noSkillsFound': noSkillsFoundMsg,
            'plainText.noInsightsFound': noInsightsFoundMsg,
            'labels.insightType': insightTypeText,
            'plainText.skills': skillsText,
            'interpolation.viewInsightsCount': 'View {{count}} Insights',
            'plainText.last7Days': lastSevenDays,
            'plainText.last30Days': lastThirtyDays,
            'plainText.lastYear': lastYear,
            'placeholder.selectDate': selectDate,
            'plainText.new': 'new',
            'plainText.open': 'open',
            'plainText.inProgress': 'in progress',
            'plainText.readyToResolve': 'ready to resolve',
            'plainText.critical': critical,
            'plainText.high': high,
            'plainText.medium': medium,
            'plainText.low': low,
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

describe('CardViewInsights', () => {
  afterEach(() => {
    jest.useRealTimers()
  })

  test('expect to see "No Insight Types Found" when "/api/insights/cards" endpoint returns empty array', async () => {
    setupServerWithCards([])
    await setup()

    const element = screen.queryByText(noSkillsFoundMsg)
    expect(element).toBeInTheDocument()
  })

  test('expect to see error message when "/api/insights/cards" endpoint returns error', async () => {
    setupServerWithReject()
    await setup()

    const element = screen.queryByText('plainText.errorOccurred')
    expect(element).toBeInTheDocument()
  })

  test('expect to see "Ungrouped insights" if ruleId is undefined', async () => {
    setupServerWithCards(ungroupedInsightCards)
    await setup()

    const element = screen.queryByText('Ungrouped Insights')
    expect(element).toBeInTheDocument()
  })

  test('expect to see different data when user pick a last occurred date filter', async () => {
    const eightDaysAgo = new Date(
      Date.now() - 8 * 24 * 60 * 60 * 1000
    ).toISOString()
    const thirtyTwoDaysAgo = new Date(
      Date.now() - 32 * 24 * 60 * 60 * 1000
    ).toISOString()
    const possibleDates = [eightDaysAgo, thirtyTwoDaysAgo]

    const allCards = makeActiveInsightTypes(eightDaysAgo, thirtyTwoDaysAgo)

    let counter = 0
    server.use(
      rest.post('/api/insights/cards', (_req, res, ctx) => {
        // only 2nd call to this endpoint will return empty array
        // this is to simulate the scenario where there are no cards returned
        counter++
        if (counter === 2) {
          return res(
            ctx.json({
              cards: {
                before: 0,
                after: 0,
                total: 0,
                items: [],
              },
            })
          )
        } else {
          return res(ctx.json(allCards))
        }
      })
    )
    const { container } = await setup()

    // since we only create insight types occurred 8 days ago and 32 days ago,
    // we would NOT see last 7 days header, and expect to see
    // last 30 days and last year headers
    await waitFor(async () => {
      expect(screen.queryByText(lastYear)).not.toBeNull()
      expect(screen.queryByText(lastThirtyDays)).not.toBeNull()
    })
    expect(screen.queryByText(lastSevenDays)).toBeNull()

    const filterSelect = screen.queryByTestId('last-occurred-date-select')
    expect(filterSelect).toBeInTheDocument()
    act(() => {
      userEvent.click(filterSelect!)
    })
    const sevenDaysFilterOption = screen.queryByText(camelCasedLastSevenDays)
    expect(sevenDaysFilterOption).not.toBeNull()
    act(() => {
      userEvent.click(sevenDaysFilterOption!)
    })
    await waitFor(() => {
      expect(
        container.getElementsByClassName('mantine-Loader-root').length
      ).toBe(0)
    })
    expect(screen.queryByText(noSkillsFoundMsg)).not.toBeNull()

    // switch to last year filter
    act(() => {
      userEvent.click(filterSelect!)
    })
    const lastYearFilterOption = screen.queryByText(camelCasedLastYear)
    expect(lastYearFilterOption).not.toBeNull()
    act(() => {
      userEvent.click(lastYearFilterOption!)
    })
    await waitFor(() => {
      expect(
        container.getElementsByClassName('mantine-Loader-root').length
      ).toBe(0)
    })

    await waitFor(() => expect(screen.queryByText(noSkillsFoundMsg)).toBeNull())
    const viewInsightCountElements = screen.getAllByText(/View \d+ Insights/)
    // expect to see all cards that occurred last year; and this includes:
    // 1. cards that occurred 8 days ago
    // 2. cards that occurred 32 days ago
    expect(viewInsightCountElements.length).toBe(
      allCards.cards.items.filter((card) =>
        possibleDates.includes(card.lastOccurredDate)
      ).length
    )
  })

  test('expect to see searched keyword in list of card results', async () => {
    const keyWord = 'Poor Chiller'
    const ruleName = 'Poor Chiller Efficiency'
    const debouncedTime = 3000
    jest.useFakeTimers()

    const { container } = await setup()
    await waitFor(() => {
      expect(
        container.getElementsByClassName('mantine-Loader-root').length
      ).toBe(0)
    })

    const searchInput = screen.queryByTestId('search-input')
    act(() => {
      userEvent.type(searchInput!, keyWord)
      // wait till debounced time has passed
      jest.advanceTimersByTime(debouncedTime)
    })

    await waitFor(() => {
      expect(
        container.getElementsByClassName('mantine-Loader-root').length
      ).toBe(0)
    })

    const cardHeader = screen.getByText(ruleName)

    expect(cardHeader).toBeInTheDocument()
    // check if card has relevant keyword
    expect(cardHeader).toHaveTextContent(keyWord)

    const ruleNameNotSatisfyingSearch =
      'NREL: AHU Chilled Water Valve Stuck Closed 2'
    // expect ruleNameNotSatisfyingSearch is actually from the list of cards
    expect(
      makeActiveInsightTypes().cards.items.find(
        (card) => card.ruleName === ruleNameNotSatisfyingSearch
      )
    ).not.toBeNull()
    expect(screen.queryByText(ruleNameNotSatisfyingSearch)).toBeNull()
  })

  test('expect to see data grid view once user clicks on view_list segment', async () => {
    await setup()
    const {
      cards: { total, items },
    } = makeActiveInsightTypes()
    const insightTypeCountElement = screen.queryAllByText(skillsText)[0]
    expect(insightTypeCountElement).toBeInTheDocument()
    expect(within(insightTypeCountElement!).queryByText(total)).not.toBeNull()

    // match by using regex to match "View {{count}} Insights" where
    // count is number
    const viewInsightCountElements = screen.getAllByText(/View \d+ Insights/)
    expect(viewInsightCountElements.length).toBe(total)

    const segmentedButton = screen.getByTestId('insightSegmentedControl')
    // click on view_list segment to switch to list view which is a data grid
    // containing "total" number of rows + 1 header row
    act(() => {
      const TableRadioButton = getByText(segmentedButton, 'view_list')
      expect(TableRadioButton).toBeInTheDocument()
      userEvent.click(TableRadioButton!)
    })

    const dataGrid = screen.queryByRole('grid')
    await waitFor(() => {
      expect(dataGrid).toBeInTheDocument()
    })
    expect(dataGrid?.getAttribute('aria-rowcount')).toBe((total + 1).toString())

    // click on a rule and expect user to land on corresponding insight page
    const secondRule = items[2]
    const secondRuleElement = screen.queryByText(secondRule.ruleName!)
    expect(secondRuleElement).toBeInTheDocument()
    act(() => {
      userEvent.click(secondRuleElement!)
    })
    assertPathnameContains(routes.insights_rule__ruleId(secondRule.ruleId))
  })

  test('expect to see insight filter panel with options', async () => {
    const modelGuaranteedDoesNotExist = 'dtmi:com:willowinc:NonExistentModel;1'
    const mockedSetSearchParams = jest.fn()
    const spy = jest.spyOn(useMultipleSearchParams, 'default')
    spy.mockReturnValue([
      {
        status: [],
      },
      mockedSetSearchParams,
    ])

    server.use(
      rest.post('/api/insights/filters', (_req, res, ctx) =>
        res(
          ctx.json({
            filters: {
              ...mockedInsights.filters,
              primaryModelIds: [
                ...mockedInsights.filters.primaryModelIds,
                modelGuaranteedDoesNotExist,
              ],
              detailedStatus: ['InProgress', 'ReadyToResolve', 'New', 'Open'],
            },
          })
        )
      )
    )

    await setup()
    const {
      cards: { total },
    } = makeActiveInsightTypes()

    const insightFilterPanel = screen.queryByTestId('insightFilterPanel')
    expect(insightFilterPanel).toBeInTheDocument()

    await waitFor(() => {
      expect(screen.queryAllByTestId('individual-card').length).toBe(total)
    })

    // expect to see all the status filter options
    await waitFor(() => {
      for (const status of mockedInsights.filters.detailedStatus) {
        expect(
          within(insightFilterPanel!).getByText(status)
        ).toBeInTheDocument()
      }
    })
    // expect status filter to be rendered in the following order
    expect(
      within(insightFilterPanel!)
        .queryAllByText(/New|Open|In Progress|Ready to Resolve/)
        .map((el) => el.textContent)
    ).toEqual(['New', 'Open', 'In Progress', 'Ready to Resolve'])

    // model id that does not exist in ontology and thus cannot be displayed
    // with a human readable name will not be displayed
    expect(
      within(insightFilterPanel!).queryByText(modelGuaranteedDoesNotExist)
    ).toBeNull()

    // expect the underlying handler to be called when user clicks on a filter option
    const statusOptionToClick = mockedInsights.filters.detailedStatus[0]
    act(() => {
      userEvent.click(
        within(insightFilterPanel!).getByText(statusOptionToClick)
      )
    })
    expect(mockedSetSearchParams).toBeCalledWith({
      page: undefined, // set page to undefined to reset page to 1
      selectedStatuses: [statusOptionToClick],
    })
  })

  test('asset model filter is visible when modelId can be found in ontology', async () => {
    jest.spyOn(useMultipleSearchParams, 'default').mockReturnValue([
      {
        status: [],
      },
      jest.fn(),
    ])
    await setup()

    await waitFor(() => {
      expect(screen.queryByTestId(modelFilterTestId)).not.toBeNull()
    })
  })

  test('asset model filter is invisible when modelId cannot be found in ontology', async () => {
    jest.spyOn(useMultipleSearchParams, 'default').mockReturnValue([
      {
        status: [],
      },
      jest.fn(),
    ])
    server.use(
      rest.get('/api/sites/:siteId/models', (_req, res, ctx) =>
        res(ctx.json(allModels.filter((model) => model.id !== assetModelId)))
      )
    )

    await setup()

    expect(screen.queryByTestId(modelFilterTestId)).toBeNull()
  })

  test('expect priority filters to filter insights based on priority', async () => {
    jest.spyOn(useMultipleSearchParams, 'default').mockRestore()
    const priorityMap = {
      [low]: 4,
      [medium]: 3,
      [high]: 2,
      [critical]: 1,
    }
    const priorityFilterValues: number[] = []
    const insightCards = makeActiveInsightTypes().cards.items

    await setup()

    // expect to not see reset filters button when no filters are selected
    expect(screen.queryByText(resetFilters)).toBeNull()

    const priorityFilters = screen.queryByTestId('priorities-checkbox-group')
    expect(priorityFilters).not.toBeNull()

    await waitFor(() => {
      expect(screen.queryAllByTestId('individual-card').length).toBe(
        insightCards.length
      )
    })

    // expect to see low priority filter, and then click on it
    const lowPriority = within(priorityFilters!).queryByText(low)
    act(() => {
      userEvent.click(lowPriority!)
    })
    priorityFilterValues.push(priorityMap[low])

    // expect to see correct number of cards after low priority filter is clicked
    await waitFor(() => {
      expect(screen.queryAllByTestId('individual-card').length).toBe(
        insightCards.filter((card) =>
          priorityFilterValues.includes(card.priority)
        ).length
      )
    })

    // expect to see medium priority filter, and then click on it
    const mediumPriority = within(priorityFilters!).queryByText(medium)
    act(() => {
      userEvent.click(mediumPriority!)
    })
    priorityFilterValues.push(priorityMap[medium])

    // expect to see correct number of cards after medium priority filter is clicked
    await waitFor(() => {
      expect(screen.queryAllByTestId('individual-card').length).toBe(
        insightCards.filter((card) =>
          priorityFilterValues.includes(card.priority)
        ).length
      )
    })
    // expect to see reset filters button when filters are selected
    const resetFiltersButton = screen.queryByText(resetFilters)
    expect(resetFiltersButton).not.toBeNull()

    // click on reset filters button and expect to see all cards
    act(() => {
      userEvent.click(resetFiltersButton!)
    })
    await waitFor(() => {
      expect(screen.queryAllByTestId('individual-card').length).toBe(
        insightCards.length
      )
    })
  })

  test('expect insight filter query failure to not block the main table', async () => {
    // query for the insight filter error out
    server.use(
      rest.post('/api/insights/filters', (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
      )
    )
    const { container } = await setup()
    await waitFor(() => {
      expect(
        container.getElementsByClassName('mantine-Loader-root').length
      ).toBe(0)
    })

    // expect main card table still to be visible
    await waitFor(() => {
      expect(screen.queryAllByTestId('individual-card').length).toBe(
        makeActiveInsightTypes().cards.items.length
      )
    })
    // expect error message to be visible on filter panel
    expect(
      within(screen.queryByTestId('insightFilterPanel')!).queryByText(
        'plainText.errorOccurred'
      )
    ).not.toBeNull()
  })
})

const setup = async () => {
  const rendered = render(<CardViewInsights />, { wrapper: Wrapper })
  await waitFor(() => {
    expect(
      rendered.container.getElementsByClassName('mantine-Loader-root').length
    ).toBe(0)
  })
  return rendered
}

const setupServerWithCards = (params) =>
  server.use(
    rest.post('api/insights/cards', (req, res, ctx) =>
      res(
        ctx.json({
          cards: { items: params },
          filters: [],
          impactScoreSummary: [],
          insightTypesGroupedByDate: [],
          cardSummaryFilters: [],
        })
      )
    )
  )

const setupServerWithReject = () =>
  server.use(
    rest.post('api/insights/cards', (req, res, ctx) =>
      res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    )
  )

const selectDate = 'Select Date'
const lastYear = 'LAST YEAR'
const lastThirtyDays = 'LAST 30 DAYS'
const lastSevenDays = 'LAST 7 DAYS'
const camelCasedLastSevenDays = 'Last 7 Days'
const camelCasedLastYear = 'Last Year'
const insightsNotEnabledLabel = 'Insights not enabled for site'
const noInsightTypesFoundMsg = 'No Insight Types Found'
const noSkillsFoundMsg = 'No Skills Found'
const noInsightsFoundMsg = 'No Insights Found'
const modelFilterTestId = 'selectedPrimaryModelIds-checkbox-group'
const assetModelId = 'dtmi:com:willowinc:Asset;1'
const critical = 'Critical'
const high = 'High'
const medium = 'Medium'
const low = 'Low'
const resetFilters = 'Reset Filters'

// lastOccurredDate is set to date within the last 7 Days from current date.
const lastOccurredDate = new Date()
lastOccurredDate.setDate(lastOccurredDate.getDate() - 6)

const ungroupedInsightCards = [
  {
    priority: 2,
    sourceName: '',
    insightCount: 45,
    lastOccurredDate: '2022-12-11T16:54:48.794Z',
    impactScores: [],
    recommendation: 'Recommendation-1',
  },
]

const insightTypeText = 'Insight Type'
const skillsText = 'Skills'

const modelId = 'dtmi:com:willowinc:Land;1'
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
    features: {
      isTicketingDisabled: false,
      isHideOccurrencesEnabled: true,
    },
  },
]
