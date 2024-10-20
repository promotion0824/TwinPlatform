import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { UserAgentProvider } from '@willow/ui/providers/UserAgentProvider/UserAgentProvider'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { UseQueryResult } from 'react-query'
import SiteProvider from '../../../../providers/sites/SiteStubProvider'
import LayoutProvider from '../../../Layout/Layout/Layout'
import useManagedPortfolios, {
  ManagedPortfolio,
} from '../../../Portfolio/useManagedPortfolios'
import Portfolios from '../Portfolios'

afterEach(() => mockedUseManagedPortfolios.mockReset())

jest.mock('../../../Portfolio/useManagedPortfolios')
const mockedUseManagedPortfolios = jest.mocked(useManagedPortfolios)

describe('Portfolios', () => {
  test('expect to see "Error Occurred" when error happens', async () => {
    mockedUseManagedPortfolios.mockImplementation(
      () =>
        ({
          status: 'error',
          data: undefined,
        } as UseQueryResult<ManagedPortfolio[], unknown>)
    )
    render(
      <Portfolios
        showPortfolioTab
        isCustomerAdmin
        featureFlags={{ hasFeatureToggle: () => true }}
      />,
      {
        wrapper: getWrapper(),
      }
    )

    expect(await screen.findByText('Error Occurred')).toBeInTheDocument()
  })

  test('expect to see spinner when loading', async () => {
    mockedUseManagedPortfolios.mockImplementation(
      () =>
        ({
          status: 'loading',
          data: undefined,
        } as UseQueryResult<ManagedPortfolio[], unknown>)
    )
    const { container } = render(
      <Portfolios
        showPortfolioTab
        isCustomerAdmin
        featureFlags={{ hasFeatureToggle: () => true }}
      />,
      {
        wrapper: getWrapper(),
      }
    )

    // the loading spinner
    expect(container.querySelector('.progress')).toBeInTheDocument()
  })

  test('expect to see every portfolio', async () => {
    mockedUseManagedPortfolios.mockImplementation(
      () =>
        ({
          status: 'success',
          data: managedPortfolios,
        } as UseQueryResult<ManagedPortfolio[], unknown>)
    )

    render(
      <Portfolios
        showPortfolioTab
        isCustomerAdmin
        featureFlags={{ hasFeatureToggle: () => true }}
      />,
      {
        wrapper: getWrapper(),
      }
    )

    expect(await screen.findByText(firstPortfolioName)).toBeInTheDocument()
    expect(await screen.findByText(secondPortfolioName)).toBeInTheDocument()

    // click on Card button representing first portfolio will open "PortfolioModal"
    await act(async () => {
      userEvent.click(await screen.findByTestId(firstPortfolioCardButtonTestId))
    })

    // PortfolioModal showed up
    await waitFor(async () => {
      expect(await screen.findByText('Manage Sites')).toBeInTheDocument()
    })

    await act(async () => {
      userEvent.click(await screen.findByText('Cancel'))
    })

    // PortfolioModal is closed
    await waitFor(() => {
      expect(screen.queryByText('Manage Sites')).not.toBeInTheDocument()
    })
  })
})

function getWrapper() {
  return ({ children }) => (
    <BaseWrapper
      hasFeatureToggle={() => true}
      user={{
        customer: { id: '123', name: 'cant think of a name' },
        portfolios: [{ id: '312', name: 'investa' }],
      }}
      i18nOptions={{
        resources: {
          en: {
            translation,
          },
        },
        lng: 'en',
        fallbackLng: ['en'],
      }}
    >
      <SiteProvider site={{ id: 123 }}>
        <LayoutProvider>
          <UserAgentProvider>{children}</UserAgentProvider>
        </LayoutProvider>
      </SiteProvider>
    </BaseWrapper>
  )
}

const translation = {
  'plainText.errorOccurred': 'Error Occurred',
  'labels.name': 'Name',
  'plainText.manageSites': 'Manage Sites',
}

const firstPortfolioName = 'Investa'
const secondPortfolioName = 'Tainves'
const firstPortfolioCardButtonTestId = 'card-Investa'

const managedPortfolios: ManagedPortfolio[] = [
  {
    portfolioId: '152b987f-0da2-4e77-9744-0e5c52f6ff3d',
    portfolioName: firstPortfolioName,
    features: {
      isTwinSearchEnabled: true,
    },
    role: 'Admin',
    sites: [
      {
        siteId: 'f1914666-4050-4ff7-afd7-013bae2eee97',
        siteName: '40 Mount Street',
        role: 'Admin',
      },
    ],
  },
  {
    portfolioId: '4941aeb2-8c4b-4e3c-8881-a3c6fb4cd112',
    portfolioName: secondPortfolioName,
    features: {
      isTwinSearchEnabled: false,
    },
    role: 'Admin',
    sites: [
      {
        siteId: 'b9b4c5f9-064f-4985-9f51-fdf9891d0389',
        siteName: 'Sofia',
        role: 'Admin',
      },
    ],
  },
]
