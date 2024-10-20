import { render, screen, fireEvent } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { InsightPriority } from '@willow/common/insights/insights/types'
import { makeActiveInsightTypes } from '../../../../../mockServer/insightTypes'
import IndividualCard from '../IndividualCard'

const handlers = [
  rest.post('/api/insights/cards', (_req, res, ctx) =>
    res(ctx.json(makeActiveInsightTypes()))
  ),
]

const server = setupServer(...handlers)
beforeAll(() => server.listen())
afterAll(() => server.close())

function getWrapper() {
  return ({ children }) => (
    <BaseWrapper
      i18nOptions={{
        resources: {
          en: {
            translation: {
              'labels.priority': 'Priority',
            },
          },
        },
        lng: 'en',
        fallbackLng: ['en'],
      }}
    >
      {children}
    </BaseWrapper>
  )
}

describe('IndividualCard', () => {
  const mockedTFunction = jest.fn().mockImplementation((text: string) => text)
  const mockHandleNavigation = jest.fn()

  test('Hide impact title, if impact scores is empty', async () => {
    setupServerWithCards(cards)
    setup(mockedTFunction, mockHandleNavigation)
    const element = screen.queryByText(impactTitle)
    expect(element).not.toBeInTheDocument()
  })

  test('Navigation function called, if clicked on card', async () => {
    setup(mockedTFunction, mockHandleNavigation)
    const element = screen.getByText(ruleName)
    fireEvent.click(element)
    expect(mockHandleNavigation).toHaveBeenCalledTimes(1)
  })
})

const setup = (mockedTFunction, mockHandleNavigation) =>
  render(
    <IndividualCard
      title={cards[0].ruleName}
      type={cards[0].insightType}
      insightCount={cards[0].insightCount}
      priority={cards[0].priority as InsightPriority}
      lastOccurred={cards[0].lastOccurredDate}
      impactTitle={impactTitle}
      impactScore={undefined}
      t={mockedTFunction}
      language="en"
      onClick={mockHandleNavigation}
    />,
    {
      wrapper: getWrapper(),
    }
  )

const ruleName = 'Test Rule Name'
const insightType = 'energy'
const priority = 2
const sourceId = '7caed3b8-c0b6-4f91-ad3e-29d68882efeb'
const sourceName = 'Willow Activate'
const insightCount = 36
const impactScores = undefined
const impactTitle = 'Avoidable Cost per Year'

const cards = [
  {
    id: 'ahu-chw-leaking-2-nrel',
    ruleId: 'ahu-chw-leaking-2-nrel',
    ruleName,
    insightType,
    priority: priority as InsightPriority,
    sourceId,
    sourceName,
    insightCount,
    lastOccurredDate: '2023-03-03T16:09:43.136Z',
    impactScores,
    recommendation: '',
  },
]

const setupServerWithCards = (params) =>
  server.use(
    ...[
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
      ),
    ]
  )
