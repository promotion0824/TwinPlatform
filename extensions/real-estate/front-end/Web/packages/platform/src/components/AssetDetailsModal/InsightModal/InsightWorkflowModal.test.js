import _ from 'lodash'
import { api, Progress } from '@willow/ui'
import { useQuery } from 'react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { makeInsight } from '@willow/common/insights/testUtils'
import SiteProvider from '../../../providers/sites/SiteStubProvider'
import SitesProvider from '../../../providers/sites/SitesStubProvider'
import InsightWorkflowModal from './InsightWorkflowModal'

const handlers = [
  // the endpoint to update insight workflow status
  rest.put(
    'api/v2/sites/:siteId/insights/:insightId/status',
    (_req, res, ctx) => res(ctx.status(204))
  ),
  rest.get('/api/sites/:siteId/insights/:insightId', (req, res, ctx) => {
    const { insightId, siteId } = req.params
    return res(ctx.json(makeInsight({ id: insightId, siteId })))
  }),
  rest.get('/api/sites/:siteId/insights/:insightId/tickets', (_req, res, ctx) =>
    res(ctx.json([]))
  ),

  rest.get(
    '/api/sites/:siteId/insights/:insightId/occurrences',
    (_req, res, ctx) => res(ctx.json(occurrences))
  ),

  rest.get('/api/sites/:siteId/models', (req, res, ctx) => res(ctx.json({}))),
  rest.get(
    '/api/sites/:siteId/insights/:insightId/activities',
    (req, res, ctx) => res(ctx.json([]))
  ),
  rest.get('/api/sites/:siteId/insights/:insightId/points', (req, res, ctx) =>
    res(
      ctx.json({
        insightPoints: [],
        impactScoresPoints: [],
      })
    )
  ),
]
const insightsHandler = rest.post('/api/insights', (req, res, ctx) =>
  res(ctx.json({ items: [makeInsight({ id: insightIdTwo, siteId })] }))
)

const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('InsightWorkflowModal', () => {
  test('expect to see error message when fetching fail', async () => {
    server.use(
      ...[
        rest.get(
          '/api/sites/:siteId/insights/:insightId/tickets',
          (req, res, ctx) =>
            res(ctx.status(500), ctx.json({ message: 'error' }))
        ),
        insightsHandler,
      ]
    )
    setup({ siteId, insightId: insightIdTwo })

    await waitFor(() => {
      expect(screen.getByText('plainText.errorOccurred')).toBeInTheDocument()
    })
  })

  test('expect to see loading initally', async () => {
    server.use(insightsHandler)
    setup({ siteId, insightId: insightIdTwo })

    expect(screen.getByRole('img', { name: 'loading' })).toBeInTheDocument()
  })

  test('show occurences on click of occurrence tab', async () => {
    server.use(insightsHandler)
    setup({ siteId, insightId: insightIdTwo })
    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    const occurenceTab = screen.queryByText(occurrencesText)

    // to check if occurrences tab is present in modal....
    expect(occurenceTab).toBeInTheDocument()

    userEvent.click(occurenceTab)
    await waitFor(() => {
      // check if occurence tab is highlighted....
      expect(occurenceTab.closest('button')).toHaveAttribute(
        'data-is-selected',
        'true'
      )

      // check if occurences show up on click on occurrence tab....
      expect(screen.queryByText(healthy)).toBeInTheDocument()
      expect(screen.queryByText(faulted)).toBeInTheDocument()
    })
  })

  test('Insight with lastStatus that is not "New" gets passed to InsightWorkflowModal will render as it is', async () => {
    server.use(insightsHandler)
    setup({ siteId, insightId: insightIdTwo })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    // there are 2 places with status text
    expect(screen.queryByText(openText)).toBeInTheDocument()
  })

  test('Insight with lastStatus of "New" will update the insight lastStatus prop and change it to "Open"', async () => {
    const newStatus = 'new'
    const openStatus = 'open'

    // first api call return insight with status of "New"
    // subsequent call will return insight with status of "Open"
    server.use(
      ...[
        rest.post('/api/insights', (req, res, ctx) =>
          res.once(
            ctx.json({
              items: [
                makeInsight({
                  id: insightIdTwo,
                  siteId,
                  lastStatus: newStatus,
                }),
              ],
            })
          )
        ),
        rest.post('/api/insights', (req, res, ctx) =>
          res(
            ctx.json({
              items: [
                makeInsight({
                  id: insightIdTwo,
                  siteId,
                  lastStatus: openStatus,
                }),
              ],
            })
          )
        ),
      ]
    )

    setup({ siteId, insightId: insightIdTwo })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    await waitFor(() => {
      expect(screen.queryByText(openText)).toBeInTheDocument()
    })
  })
})

const siteId = 'site-id-1'
const insightIdTwo = 'insight-id-2'
const mockedSites = [
  {
    id: siteId,
    name: 'site 1',
    features: { isTicketingDisabled: false, isHideOccurrencesEnabled: true },
  },
]

const Wrapper = ({ children }) => (
  <BaseWrapper
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'labels.summary': summary,
            'headers.open': openText,
            'plainText.occurrences': occurrencesText,
            'plainText.healthy': healthy,
            'plainText.faulted': faulted,
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
          features: { isTicketingDisabled: true },
        }}
      >
        {children}
      </SiteProvider>
    </SitesProvider>
  </BaseWrapper>
)

const setup = ({ siteId, insightId }) =>
  render(<ContainerWithInsights siteId={siteId} insightId={insightId} />, {
    wrapper: Wrapper,
  })

const summary = 'Summary'
const openText = 'Open'
const occurrencesText = 'Occurrences'
const healthy = 'Healthy'
const faulted = 'Faulted'

const ContainerWithInsights = ({ siteId, insightId }) => {
  const insightsQuery = useQuery(['insights'], async () => {
    const response = await api.post('/insights')
    return response.data.items
  })

  const insight = (insightsQuery.data ?? [])?.find((i) => i.id === insightId)

  return insightsQuery.isLoading ? (
    <Progress />
  ) : (
    <InsightWorkflowModal
      insightId={insightId}
      siteId={siteId}
      onClose={_.noop}
      name={insight?.name}
      lastStatus={insight?.lastStatus}
      showNavigationButtons
      onPreviousItem={_.noop}
      onNextItem={_.noop}
      setIsTicketUpdated={_.noop}
    />
  )
}

const occurrences = [
  {
    id: '731e6a97-19ae-4ca6-a6e1-17ef5747f8fc',
    insightId: 'cb3397b9-e921-426d-95c5-5c865ce82b15',
    isValid: true,
    isFaulted: true,
    started: '2023-05-06T17:47:44.609Z',
    ended: '2023-06-06T17:47:44.609Z',
    text: 'Faulted Stuck on 0.000 for 15h15m since 05/17/2023 18:34:32 -06:00',
  },
  {
    id: 'd0760277-3497-4598-aeff-cad24e01cb2a',
    insightId: 'cb3397b9-e921-426d-95c5-5c865ce82b15',
    isValid: true,
    isFaulted: false,
    started: '2023-05-06T17:47:44.609Z',
    ended: '2023-06-06T17:47:44.609Z',
    text: 'occur2',
  },
]
