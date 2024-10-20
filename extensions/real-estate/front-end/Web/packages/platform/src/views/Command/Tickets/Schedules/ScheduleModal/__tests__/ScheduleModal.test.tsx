import { ReactNode, ReactPortal } from 'react'
import ReactDOM from 'react-dom'
import { matchRequestUrl, rest } from 'msw'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import {
  supportDropdowns,
  openDropdown,
} from '@willow/common/utils/testUtils/dropdown'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { TicketStatusesStubProvider } from '@willow/common'
import ScheduleModal from '../ScheduleModal'
import { ScheduleModalProvider } from '../Hooks/ScheduleModalProvider'
import { setupTestServer } from '../../../../../../mockServer/testServer'
import { SitesProvider } from '../../../../../../providers'

supportDropdowns()

const { server } = setupTestServer()

beforeEach(() => {
  server.use(
    rest.get('/api/sites/123/tickettemplate/schedule123', (_req, res, ctx) =>
      res(
        ctx.json({
          recurrence: {
            startDate: '2022-10-27T00:00:00',
            occurs: 'weekly',
            interval: 3,
          },
          // Provide enough initial data so we can save without getting
          // a bunch of "this field is required" validation errors.
          assets: [
            {
              id: 'Asset 1',
              assetId: 'asset1',
              assetName: 'FS-DB-VESDA-L10',
            },
          ],
          summary: 'summary',
          description: 'description',
          reporterName: 'Bob',
          reporterEmail: 'reporter@willowinc.com',
          reporterPhone: '99999999999',
        })
      )
    ),
    rest.post('/api/sites/:siteId/tickettemplate', (_req, res, ctx) =>
      res(ctx.json({}))
    )
  )
  server.listen()
})
afterEach(() => {
  server.restoreHandlers()
})
afterAll(() => server.close())

function Wrapper({ children }: { children: ReactNode }) {
  return (
    <BaseWrapper>
      <SitesProvider sites={[{ id: 'site123' }] as any}>
        <ScheduleModalProvider>
          <TicketStatusesStubProvider>{children}</TicketStatusesStubProvider>
        </ScheduleModalProvider>
      </SitesProvider>
    </BaseWrapper>
  )
}

// This is the best way I have found to make the test work despite the fact
// that the submit button is inside a portal.
beforeAll(() => {
  jest
    .spyOn(ReactDOM, 'createPortal')
    .mockImplementation((element: ReactPortal) => element)
})
afterAll(() => {
  ;(
    ReactDOM.createPortal as unknown as jest.Mock<typeof ReactDOM.createPortal>
  ).mockReset()
})

test('should allow the user to set a frequency of 8 years and make the right POST request', async () => {
  let submittedRequestBody: any
  server.events.on('request:start', (req) => {
    if (
      req.method.toLowerCase() === 'post' &&
      matchRequestUrl(req.url, '/api/sites/:siteId/tickettemplate').matches
    ) {
      submittedRequestBody = req.body
    }
  })

  render(
    <ScheduleModal
      siteId="123"
      scheduleId="schedule123"
      isReadOnly={false}
      onClose={() => {}}
    />,
    { wrapper: Wrapper }
  )

  await waitFor(() =>
    expect(screen.getByTestId('recurrence-interval')).toBeInTheDocument()
  )

  userEvent.clear(screen.getByTestId('recurrence-interval'))
  userEvent.type(screen.getByTestId('recurrence-interval'), '8')
  openDropdown(screen.getByTestId('recurrence-occurs'))
  userEvent.click(screen.getByText('plainText.years'))

  userEvent.click(screen.getByText('plainText.scheduleTicket'))

  await waitFor(() => expect(submittedRequestBody).toBeDefined())
  expect(submittedRequestBody.recurrence.occurs).toBe('yearly')
  expect(submittedRequestBody.recurrence.interval).toBe('8')
})
