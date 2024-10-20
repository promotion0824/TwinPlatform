/* eslint-disable @typescript-eslint/no-non-null-assertion */
import _ from 'lodash'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { DateTime } from 'luxon'
import 'fake-indexeddb/auto'
import { DefaultRequestBody, rest } from 'msw'
import { setupServer } from 'msw/node'
import {
  supportDropdowns,
  openDropdown,
} from '@willow/common/utils/testUtils/dropdown'
import BaseWrapper from '../../../../utils/testUtils/Wrapper'
import LayoutStubProvider from '../../../Layout/Layout/LayoutStubProvider'
import { Check, Inspection, InspectionRecord, IStore } from '../types'
import { makeInspection, makeInspectionRecord } from '../testUtils'
import { InspectionRecordsProvider } from '../InspectionRecordsContext'
import { DummyIndexedDbStore, IndexedDbStore } from '../InspectionRecordsDb'
import InspectionsApi from '../api'
import { useMemo } from 'react'
import { useApi } from '@willow/mobile-ui'
import { InspectionsNew } from '../../Inspections'
import { assertPathnameContains } from '@willow/common/utils/testUtils/LocationDisplay'

supportDropdowns()

// Pick a time zone that is not UTC (so we can test that we are
// converting to local time zone), and doesn't have daylight saving time.
const timezone = 'Australia/Brisbane'

const toggleCheckRecord = (checkItem) => {
  userEvent.click(
    screen.getByRole('button', {
      name: new RegExp(`${checkItem.name}.+`),
    })
  )
}

function setup({
  inspection,
  inspectionRecord = makeInspectionRecord(inspection),
  store,
}: {
  inspection: Inspection
  inspectionRecord?: InspectionRecord
  store?: IStore
}) {
  const checkRecords = {}
  const attachments = {}
  const submitRequests: DefaultRequestBody[] = []
  let isOnline = true

  server.use(
    rest.get(
      '/mobile-web/us/api/sites/:siteId/inspections/:inspectionId/lastRecord',
      (_req, res, ctx) => {
        if (!isOnline) {
          return res.networkError('offline')
        } else {
          return res(ctx.json(inspectionRecord))
        }
      }
    ),

    /**
     * Update the current inspection record's value for the specified check record.
     * Simulates switching dependent checks' states between notRequired and overdue,
     * but this logic is definitely not complete.
     */
    // eslint-disable-next-line complexity
    rest.post(
      '/mobile-web/us/api/sites/:siteId/syncInspectionRecords',
      (req, res, ctx) => {
        submitRequests.push(req.body)
        if (!isOnline) {
          return res.networkError('offline')
        }

        const body = req.body as any
        for (const ir of body.inspectionRecords) {
          if (ir.id !== inspectionRecord.id) {
            throw new Error('...')
          }
          for (const checkRecord of ir.checkRecords) {
            checkRecords[checkRecord.id as string] = {
              ..._.omit(checkRecord, ['id', 'attachments']),
              attachments: [],
            }
            const index = inspectionRecord.checkRecords.findIndex(
              (c) => c.id === checkRecord.id
            )
            inspectionRecord.checkRecords[index] = {
              id: checkRecord.id,
              inspectionId: inspection.id,
              checkId: inspectionRecord.checkRecords[index].checkId,
              inspectionRecordId: inspectionRecord.id,
              status: 'completed',
              submittedUserId: '26936cf4-c44a-4cb0-b7b1-5e39b375cec1',
              submittedDate: '2022-11-03T23:50:37.043Z',
              submittedSiteLocalDate: '2022-11-04T10:50:37.043Z',
              effectiveDate: '2022-11-03T20:00:00.000Z',
              ..._.omit(checkRecord, ['id', 'attachments']),
              attachments: [],
            }

            for (const cr of inspectionRecord.checkRecords) {
              const check = inspection.checks.find((c) => c.id === cr.checkId)
              if (check == null) {
                throw new Error(`Did not find check for ${cr.id}`)
              }
              if (cr.status !== 'completed') {
                if (
                  check.dependencyId != null &&
                  !inspectionRecord.checkRecords.some(
                    (r) =>
                      r.checkId === check.dependencyId &&
                      'stringValue' in r &&
                      r.stringValue === check.dependencyValue
                  )
                ) {
                  cr.status = 'notRequired'
                } else {
                  cr.status = 'overdue'
                }
              }
            }
          }
        }

        return res(
          ctx.json({
            inspectionRecords: body.inspectionRecords.map((ir) => ({
              id: ir.id,
              checkRecords: ir.checkRecords.map((cr) => ({
                id: cr.id,
                result: 'Success',
              })),
            })),
          })
        )
      }
    ),

    /**
     * Add the attachment to the `attachments` array. Since we stub
     * `URL.createObjectURL` which is used by the attachment logic,
     * we cannot retrieve the actual content of the attachments.
     */
    rest.post(
      '/mobile-web/us/api/sites/:siteId/checkRecords/:checkRecordId/attachments',
      (req, res, ctx) => {
        const body = req.body as FormData
        attachments[req.params.checkRecordId as string] = {
          fileName: body.get('fileName'),
          file: body.get('attachmentFile'),
        }
        return res(ctx.json({ status: 'ok' }))
      }
    )
  )

  const renderResult = render(<InspectionsNew />, {
    wrapper: ({ children }) => {
      const api = useApi()
      const memoizedStore = useMemo(
        () => store ?? new DummyIndexedDbStore(),
        []
      )
      const inspectionsApi = useMemo(() => new InspectionsApi(api), [api])
      return (
        <Wrapper
          initialEntries={[
            `/sites/site123/inspectionZones/zone123/inspections/${inspection.id}`,
          ]}
        >
          <InspectionRecordsProvider api={inspectionsApi} store={memoizedStore}>
            {children}
          </InspectionRecordsProvider>
        </Wrapper>
      )
    },
  })

  return {
    result: renderResult,
    inspectionRecord,
    checkRecords,
    attachments,
    submitRequests,
    /**
     * If set to false, the endpoints we have added in this setup function will
     * return a network error.
     */
    setOnline: (val: boolean) => {
      isOnline = val
    },
  }
}

