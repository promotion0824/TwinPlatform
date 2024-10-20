import { act } from 'react-dom/test-utils'
import { render, screen } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import ConnectivityMetrics from '../ConnectivityMetric'

const sitesOnline = 'Sites online'
const siteOnlineCountValue = 60
const connectionErrors = 'Connection errors'
const connectionErrorCountValue = 6

function constructRenderMetricObject({
  siteOnlineCount,
  connectionErrorCount,
}: {
  siteOnlineCount: number
  connectionErrorCount: number
}) {
  return {
    'Sites online': {
      count: siteOnlineCount.toLocaleString(),
      color: 'green',
      icon: 'buildingNew',
      type: sitesOnline,
    },

    'Connection errors': {
      count: connectionErrorCount.toLocaleString(),
      color: 'red',
      icon: 'warning',
      type: connectionErrors,
    },
  }
}

describe('ConnectivityMetrics', () => {
  test('Should display correct texts', () => {
    act(() => {
      render(
        <Wrapper>
          <ConnectivityMetrics
            renderMetricObject={constructRenderMetricObject({
              siteOnlineCount: siteOnlineCountValue,
              connectionErrorCount: connectionErrorCountValue,
            })}
          />
        </Wrapper>
      )
    })

    expect(screen.getByText('plainText.connectivity')).toBeInTheDocument()

    expect(screen.getByText(siteOnlineCountValue)).toBeInTheDocument()
    expect(screen.getByText(sitesOnline)).toBeInTheDocument()

    expect(screen.getByText(connectionErrorCountValue)).toBeInTheDocument()
    expect(screen.getByText(connectionErrors)).toBeInTheDocument()
  })
})
