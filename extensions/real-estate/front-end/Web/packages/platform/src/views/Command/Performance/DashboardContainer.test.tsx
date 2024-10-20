import { render, screen } from '@testing-library/react'
import '@willow/common/utils/testUtils/matchMediaMock'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import { EmbedGroup } from '../../../components/Reports/ReportsLayout'
import DashboardContainer from './DashboardContainer'

describe('DashboardContainer.tsx', () => {
  test('when isFetchOrAuthLoading is true, should only see the spinner not the children', async () => {
    const { container } = render(
      <DashboardContainerWithProvider
        isFetchOrAuthLoading
        isFetchOrAuthError={false}
        isGetWidgetsSuccess={false}
        selectedReport={{ id: '123' }}
        selectedDashboardReport={dashboardReport}
        isAuthReportSuccess={false}
      >
        <div className="reportClass">I Am Report</div>
      </DashboardContainerWithProvider>
    )

    expect(container.querySelector('.progress')).toBeInTheDocument() // the spinner
    expect(screen.queryByText('I Am Report')).not.toBeInTheDocument()
  })

  test('when isFetchOrAuthError is true, should only see erorr message', async () => {
    const { findByText } = render(
      <DashboardContainerWithProvider
        isFetchOrAuthLoading={false}
        isFetchOrAuthError
        isGetWidgetsSuccess={false}
        selectedReport={{ id: '123' }}
        selectedDashboardReport={dashboardReport}
        isAuthReportSuccess={false}
      >
        <div className="reportClass">I Am Report</div>
      </DashboardContainerWithProvider>
    )

    const errorMessage = await findByText('An Error Has Occurred')
    expect(screen.queryByText('I Am Report')).not.toBeInTheDocument()
    expect(errorMessage).toBeInTheDocument()
  })

  test('when isGetWidgetsSuccess is false, should see No Report Found message', async () => {
    render(
      <DashboardContainerWithProvider
        isFetchOrAuthLoading={false}
        isFetchOrAuthError={false}
        isGetWidgetsSuccess={false}
        selectedReport={{ id: '123' }}
        selectedDashboardReport={dashboardReport}
        isAuthReportSuccess={false}
      >
        <div className="reportClass">I Am Report</div>
      </DashboardContainerWithProvider>
    )

    const noReportMessage = await screen.findByText('No report found')
    expect(screen.queryByText('I Am Report')).not.toBeInTheDocument()
    expect(noReportMessage).toBeInTheDocument()
  })

  test('when selectedReport.id or selectedDashboardReport?.embedPath do not exist, should see No Report Found message', async () => {
    const assertNoReportMessageExists = async () => {
      expect(screen.queryByText('I Am Report')).not.toBeInTheDocument()
      expect(await screen.findByText('No report found')).toBeInTheDocument()
    }

    const { rerender } = render(
      <DashboardContainerWithProvider
        isFetchOrAuthLoading={false}
        isFetchOrAuthError={false}
        isGetWidgetsSuccess
        selectedReport={undefined}
        selectedDashboardReport={dashboardReport}
        isAuthReportSuccess={false}
      >
        <div className="reportClass">I Am Report</div>
      </DashboardContainerWithProvider>
    )

    await assertNoReportMessageExists()

    rerender(
      <DashboardContainerWithProvider
        isFetchOrAuthLoading={false}
        isFetchOrAuthError={false}
        isGetWidgetsSuccess
        selectedReport={{ id: '1' }}
        selectedDashboardReport={undefined}
        isAuthReportSuccess={false}
      >
        <div className="reportClass">I Am Report</div>
      </DashboardContainerWithProvider>
    )

    await assertNoReportMessageExists()
  })

  test('should see children when fetch and auth are successful', async () => {
    render(
      <DashboardContainerWithProvider
        isFetchOrAuthLoading={false}
        isFetchOrAuthError={false}
        isGetWidgetsSuccess
        selectedReport={{ id: '1' }}
        selectedDashboardReport={dashboardReport}
        isAuthReportSuccess
      >
        <div className="reportClass">I Am Report</div>
      </DashboardContainerWithProvider>
    )

    const childrenReport = await screen.findByText('I Am Report')
    expect(childrenReport).toBeInTheDocument()
  })
})

const DashboardContainerWithProvider = (
  props: React.ComponentProps<typeof DashboardContainer>
) => (
  <Wrapper
    translation={{
      'plainText.errorOccurred': 'An Error Has Occurred',
      'plainText.NoDashboardsAvailableForThisLocation': 'No report found',
    }}
  >
    <DashboardContainer {...props} />
  </Wrapper>
)

const dashboardReport: EmbedGroup = {
  name: 'dashboardReport-1',
  embedPath: 'path-1',
  order: 0,
}