const server = setupServer()

/**
 * Because
 * 1. Sometimes we fake timers and sometimes we don't
 * 2. Jest does not provide a good way to determine whether timers are faked [1]
 * 3. Jest outputs annoying warnings if we try to disable fake timers when they
 *    aren't enabled.
 *
 * [1]: https://github.com/facebook/jest/issues/10555
 */
let areTimersFake = false

beforeAll(() => server.listen())
afterEach(() => server.restoreHandlers())
afterEach(() => {
  if (areTimersFake) {
    jest.runOnlyPendingTimers()
    jest.useRealTimers()
    areTimersFake = false
  }
})
afterAll(() => server.close())

function useFakeTimers() {
  jest.useFakeTimers()
  areTimersFake = true
}

// This function is used in the code that processes attachments,
// but does not exist in jsdom.
beforeEach(() => {
  window.URL.createObjectURL = jest.fn()
})
afterEach(() => {
  ;(window.URL.createObjectURL as any).mockReset()
})

const numericCheck = {
  id: 'check1',
  sortOrder: 0,
  name: 'numeric check',
  type: 'numeric',
  typeValue: 'widgets',
  decimalPlaces: 2,
  minValue: 0,
  maxValue: 10,
} as const

const totalCheck = {
  id: 'check1',
  sortOrder: 0,
  name: 'total check',
  type: 'total',
  typeValue: 'widgets',
  decimalPlaces: 2,
} as const

const listCheck = {
  id: 'check1',
  sortOrder: 0,
  name: 'list check',
  type: 'list',
  typeValue: 'listitem1|listitem2',
} as const

