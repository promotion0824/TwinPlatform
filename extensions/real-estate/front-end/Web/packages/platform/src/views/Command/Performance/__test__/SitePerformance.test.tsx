import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { queryCache } from '@willow/common'
import { supportDropdowns } from '@willow/ui/utils/testUtils/dropdown'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import withTranslationValues from '@willow/ui/utils/withTranslationValues'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import * as authService from '../../../../services/Widgets/AuthWidgetService'
import { AuthenticatedSigma } from '../../../../services/Widgets/AuthWidgetService'
import * as widgetsService from '../../../../services/Widgets/WidgetsService'
import {
  checkDashboardReportStatus,
  reportOne,
  sixtyMartin,
  translation,
} from '../../../Portfolio/KPIDashboards/HeaderControls/__test__/utils'
import SitePerformance from '../SitePerformance'

supportDropdowns()
const handler = [
  rest.get('/api/tenants', (_req, res, ctx) => res(ctx.json([]))),
  rest.post('api/sigma/portfolios/:portfolioId/embedurls', (_req, res, ctx) =>
    res(ctx.delay(), ctx.json([]))
  ),
]
const server = setupServer(...handler)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())
afterEach(() => queryCache.clear())

supportDropdowns()

describe('SitePerformance.tsx', () => {
  test('expect to see error message when getWidgets services return fetch error', async () => {
    initialize({
      track: jest.fn(),
      widgetsResponse: { isError: true, status: 'error' },
      widgetsToDisplay: [],
    })

    await waitFor(() => {
      expect(screen.getByText('An Error Has Occurred')).toBeInTheDocument()
    })
  })

  test('expect to see error message when postAuthenticatedReport services return fetch error', async () => {
    const onDashboardReportClickMock = jest.fn()
    jest
      .spyOn(authService, 'fetchAuthenticatedReport')
      .mockRejectedValue(new Error('fetch error'))

    initialize({
      track: jest.fn(),
      widgetsResponse: {
        data: { widgets: [reportOne as widgetsService.SigmaWidget] },
      },
      widgetsToDisplay: [reportOne as widgetsService.SigmaWidget],
      selectedReport: reportOne,
      selectedDashboardReport: reportOne.metadata.embedGroup[0],
      onDashboardReportClick: onDashboardReportClickMock,
    })

    const errorMessage = await screen.findByText('An Error Has Occurred')

    expect(errorMessage).toBeInTheDocument()
  })

  test('expect to see first dashboard report selected when widgetsService succeeds', async () => {
    const onDashboardReportClickMock = jest.fn()

    initialize({
      track: jest.fn(),
      widgetsResponse: {
        data: { widgets: [reportOne as widgetsService.SigmaWidget] },
      },
      widgetsToDisplay: [reportOne as widgetsService.SigmaWidget],
      selectedReport: reportOne,
      selectedDashboardReport: reportOne.metadata.embedGroup[0],
      onDashboardReportClick: onDashboardReportClickMock,
    })

    await checkDashboardReportStatus([
      'dashboard-11',
      'dashboard-12',
      'dashboard-13',
    ])

    await act(async () => {
      userEvent.click(await screen.findByText('dashboard-12'))
    })

    expect(onDashboardReportClickMock).toBeCalledWith(
      reportOne.metadata.embedGroup.find(
        (dashboard) => dashboard.name === 'dashboard-12'
      ),
      reportOne.metadata.category
    )
  })

  test('expect to see spinner when status is loading', async () => {
    const { container } = initialize({
      widgetsResponse: { isLoading: true, status: 'loading' },
      widgetsToDisplay: [],
    })

    // the loading spinner
    expect(container.querySelector('.progress')).toBeInTheDocument()
  })

  test('expect to see the iframe when both services succeed and no error happens', async () => {
    const {
      metadata: { embedGroup },
    } = reportOne

    jest.spyOn(authService, 'fetchAuthenticatedReport').mockResolvedValue({
      url: embedGroup[0].embedPath,
      name: embedGroup[0].name,
    } as AuthenticatedSigma)

    server.use(
      rest.get('/api/tenants', (req, res, ctx) =>
        res(ctx.json([{ tenantId: 'tenant-1', tenantName: 'tenant-1' }]))
      )
    )

    const { container, rerender } = initialize({
      widgetsResponse: {
        widgets: [reportOne as widgetsService.SigmaWidget],
        isSuccess: true,
        isLoading: false,
        isError: false,
        status: 'success',
      },
      widgetsToDisplay: [reportOne as widgetsService.SigmaWidget],
      selectedReport: reportOne,
      selectedDashboardReport: dashboardWithTenantFilter,
    })

    const firstDashboard = await screen.findByText(
      dashboardNameWithTenantFilter
    )
    await waitFor(() => {
      expect(firstDashboard).toBeInTheDocument()
      const iframe = container.querySelector('iframe')
      // screen.debug(undefined, 9999)
      expect(iframe).not.toBeNull()
    })

    rerender(
      <TranslatedSitePerformance
        site={sixtyMartin}
        dateRange={['2022-01-01T10:15:00', '2022-01-15T10:15:00']}
        analytics={{ track: jest.fn() }}
        featureFlags={{
          hasFeatureToggle: () => true,
        }}
        onViewClick={jest.fn()}
        widgetsResponse={{}}
        widgetsToDisplay={[]}
        selectedReport={reportOne}
        selectedDashboardReport={dashboardWithoutTenantFilter} // rerender dashboard without tenant filter
        onDashboardReportClick={jest.fn()}
      />
    )

    await waitFor(() => {
      expect(screen.queryByText(/tenant-1/i)).toBeNull()
    })
  })
})

const dashboardWithTenantFilter = reportOne.metadata.embedGroup[0]
const dashboardWithoutTenantFilter = reportOne.metadata.embedGroup[1]
const dashboardNameWithTenantFilter = 'dashboard-11'
const dashboardNameWithoutTenantFilter = 'dashboard-12'

const TranslatedSitePerformance =
  withTranslationValues(translation)(SitePerformance)

function getWrapper() {
  return ({ children }) => (
    <BaseWrapper translation={translation}>{children}</BaseWrapper>
  )
}

const initialize = ({
  track = jest.fn(),
  dateRange = ['2022-01-01T10:15:00', '2022-01-15T10:15:00'],
  site = sixtyMartin,
  widgetsResponse = {},
  selectedReport = {} as any,
  selectedDashboardReport = {},
  onDashboardReportClick = jest.fn(),
  widgetsToDisplay,
}) =>
  render(
    <TranslatedSitePerformance
      site={site}
      dateRange={dateRange}
      analytics={{ track }}
      featureFlags={{
        hasFeatureToggle: () => true,
      }}
      widgetsResponse={widgetsResponse}
      selectedReport={selectedReport}
      selectedDashboardReport={{
        ...selectedDashboardReport,
        widgetId: selectedReport.id,
      }}
      onReportSelection={onDashboardReportClick}
      widgetsToDisplay={widgetsToDisplay}
    />,
    {
      wrapper: getWrapper(),
    }
  )
