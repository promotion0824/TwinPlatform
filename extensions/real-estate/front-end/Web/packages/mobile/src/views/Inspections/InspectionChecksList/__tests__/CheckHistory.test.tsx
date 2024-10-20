import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { QueryClient, QueryClientProvider } from 'react-query'
import { v4 as uuidv4 } from 'uuid'
import BaseWrapper from '../../../../utils/testUtils/Wrapper'
import CheckHistory from '../CheckHistory'
import { Check } from '../types'

const siteId = uuidv4()
const checkId = uuidv4()
const inspectionId = uuidv4()

const makeCheck = (type, typeValue, decimalPlaces = 2): Check => ({
  id: checkId,
  name: `My ${type} check`,
  type,
  inspectionId,
  sortOrder: 1,
  typeValue,
  decimalPlaces,
})

const makeRecord = (
  entry:
    | { stringValue: string }
    | { numberValue: number }
    | { dateValue: string } = { numberValue: 2 }
) => ({
  ...entry,
  id: uuidv4(),
  inspectionId,
  checkId,
  status: 'completed',
  submittedUserId: '26936cf4-c44a-4cb0-b7b1-5e39b375cec1',
  submittedDate: '2022-11-18T05:29:00.823Z',
  submittedSiteLocalDate: '2022-11-18T16:29:00.823Z',
  enteredBy: {
    firstName: 'Investa-AU',
    lastName: 'SiteAdmin',
  },
  effectiveDate: '2022-11-16T21:00:00.000Z',
  notes: 'check 1',
  attachments: [
    {
      id: '4e81b897-d7f6-4666-82eb-7bfaf325ab0a',
      type: 'image',
      fileName: 'book.png',
      createdDate: '2022-11-18T05:29:00.594Z',
      previewUrl:
        '/au/api/images/2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b/sites/404bd33c-a697-4027-b6a6-677e30a53d07/checkRecords/831ff746-3c7c-452d-bf9b-64e10c20efc3/4e81b897-d7f6-4666-82eb-7bfaf325ab0a_1_w100_h100.jpg',
      url: '/au/api/images/2ea69d3c-8b2b-4829-8e43-fd0fdd2a7a6b/sites/404bd33c-a697-4027-b6a6-677e30a53d07/checkRecords/831ff746-3c7c-452d-bf9b-64e10c20efc3/4e81b897-d7f6-4666-82eb-7bfaf325ab0a_0.jpg',
    },
  ],
})

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
})

const Wrapper = ({ children }) => (
  <BaseWrapper intl={{ locale: 'en-US', timezone: 'Australia/Sydney' }}>
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  </BaseWrapper>
)

const server = setupServer()

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  queryClient.clear()
})
afterAll(() => server.close())