const dateCheck = {
  id: 'check1',
  sortOrder: 0,
  name: 'date check',
  type: 'date',
  typeValue: '',
} as const
describe('InspectionChecksList', () => {
  describe('check types', () => {
    test('numeric check', async () => {
      const { inspectionRecord, checkRecords } = setup({
        inspection: makeInspection({ checks: [numericCheck] }),
      })
      await waitFor(() =>
        expect(screen.getByText('numeric check')).toBeInTheDocument()
      )

      toggleCheckRecord(numericCheck) // To open a numericCheck

      // We cannot rely on `userEvent.type` here - it will only type the first character.
      // This appears to be very difficult to fix.
      fireEvent.change(screen.getByLabelText('Entry'), {
        target: { value: '234' },
      })
      userEvent.click(screen.getByText('Submit'))

      await waitFor(() => {
        expect(checkRecords[inspectionRecord.checkRecords[0].id]).toMatchObject(
          {
            numberValue: 234,
            notes: '',
            attachments: [],
          }
        )
      })
    })

    test('total check', async () => {
      const { inspectionRecord, checkRecords } = setup({
        inspection: makeInspection({ checks: [totalCheck] }),
      })
      await waitFor(() =>
        expect(screen.getByText('total check')).toBeInTheDocument()
      )

      toggleCheckRecord(totalCheck) // To open a totalCheck

      // We cannot rely on `userEvent.type` here - it will only type the first character.
      // This appears to be very difficult to fix.
      fireEvent.change(screen.getByLabelText('Entry'), {
        target: { value: '234' },
      })
      userEvent.click(screen.getByText('Submit'))

      await waitFor(() =>
        expect(checkRecords[inspectionRecord.checkRecords[0].id]).toMatchObject(
          {
            numberValue: 234,
            notes: '',
            attachments: [],
          }
        )
      )
    })

    test('list check', async () => {
      const { inspectionRecord, checkRecords } = setup({
        inspection: makeInspection({ checks: [listCheck] }),
      })
      await waitFor(() =>
        expect(screen.getByText('list check')).toBeInTheDocument()
      )

      toggleCheckRecord(listCheck) // to open a listCheck

      openDropdown(screen.getByText('Select value'))

      userEvent.click(screen.getByText('Listitem1'))

      userEvent.click(screen.getByText('Submit'))

      await waitFor(() =>
        expect(checkRecords[inspectionRecord.checkRecords[0].id]).toMatchObject(
          {
            stringValue: 'listitem1',
            notes: '',
            attachments: [],
          }
        )
      )
    })

    test('date check', async () => {
      const { inspectionRecord, checkRecords } = setup({
        inspection: makeInspection({ checks: [dateCheck] }),
      })
      await waitFor(() =>
        expect(screen.getByText('date check')).toBeInTheDocument()
      )

      toggleCheckRecord(dateCheck) // To open a dateCheck

      openDropdown(screen.getByText('- Date -'))

      // Depending on the date, there may be a 25 on the calendar in the
      // previous month and the current month.
      userEvent.click(screen.getAllByText(25)[0])

      userEvent.click(screen.getByText('Submit'))

      await waitFor(() => {
        const { dateValue } = checkRecords[inspectionRecord.checkRecords[0].id]
        expect(DateTime.fromISO(dateValue, { zone: timezone }).day).toEqual(25)
      })
    })
  })

  test('Adding an attachment to a check', async () => {
    const {
      inspectionRecord,
      attachments,
      result: { container },
    } = setup({
      inspection: makeInspection({ checks: [numericCheck] }),
    })

    await waitFor(() =>
      expect(screen.getByText('numeric check')).toBeInTheDocument()
    )

    toggleCheckRecord(numericCheck) // To open a numericCheck

    // We cannot rely on `userEvent.type` here - it will only type the first character.
    // This appears to be very difficult to fix.
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '234' },
    })

    const file = new File(['(⌐□_□)'], 'chucknorris.png', { type: 'image/png' })

    expect(container.querySelector('[type=file]')).toBeInTheDocument()

    const input = container.querySelector('[type=file]')

    fireEvent.change(input!, {
      target: { files: { item: () => file, length: 1, 0: file } },
    })

    await waitFor(() => {
      const image = screen.queryByAltText('chucknorris.png')
      expect(image).not.toBeNull()
    })

    // Not sure what to listen for to eliminate this
    await new Promise((r) => setTimeout(r, 1000))

    userEvent.click(screen.getByText('Submit'))

    await waitFor(() =>
      expect(attachments[inspectionRecord.checkRecords[0].id]).toBeDefined()
    )
    const attachment = attachments[inspectionRecord.checkRecords[0].id]
    expect(attachment.fileName).toBe('chucknorris.png')

    // Because we mocked URL.createObjectURL, we do not have access to the
    // actual content of the file.
    expect(attachment.file).toBeDefined()
  })

  test('Advancing through checks without dependencies', async () => {
    const numericCheck1 = {
      id: 'check-1-id',
      sortOrder: 0,
      name: 'numeric check 1',
      type: 'numeric',
      typeValue: 'widgets',
      decimalPlaces: 2,
      minValue: 0,
      maxValue: 10,
    }

    const numericCheck2 = {
      id: 'check-2-id',
      sortOrder: 1,
      name: 'numeric check 2',
      type: 'numeric',
      typeValue: 'widgets',
      decimalPlaces: 2,
      minValue: 0,
      maxValue: 10,
    }

    // When we submit a check, the system automatically advances to the next
    // check. This test also checks that we correctly move to the "Sync
    // Pending" state if we submit a check while offline, and that the
    // subsequent submit (while online) updates both entries.

    const { inspectionRecord, checkRecords, setOnline } = setup({
      inspection: makeInspection({
        checks: [numericCheck1, numericCheck2] as Check[],
      }),
    })
    await waitFor(() =>
      expect(screen.getByText('numeric check 1')).toBeInTheDocument()
    )

    toggleCheckRecord(numericCheck1)

    // We cannot rely on `userEvent.type` here - it will only type the first character.
    // This appears to be very difficult to fix.
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '234' },
    })

    setOnline(false)
    userEvent.click(screen.getByText('Submit'))

    // Timeout to wait for server request to ends so Network Error observed
    // here due to offline mode before the next action.
    await new Promise((r) => setTimeout(r, 1000))

    await waitFor(() =>
      expect(screen.getByText('Sync Pending')).toBeInTheDocument()
    )

    // We cannot rely on `userEvent.type` here - it will only type the first character.
    // This appears to be very difficult to fix.
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '345' },
    })

    setOnline(true)
    userEvent.click(screen.getByText('Submit'))

    await waitFor(() => expect(Object.keys(checkRecords)).toHaveLength(2))
    expect(checkRecords[inspectionRecord.checkRecords[0].id]).toMatchObject({
      numberValue: 234,
      notes: '',
      attachments: [],
    })
    expect(checkRecords[inspectionRecord.checkRecords[1].id]).toMatchObject({
      numberValue: 345,
      notes: '',
      attachments: [],
    })
  })

  test.each([
    {
      name: 'Visits check with dependency when the dependency is met',
      meetsDependency: false,
    },
    {
      name: 'Skips check with dependency when the dependency is not met',
      meetsDependency: true,
    },
  ])('$name', async ({ meetsDependency }) => {
    const listCheck1 = {
      id: 'check-1-id',
      sortOrder: 0,
      name: 'list check',
      type: 'list',
      typeValue: 'listitem1|listitem2',
    }

    const numericCheck3 = {
      id: 'check-3-id',
      sortOrder: 2,
      name: 'no dependencies',
      type: 'numeric',
      typeValue: 'widgets',
      decimalPlaces: 2,
      minValue: 0,
      maxValue: 10,
    }

    const numericCheck2 = {
      id: 'check-2-id',
      sortOrder: 1,
      name: 'check with dependency',
      type: 'numeric',
      typeValue: 'widgets',
      decimalPlaces: 2,
      minValue: 0,
      maxValue: 10,
      dependencyId: 'check-1-id',
      dependencyValue: 'listitem2',
    }

    const {
      result: { container },
    } = setup({
      inspection: makeInspection({
        checks: [listCheck1, numericCheck2, numericCheck3] as Check[],
      }),
    })

    await waitFor(() =>
      expect(screen.getByText('list check')).toBeInTheDocument()
    )

    toggleCheckRecord(listCheck1)

    openDropdown(screen.getByText('Select value'))

    if (meetsDependency) {
      userEvent.click(screen.getByText('Listitem2'))
    } else {
      userEvent.click(screen.getByText('Listitem1'))
    }

    userEvent.click(screen.getByText('Submit'))

    await new Promise((r) => setTimeout(r, 1000))

    expect(container.querySelector("[data-index='1'] label") != null).toEqual(
      meetsDependency
    )

    expect(container.querySelector("[data-index='2'] label") != null).toEqual(
      !meetsDependency
    )
  })

  test('on completing an inspection, redirect to inspections list and show Completed status', async () => {
    const listCheck1 = {
      id: 'check-1-id',
      sortOrder: 0,
      name: 'list check',
      type: 'list',
      typeValue: 'listitem1|listitem2',
    }
    const inspection = makeInspection({
      checks: [listCheck1] as Check[],
    })

    // Set up some routes for the inspections list (which is not tested by
    // other tests in this file). Just provide enough data so the page will
    // load, and importantly, set the checkRecordSummaryStatus to something
    // other than "complete", so we know that the Completed label is coming
    // from the IndexedDb.
    server.use(
      rest.get(
        '/api/sites/:siteId/inspectionZones/:inspectionZoneId',
        (_req, res, ctx) => res(ctx.json({ name: 'some zone' }))
      ),
      rest.get(
        '/api/sites/:siteId/inspectionZones/:inspectionZoneId/inspections',
        (_req, res, ctx) =>
          res(
            ctx.json([
              {
                id: inspection.id,
                name: 'some name',
                checkRecordSummaryStatus: 'overdue',
              },
            ])
          )
      )
    )

    const { setOnline } = setup({
      inspection,
      store: await IndexedDbStore.create('redirectInspectionsTest'),
    })

    await waitFor(() =>
      expect(screen.getByText('list check')).toBeInTheDocument()
    )

    setOnline(false)

    toggleCheckRecord(listCheck1)

    openDropdown(screen.getByText('Select value'))
    userEvent.click(screen.getByText('Listitem1'))
    userEvent.click(screen.getByText('Submit'))

    await waitFor(() => {
      assertPathnameContains(
        '/sites/site123/inspectionZones/zone123/inspections'
      )
    })

    // The ability to get this update while offline is currently disabled,
    // see comments in InspectionsList.js.
    // await waitFor(() => {
    //   expect(screen.getByText('Completed')).toBeInTheDocument()
    // })
    // expect(screen.queryByText('Overdue')).not.toBeInTheDocument()
  })

  test('should automatically sync after going online', async () => {
    // Make some changes while offline. Go online, and assert that we sync automatically
    // after a period of time.
    useFakeTimers()

    const numericCheck1 = {
      id: 'check-1-id',
      sortOrder: 0,
      name: 'numeric check 1',
      type: 'numeric',
      typeValue: 'widgets',
      decimalPlaces: 2,
      minValue: 0,
      maxValue: 10,
    }

    const numericCheck2 = {
      id: 'check-2-id',
      sortOrder: 1,
      name: 'numeric check 2',
      type: 'numeric',
      typeValue: 'widgets',
      decimalPlaces: 2,
      minValue: 0,
      maxValue: 10,
    }

    const { setOnline } = setup({
      inspection: makeInspection({
        checks: [numericCheck1, numericCheck2] as Check[],
      }),
    })

    await waitFor(() =>
      expect(screen.getByText('numeric check 1')).toBeInTheDocument()
    )
    toggleCheckRecord(numericCheck1)

    setOnline(false)

    // We cannot rely on `userEvent.type` here - it will only type the first character.
    // This appears to be very difficult to fix.
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '123' },
    })

    userEvent.click(screen.getByText('Submit'))

    await waitFor(() =>
      expect(screen.getByText('Sync Pending')).toBeInTheDocument()
    )

    setOnline(true)
    jest.advanceTimersByTime(6000)

    await waitFor(() =>
      expect(screen.getByText('Completed')).toBeInTheDocument()
    )
  })

  /**
   * Fill in a bunch of inspection records, then simulate the expiration of the
   * current inspection record. The system should notice that the current
   * inspection record has become expired, and display a refresh button. When
   * the user hits the refresh button, the system should update to the current
   * inspection record and copy the values from the old record to the new
   * record. On the next submit, all the values should be synced to the new
   * inspection record.
   */
  test('refresh to new inspection record when expired', async () => {
    jest.useFakeTimers()
    jest.setSystemTime(new Date(2023, 1, 1)) // Feb 1

    const inspection = makeInspection({
      checks: [
        // Create one check for each type, plus one more so that when we submit the fourth
        // record, we don't get redirected away from the inspection.
        numericCheck,
        totalCheck,
        listCheck,
        dateCheck,
        {
          ...numericCheck,
          name: 'dummy',
        },
      ].map((c, i) => ({ ...c, sortOrder: i, id: `check-${i}` })),
    })

    const inspectionRecord = {
      ...makeInspectionRecord(inspection),
      effectiveAt: '2023-02-20T09:12:00.000Z',
      expiresAt: '2023-02-20T09:12:00.000Z',
    }
    const {
      submitRequests,
      result: { container },
    } = setup({ inspection, inspectionRecord })

    async function submitAndWait() {
      const numSubmitRequests = submitRequests.length
      userEvent.click(screen.getByText('Submit'))
      await waitFor(() =>
        expect(submitRequests.length).toBeGreaterThan(numSubmitRequests)
      )
    }

    await waitFor(() =>
      expect(screen.getByText('numeric check')).toBeInTheDocument()
    )

    toggleCheckRecord(numericCheck)

    // We should show the current due date, but no refresh button because we
    // are not expired yet.
    expect(screen.getByText(/20 Feb 2023, 19:12/)).toBeInTheDocument()
    expect(screen.queryByText('Refresh')).not.toBeInTheDocument()

    // Fill in the values for the four check types
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '7' },
    })
    await submitAndWait()

    await waitFor(() =>
      expect(screen.getByText('total check')).toBeInTheDocument()
    )
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '9' },
    })
    await submitAndWait()

    await waitFor(() =>
      expect(screen.getByText('list check')).toBeInTheDocument()
    )
    openDropdown(screen.getByText('Select value'))

    userEvent.click(screen.getByText('Listitem1'))
    await submitAndWait()

    openDropdown(screen.getByText('- Date -'))
    userEvent.click(screen.getByText(25))
    await submitAndWait()

    // Simulate the inspection record expiry.
    jest.setSystemTime(new Date(2023, 1, 22))
    inspectionRecord.id = 'new-inspection-record-id'
    inspectionRecord.checkRecords.forEach((checkRecord, i) => {
      checkRecord.id = `new-check-record-${i}`
    })
    inspectionRecord.effectiveAt = '2023-03-20T13:17:00.000Z'
    inspectionRecord.expiresAt = '2023-03-20T13:17:00.000Z'
    jest.runOnlyPendingTimers()

    // Now the Refresh button should be visible. Click it.
    await waitFor(() => expect(screen.getByText('Refresh')).toBeInTheDocument())
    userEvent.click(screen.getByText('Refresh'))

    // The due date should be updated and the Refresh button should be gone.
    await waitFor(() =>
      expect(screen.getByText(/20 Mar 2023, 23:17/)).toBeInTheDocument()
    )
    expect(screen.queryByText('Refresh')).not.toBeInTheDocument()

    // On refresh, we should sync all the existing values to the new inspection record.
    await waitFor(() => expect(submitRequests).toHaveLength(5))
    const submittedInspectionRecord = (submitRequests[4] as any)
      .inspectionRecords[0]
    expect(submittedInspectionRecord.id).toEqual('new-inspection-record-id')
    for (let i = 0; i < 4; i++) {
      expect(submittedInspectionRecord.checkRecords[i].id).toEqual(
        `new-check-record-${i}`
      )
    }

    // We should still have the fifth check record selected. Submit a value to
    // it.
    await waitFor(() =>
      expect(
        container.querySelector("[data-index='4'] label") != null
      ).toBeTrue()
    )
    fireEvent.change(screen.getByLabelText('Entry'), {
      target: { value: '999' },
    })
    await submitAndWait()

    // The new submission should sync to the new records.
    const submittedInspectionRecord2 = (submitRequests[5] as any)
      .inspectionRecords[0]
    expect(submittedInspectionRecord2.id).toEqual('new-inspection-record-id')
    expect(submittedInspectionRecord2.checkRecords[0].id).toEqual(
      'new-check-record-4'
    )
    expect(submittedInspectionRecord2.checkRecords[0].numberValue).toEqual(999)
  })
})

function Wrapper({ children, ...options }) {
  return (
    <BaseWrapper intl={{ timezone, locale: 'en-AU' }} {...options}>
      <LayoutStubProvider>{children}</LayoutStubProvider>
    </BaseWrapper>
  )
}
