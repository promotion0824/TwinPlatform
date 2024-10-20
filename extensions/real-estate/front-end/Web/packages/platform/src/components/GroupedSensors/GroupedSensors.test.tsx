import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { SelectedPointsProvider } from '../MiniTimeSeries'
import GroupedSensors from './GroupedSensors'

const handleMonitorPoint = jest.fn()

const Wrapper = ({ children }) => (
  <BaseWrapper>
    <SelectedPointsProvider>{children}</SelectedPointsProvider>
  </BaseWrapper>
)

describe('GroupedSensors', () => {
  test.each([
    {
      description: 'With hostedBy device & connector',
      hostedBy: { id: 'DeviceId', name: 'My Device' },
      connector: { id: 'ConnectorId', name: 'My Connector' },
      hasHeader: true,
    },
    {
      description: 'With Connector',
      connector: { id: 'ConnectorId', name: 'Just Connector' },
      hasHeader: true,
    },
    { description: 'Without Connector', hasHeader: false },
  ])('$description', ({ hostedBy, connector, hasHeader }) => {
    render(
      <GroupedSensors
        hostedBy={hostedBy}
        connector={connector}
        points={[
          {
            name: 'VFD Inverter Temp Sensor',
            externalId: '-FACILITY-MANWEST-DFSDF',
            trendId: '1',
            properties: { siteID: { value: '1234' } },
            connectorName: 'Connector Temp',
          },
        ]}
        onTogglePoint={handleMonitorPoint}
      />,
      {
        wrapper: Wrapper,
      }
    )

    if (hasHeader) {
      if (hostedBy) {
        expect(screen.getByText(hostedBy.name)).toBeInTheDocument()
      }
      if (connector) {
        expect(screen.getByText(connector.name)).toBeInTheDocument()
      }
    } else {
      expect(screen.queryByTestId('groupHeader')).not.toBeInTheDocument()
    }

    expect(screen.getAllByRole('listitem')).toHaveLength(1)

    const pointItem = screen.getByRole('listitem')

    expect(
      within(pointItem).getByText('VFD Inverter Temp Sensor')
    ).toBeInTheDocument()
    userEvent.click(within(pointItem).getByRole('button'))

    expect(handleMonitorPoint).toBeCalledWith(
      {
        sitePointId: '1234_1',
        name: 'VFD Inverter Temp Sensor',
      },
      true
    )
  })
})
