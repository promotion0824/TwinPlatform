/* eslint-disable @typescript-eslint/no-empty-function */
import { act } from 'react-dom/test-utils'
import { render, screen } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import ConnectivityTable from '../ConnectivityTable'
import { ALL_SITES_TAB } from '../utils'

describe('ConnectivityTable', () => {
  test('Should display correct texts: error state', () => {
    act(() => {
      render(
        <Wrapper>
          <ConnectivityTable
            connectivityTableData={[]}
            connectivityTableState={{
              isError: true,
              isLoading: false,
              isSuccess: false,
            }}
            selectedTab={ALL_SITES_TAB}
            setSelectedTab={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.allSites')).toBeInTheDocument()
    expect(screen.queryByText('headers.offline')).toBeInTheDocument()
    expect(screen.queryByText('headers.online')).toBeInTheDocument()

    expect(screen.queryByText('plainText.errorOccurred')).toBeInTheDocument()
  })

  test('Should display correct texts: loading state', () => {
    act(() => {
      render(
        <Wrapper>
          <ConnectivityTable
            connectivityTableData={[]}
            connectivityTableState={{
              isError: false,
              isLoading: true,
              isSuccess: false,
            }}
            selectedTab={ALL_SITES_TAB}
            setSelectedTab={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.allSites')).toBeInTheDocument()
    expect(screen.queryByText('headers.offline')).toBeInTheDocument()
    expect(screen.queryByText('headers.online')).toBeInTheDocument()

    expect(screen.getByTestId('loader')).toBeInTheDocument()
  })

  test('Should display correct texts: success state, no connectors', () => {
    act(() => {
      render(
        <Wrapper>
          <ConnectivityTable
            connectivityTableData={[]}
            connectivityTableState={{
              isError: false,
              isLoading: false,
              isSuccess: true,
            }}
            selectedTab={ALL_SITES_TAB}
            setSelectedTab={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.allSites')).toBeInTheDocument()
    expect(screen.queryByText('headers.offline')).toBeInTheDocument()
    expect(screen.queryByText('headers.online')).toBeInTheDocument()

    expect(
      screen.queryByText('plainText.noConnectorsFound')
    ).toBeInTheDocument()
  })

  test('Should display correct texts: success state, has connectors', () => {
    const assetClass = 'Office'
    const city = 'Calgary'
    const connectorStatus = 'Online'
    const country = 'Canada'
    const name = 'Eau Claire Tower'
    const dataIn = 123456
    const state = 'AB'
    const isOnline = true

    act(() => {
      render(
        <Wrapper>
          <ConnectivityTable
            connectivityTableData={[
              {
                name,
                country,
                state,
                city,
                assetClass,
                connectorStatus,
                dataIn,
                isOnline,
              },
            ]}
            connectivityTableState={{
              isError: false,
              isLoading: false,
              isSuccess: true,
            }}
            selectedTab={ALL_SITES_TAB}
            setSelectedTab={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.allSites')).toBeInTheDocument()
    expect(screen.queryByText('headers.offline')).toBeInTheDocument()
    expect(screen.queryByText('headers.online')).toBeInTheDocument()

    expect(screen.queryByText('labels.site')).toBeInTheDocument()
    expect(screen.queryByText('headers.connectorStatus')).toBeInTheDocument()
    expect(screen.queryByText('plainText.dataIn')).toBeInTheDocument()
    expect(screen.queryByText('plainText.action')).toBeInTheDocument()

    expect(screen.queryByText(name)).toBeInTheDocument()
    expect(
      screen.queryByText(`${city}, ${state}, ${country}`)
    ).toBeInTheDocument()
    expect(
      screen.queryByText(`${connectorStatus} headers.online`)
    ).toBeInTheDocument()
    expect(screen.queryByText(dataIn.toLocaleString())).toBeInTheDocument()
  })
})