describe('CheckHistory', () => {
  test.each([
    {
      checkType: 'numeric',
      decimalPlaces: 0,
      entry: { numberValue: 0 },
      expectedEntry: '0',
    },
    {
      checkType: 'numeric',
      decimalPlaces: 1,
      entry: { numberValue: 20.2 },
      expectedEntry: '20.2',
    },
    {
      checkType: 'numeric',
      decimalPlaces: 0,
      entry: { numberValue: 20.2 },
      expectedEntry: '20',
    },
    {
      checkType: 'numeric',
      decimalPlaces: 0,
      entry: { stringValue: 'incorrect value' },
      expectedEntry: '-',
    },
    {
      checkType: 'total',
      decimalPlaces: 1,
      entry: { numberValue: 20.2 },
      expectedEntry: '20.2',
    },
    {
      checkType: 'total',
      decimalPlaces: 0,
      entry: { stringValue: 'incorrect value' },
      expectedEntry: '-',
    },
    { checkType: 'list', entry: { stringValue: 'a' }, expectedEntry: 'A' },
    {
      checkType: 'list',
      entry: { numberValue: 1234 },
      expectedEntry: '-',
    },
    {
      checkType: 'date',
      entry: { dateValue: '2022-11-23T16:00:00.000Z' },
      expectedEntry: 'Nov 24, 2022',
    },
    {
      checkType: 'date',
      entry: { stringValue: 'incorrect value' },
      expectedEntry: '-',
    },
  ])(
    'Check history header for $checkType with entry $entry should display $expectedEntry',
    async ({ checkType, decimalPlaces, entry, expectedEntry }) => {
      server.use(
        rest.get(
          '/mobile-web/us/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
          (_req, res, ctx) => res(ctx.json([makeRecord(entry)]))
        )
      )
      render(
        <CheckHistory
          siteId={siteId}
          check={makeCheck(checkType, '', decimalPlaces)}
          attachmentEntries={[]}
        />,
        {
          wrapper: Wrapper,
        }
      )

      await waitFor(() =>
        expect(
          screen.queryByRole('img', { name: 'loading' })
        ).not.toBeInTheDocument()
      )

      const pastCheckRecordList = screen.getByRole('list')
      const listItems = within(pastCheckRecordList).getAllByRole('listitem')
      expect(listItems).toHaveLength(1)

      expect(within(listItems[0]).getByTestId('entry')).toHaveTextContent(
        `Entry: ${expectedEntry}`
      )
      expect(within(listItems[0]).getByTestId('timestamp')).toHaveTextContent(
        'Timestamp: Nov 18, 2022, 16:29'
      )
    }
  )

  test('Toggling list of check history', async () => {
    server.use(
      rest.get(
        '/mobile-web/us/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
        (_req, res, ctx) => res(ctx.json([makeRecord(), makeRecord()]))
      )
    )
    render(
      <CheckHistory
        siteId={siteId}
        check={makeCheck('numeric', '200', 2)}
        attachmentEntries={[]}
      />,
      { wrapper: Wrapper }
    )

    await waitFor(() =>
      expect(
        screen.queryByRole('img', { name: 'loading' })
      ).not.toBeInTheDocument()
    )

    const pastCheckRecordList = screen.getByRole('list')
    const listItems = within(pastCheckRecordList).getAllByRole('listitem')
    expect(listItems).toHaveLength(2)

    // Toggle first item
    userEvent.click(within(listItems[0]).getByRole('button'))
    await waitFor(() => {
      expect(within(listItems[0]).queryByRole('form')).toBeInTheDocument()
      expect(within(listItems[1]).queryByRole('form')).not.toBeInTheDocument()
    })

    // Toggle second item
    userEvent.click(within(listItems[1]).getByRole('button'))
    await waitFor(() => {
      expect(within(listItems[1]).queryByRole('form')).toBeInTheDocument()
      expect(within(listItems[0]).queryByRole('form')).not.toBeInTheDocument()
    })
  })

  test('Empty check history', async () => {
    server.use(
      rest.get(
        '/mobile-web/us/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
        (_req, res, ctx) => res(ctx.json([]))
      )
    )

    render(
      <CheckHistory
        siteId={siteId}
        check={makeCheck('numeric', '200', 2)}
        attachmentEntries={[]}
      />,
      { wrapper: Wrapper }
    )

    await waitFor(() =>
      expect(
        screen.queryByText('No inspection check history found')
      ).toBeInTheDocument()
    )
  })

  test('Check history not found', async () => {
    server.use(
      rest.get(
        '/mobile-web/us/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
        (_req, res, ctx) =>
          res(ctx.status(204), ctx.json({ message: 'No content' }))
      )
    )

    render(
      <CheckHistory
        siteId={siteId}
        check={makeCheck('numeric', '200', 2)}
        attachmentEntries={[]}
      />,
      { wrapper: Wrapper }
    )

    await waitFor(() =>
      expect(
        screen.queryByText('No inspection check history found')
      ).toBeInTheDocument()
    )
  })

  test('Fetching check history when network is not available', async () => {
    server.use(
      rest.get(
        '/mobile-web/us/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
        (_req, res, _ctx) => res.networkError('Unable to connect')
      )
    )

    render(
      <CheckHistory
        siteId={siteId}
        check={makeCheck('numeric', '200', 2)}
        attachmentEntries={[]}
      />,
      { wrapper: Wrapper }
    )

    await waitFor(() =>
      expect(
        screen.queryByText('Check history is not available in offline mode.')
      ).toBeInTheDocument()
    )
  })
})
