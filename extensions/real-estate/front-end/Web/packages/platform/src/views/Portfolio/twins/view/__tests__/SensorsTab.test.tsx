import { render, screen, within } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { SelectedPointsProvider } from '../../../../../components/MiniTimeSeries'
import SensorsTab from '../SensorsTab'

const siteId = 'mySite-12345'
const twinId = 'twin-ABCD'

const Wrapper = ({ children }) => (
  <BaseWrapper>
    <SelectedPointsProvider>{children}</SelectedPointsProvider>
  </BaseWrapper>
)

const server = setupServer(
  rest.get(`/api/sites/${siteId}/twins/${twinId}/points`, (_req, res, ctx) =>
    res(
      ctx.json([
        {
          name: 'Point 1 - No connector',
          externalId: '1',
          properties: {
            siteID: {
              value: siteId,
            },
          },
        },
        {
          name: 'Point 2 - Device D & Connector A',
          externalId: '2',
          device: {
            id: 'dddd',
            name: 'Device D',
          },
          properties: {
            connectorID: {
              value: 'aaaa',
            },
            siteID: {
              value: siteId,
            },
          },
          connectorName: 'Connector A',
        },
        {
          name: 'Point 3 - Connector B',
          externalId: '3',
          properties: {
            connectorID: {
              value: 'bbbb',
            },
            siteID: {
              value: siteId,
            },
          },
          connectorName: 'Connector B',
        },
        {
          name: 'Point 4 - No connector',
          externalId: '4',
          properties: {
            siteID: {
              value: siteId,
            },
          },
        },
        {
          name: 'Point 5 - Device E & Connector C',
          externalId: '5',
          device: {
            id: 'eeee',
            name: 'Device E',
          },
          properties: {
            connectorID: {
              value: 'C',
            },
            siteID: {
              value: siteId,
            },
          },
          connectorName: 'Connector C',
        },
        {
          name: 'Point 6 - Connector B',
          externalId: '6',
          properties: {
            connectorID: {
              value: 'bbbb',
            },
            siteID: {
              value: siteId,
            },
          },
          connectorName: 'Connector B',
        },
      ])
    )
  ),
  rest.get(
    `/api/sites/${siteId}/assets/${twinId}/pinOnLayer`,
    (_req, res, ctx) => res(ctx.json({}))
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('SensorsTab', () => {
  test('Sensor groups should be sorted by hostedBy and connector', async () => {
    const expectedHeaders = [
      { hostedBy: 'Device D', connector: 'Connector A', hasHeader: true },
      { hostedBy: 'Device E', connector: 'Connector C', hasHeader: true },
      { connector: 'Connector B', hasHeader: true },
    ]

    const expectedListings = [
      ['Point 2 - Device D & Connector A'],
      ['Point 5 - Device E & Connector C'],
      ['Point 3 - Connector B', 'Point 6 - Connector B'],
      ['Point 1 - No connector', 'Point 4 - No connector'],
    ]

    render(
      <SensorsTab
        twin={{ siteID: siteId, uniqueID: twinId }}
        selectTimeSeriesTab={jest.fn}
        missingSensors={[]}
        count={expectedListings.length}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const groupHeaders = await screen.findAllByTestId('groupHeader')
    const allLists = await screen.findAllByRole('list')

    expect(groupHeaders.length).toBe(expectedHeaders.length)
    expect(allLists.length).toBe(expectedListings.length)

    groupHeaders.forEach((groupHeader, index) => {
      if (expectedHeaders[index].hasHeader) {
        if (expectedHeaders[index].hostedBy) {
          expect(
            within(groupHeader).getByText(
              expectedHeaders[index].hostedBy as string
            )
          ).toBeInTheDocument()
        }
        expect(
          within(groupHeader).getByText(expectedHeaders[index].connector)
        ).toBeInTheDocument()
      }
    })

    allLists.forEach((list, listIndex) => {
      const items = within(list).getAllByRole('listitem')
      const expectedItems = expectedListings[listIndex]
      expect(items).toHaveLength(expectedItems.length)
      items.forEach((item, index) => {
        expect(within(item).getByText(expectedItems[index])).toBeInTheDocument()
      })
    })
  })
})
