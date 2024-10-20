import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import api from '../../../../../../ui/src/utils/api'
import sites from '../../../../mockServer/sites'
import PortfolioModal from '../PortfolioModal'

const handlers = [
  rest.get('/api/me/sites', (_req, res, ctx) => res(ctx.json([]))),
]

const server = setupServer(...handlers)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.restoreAllMocks()
})
afterAll(() => server.close())

describe('PortfolioModal', () => {
  test('expect not to show all sites features section when user is not admin', async () => {
    render(
      <PortfolioModal
        portfolio={{
          portfolioId: sites[0].portfolioId,
          role: 'Admin',
        }}
      />,
      {
        wrapper: getWrapper({ user: { isCustomerAdmin: false } }),
      }
    )

    expect(screen.queryByTestId(allSitesFeaturesTestId)).not.toBeInTheDocument()
  })

  test('expect not to show all sites features section when user is not from willow', async () => {
    render(
      <PortfolioModal
        portfolio={{
          portfolioId: sites[0].portfolioId,
          role: 'Admin',
        }}
      />,
      {
        wrapper: getWrapper({
          user: { isCustomerAdmin: true, email: 'jane.doe@google.com' },
        }),
      }
    )

    expect(screen.queryByTestId(allSitesFeaturesTestId)).not.toBeInTheDocument()
  })

  test('enable all sites features should trigger feature update on every site', async () => {
    const mockedPut = jest.fn()
    jest.spyOn(api, 'put').mockImplementation(mockedPut)

    const reportOnInsightOff = {
      isReportsEnabled: true,
      isInsightsDisabled: true,
    }
    server.use(
      rest.get('/api/me/sites', (_req, res, ctx) =>
        res(
          ctx.json(
            sites.map((site) => ({
              ...site,
              features: {
                ...site.features,
                ...reportOnInsightOff,
              },
            }))
          )
        )
      ),
      rest.put(
        `/api/customers/:customerId/portfolios/:portfolioId`,
        (_req, res, ctx) => res(ctx.status(200))
      )
    )

    render(
      <PortfolioModal
        portfolio={{
          portfolioId: sites[0].portfolioId,
          role: 'Admin',
        }}
      />,
      {
        wrapper: getWrapper({
          user: { isCustomerAdmin: true, email: 'john.doe@willowinc.com' },
        }),
      }
    )

    // expect all sites features section to be shown
    await waitFor(() => {
      expect(screen.getByTestId(allSitesFeaturesTestId)).toBeInTheDocument()
    })

    // expect reports feature to be enabled while insights feature to be disabled
    const reportsFeature = screen.queryByTestId(reportFeature)
    const insightsFeature = screen.queryByTestId(insightFeature)
    expect(reportsFeature).toHaveAttribute('checked')
    expect(insightsFeature).not.toHaveAttribute('checked')

    // click on reports feature to disable it
    act(() => {
      userEvent.click(reportsFeature)
    })
    // click on insights feature to enable it
    act(() => {
      userEvent.click(insightsFeature)
    })

    const saveButton = screen.queryByText(saveText)
    act(() => {
      userEvent.click(saveButton)
    })

    // expect feature update to be called for every site
    expect(mockedPut).toHaveBeenCalledTimes(sites.length)
    for (const call of mockedPut.mock.calls) {
      const [, data] = call
      expect(data.features.isReportsEnabled).toBe(false) // set "enable" flag to false to disable the feature
      expect(data.features.isInsightsDisabled).toBe(false) // set "disable" flag to false to enable the feature
    }
  })
})

const saveText = 'plainText.save'
const allSitesFeaturesTestId = 'all-sites-features'
const reportFeature = 'isReportsEnabled'
const insightFeature = 'isInsightsDisabled'

const getWrapper =
  ({ user }) =>
  ({ children }) =>
    (
      <BaseWrapper user={user} hasFeatureToggle={() => true}>
        {children}
      </BaseWrapper>
    )
