/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { render, screen, waitFor, within } from '@testing-library/react'
import { DateTime } from 'luxon'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { v4 as uuidv4 } from 'uuid'
import userEvent from '@testing-library/user-event'
import {
  openDropdown,
  supportDropdowns,
} from '@willow/common/utils/testUtils/dropdown'
import CheckRecordItem from '../CheckRecordItem'
import Wrapper from '../../../../utils/testUtils/Wrapper'
import { makeCheckRecord } from '../testUtils'
import { Check } from '../types'
import { FormValue } from '../CheckRecordForm'

const server = setupServer(
  rest.get(
    '/api/sites/:siteId/inspections/:inspectionId/checks/:checkId/submittedhistory',
    (_req, res, ctx) => res(ctx.json({}))
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

const inspectionRecordId = 'a758bd31-1d5e-4663-888c-82f046b7621a'
const inspectionId = 'e764f985-0841-4955-a30d-a8dd83e80c7a'
const siteId = uuidv4()

const numericCheck: Check = {
  id: uuidv4(),
  type: 'numeric',
  name: 'Numeric check',
  inspectionId,
  sortOrder: 0,
  typeValue: '20',
  decimalPlaces: 2,
}

const totalCheck: Check = {
  id: uuidv4(),
  type: 'total',
  name: 'Total check',
  inspectionId,
  sortOrder: 0,
  typeValue: '100',
  decimalPlaces: 3,
}

const lastTotalCheck: Check = {
  id: uuidv4(),
  type: 'total',
  name: 'Last total check',
  inspectionId,
  sortOrder: 0,
  typeValue: '100',
  decimalPlaces: 3,
}

const listCheck: Check = {
  id: uuidv4(),
  type: 'list',
  name: 'List check',
  inspectionId,
  sortOrder: 0,
  typeValue: 'Option1|Option2|Option3',
}

const dateCheck: Check = {
  id: uuidv4(),
  type: 'date',
  name: 'Date check',
  inspectionId,
  sortOrder: 0,
  typeValue: '',
}

const getFields = (isAfterSubmission?: boolean) => {
  const form = screen.getByRole('form')
  return {
    numberEntry: within(form).queryByRole('textbox', {
      // Somehow after submission, the recognised name is validation error
      name: isAfterSubmission ? 'Value is required' : 'Entry',
    }),
    dropdownEntry: within(form).queryByRole('button', { name: 'Entry' }),
    dateEntry: within(form).queryByRole('button', { name: 'Date' }),
    notes: within(form).queryByRole('textbox', { name: 'Notes' }),
    submitButton: within(form).queryByRole('button', { name: 'Submit' }),
  }
}

const setupTest = (
  check: Check,
  isExpanded = false,
  checkRecordValue?: number | string
) => {
  const checkRecord = makeCheckRecord(
    check,
    inspectionRecordId,
    'due',
    checkRecordValue
  )
  render(
    <CheckRecordItem
      isExpanded={isExpanded}
      siteId={siteId}
      check={check}
      checkRecord={checkRecord}
      attachmentEntries={[]}
      onToggle={mockedToggle}
      onSubmit={mockedSubmit}
      onSubmitError={mockedSubmitError}
    />,
    { wrapper: Wrapper }
  )
  return { check, checkRecord }
}

const openCheckRecord = (checkItem) => {
  userEvent.click(
    screen.getByRole('button', {
      name: new RegExp(`${checkItem.name}.+`),
    })
  )
}

const assertNumberInput = async (
  expectedValue: string,
  expectedErrorMessage?: string,
  isAfterSubmission?: boolean
) => {
  const fields = getFields(isAfterSubmission)

  await waitFor(() => {
    expect(fields.numberEntry).toHaveValue(expectedValue)
    if (expectedErrorMessage != null) {
      expect(fields.numberEntry).toHaveAttribute('data-error', 'true')
      expect(screen.getByText(expectedErrorMessage)).toBeInTheDocument()
    } else {
      expect(fields.numberEntry).not.toHaveAttribute('data-error')
    }
  })
}

const submitAndAssertSubmitSuccess = async (expectedFormValue: FormValue) => {
  const fields = getFields()

  userEvent.click(fields.submitButton!)
  await waitFor(() => {
    expect(mockedSubmit).toHaveBeenCalledWith(
      expectedFormValue,
      expect.toContainKeys(['type', 'target']) // submit event object
    )
    expect(mockedSubmitError).toHaveBeenCalledTimes(0)
  })
}

const submitAndAssertSubmitError = async () => {
  const fields = getFields()

  userEvent.click(fields.submitButton!)
  await waitFor(() => {
    expect(mockedSubmit).toHaveBeenCalledTimes(0)
    expect(mockedSubmitError).toHaveBeenCalledTimes(1)
  })
}

const mockedToggle = jest.fn()
const mockedSubmit = jest.fn()
const mockedSubmitError = jest.fn()
const mockExpanded = jest.fn()

supportDropdowns()

beforeEach(() => {
  mockedToggle.mockReset()
  mockedSubmit.mockReset()
  mockedSubmitError.mockReset()
})

describe('CheckRecordItem tests', () => {
  test('Toggle check record button', () => {
    setupTest(dateCheck)

    openCheckRecord(dateCheck)

    expect(mockedToggle).toHaveBeenCalledTimes(1)
  })

  describe('Form Validation', () => {
    describe('Numeric check record', () => {
      test('Submitting empty form should show validation for required field', async () => {
        setupTest(numericCheck, true)

        openCheckRecord(numericCheck)

        await assertNumberInput('')

        await submitAndAssertSubmitError()

        await assertNumberInput('', 'Value is required', true)
      })

      test('Entry should show required error after value has been cleared', async () => {
        setupTest(numericCheck, true, 12)
        openCheckRecord(numericCheck)
        const fields = getFields()

        await assertNumberInput('12.00')
        await userEvent.type(fields.numberEntry!, '{selectall}{backspace}', {
          delay: 1,
        })

        userEvent.tab() // blur numeric entry
        await assertNumberInput('', 'Value is required')

        await submitAndAssertSubmitError()
      })

      test.each([
        {
          inputEntry: '0',
          expectedEntryField: '0',
          expectedNumberValue: 0,
          decimalPlaces: 0,
        },
        {
          inputEntry: '-1',
          expectedEntryField: '-1',
          expectedNumberValue: -1,
          decimalPlaces: 0,
        },
        {
          inputEntry: '246',
          expectedEntryField: '246',
          expectedNumberValue: 246,
          decimalPlaces: 0,
        },
        {
          inputEntry: '0',
          expectedEntryField: '0.00',
          expectedNumberValue: 0,
          decimalPlaces: 2,
        },
        {
          inputEntry: '-1',
          expectedEntryField: '-1.00',
          expectedNumberValue: -1,
          decimalPlaces: 2,
        },
        {
          inputEntry: '246',
          expectedEntryField: '246.00',
          expectedNumberValue: 246,
          decimalPlaces: 2,
        },
      ])(
        'Numeric entry with $decimalPlaces decimal place: Entering "$inputEntry"',
        async ({
          inputEntry,
          expectedEntryField,
          expectedNumberValue,
          decimalPlaces,
        }) => {
          setupTest(
            {
              ...numericCheck,
              decimalPlaces,
            } as Check,
            true
          )
          openCheckRecord(numericCheck)
          const fields = getFields()

          // Test negative value
          await userEvent.type(fields.numberEntry!, inputEntry, { delay: 1 })

          await assertNumberInput(expectedEntryField)

          await submitAndAssertSubmitSuccess({
            checkRecord: {
              numberValue: expectedNumberValue,
              notes: '',
            },
            attachmentEntries: [],
          })
        }
      )

      test('Should show validation when entered value is above maxValue', async () => {
        setupTest({ ...numericCheck, maxValue: 4 } as Check, true)
        openCheckRecord(numericCheck)
        const fields = getFields()

        await userEvent.type(fields.numberEntry!, '4.01', { delay: 1 })
        // Blur numeric entry
        userEvent.tab()

        await assertNumberInput(
          '4.01',
          'This is above the 4 threshold value. Are you sure?'
        )

        await submitAndAssertSubmitSuccess({
          checkRecord: {
            numberValue: 4.01,
            notes: '',
          },
          attachmentEntries: [],
        })
      })

      test('Should show validation when entered value is below minValue', async () => {
        setupTest({ ...numericCheck, minValue: 1 } as Check, true)
        openCheckRecord(numericCheck)
        const fields = getFields()

        await userEvent.type(fields.numberEntry!, '0.99', { delay: 1 })
        // Blur numeric entry
        userEvent.tab()

        await assertNumberInput(
          '0.99',
          'This is below the 1 threshold value. Are you sure?'
        )

        await submitAndAssertSubmitSuccess({
          checkRecord: {
            numberValue: 0.99,
            notes: '',
          },
          attachmentEntries: [],
        })
      })
    })

    describe('Total check record', () => {
      test('Submitting empty form should show validation for required field', async () => {
        setupTest(totalCheck, true)
        openCheckRecord(totalCheck)
        const fields = getFields()

        await assertNumberInput('')
        userEvent.click(fields.submitButton!)

        await assertNumberInput('', 'Value is required')
      })

      test('Entry should show required error after the entry value has been cleared', async () => {
        setupTest(totalCheck, true, 100)
        openCheckRecord(totalCheck)
        const fields = getFields()

        await assertNumberInput('100.000')
        await userEvent.type(fields.numberEntry!, '{selectall}{backspace}', {
          delay: 1,
        })
        userEvent.tab() // blur numeric entry

        await assertNumberInput('', 'Value is required')

        await submitAndAssertSubmitError()
      })

      test('Entry should use last submitted entry as default value', async () => {
        setupTest(
          {
            ...totalCheck,
            lastSubmittedRecord: makeCheckRecord(
              lastTotalCheck,
              inspectionRecordId,
              'completed',
              -100
            ),
          } as Check,
          true
        )
        openCheckRecord(totalCheck)
        await assertNumberInput('-100.000')
      })

      test('Should show validation when entered value is below last submitted entry', async () => {
        setupTest(
          {
            ...totalCheck,
            lastSubmittedRecord: makeCheckRecord(
              lastTotalCheck,
              inspectionRecordId,
              'completed',
              20.123
            ),
          } as Check,
          true
        )
        openCheckRecord(totalCheck)
        const fields = getFields()

        await userEvent.type(
          fields.numberEntry!,
          '{selectall}{backspace}20.122',
          { delay: 1 }
        )
        // Blur numeric entry
        userEvent.tab()

        await assertNumberInput(
          '20.122',
          'Total value cannot be lower than 20.123'
        )

        await submitAndAssertSubmitSuccess({
          checkRecord: {
            numberValue: 20.122,
            notes: '',
          },
          attachmentEntries: [],
        })
      })
    })

    describe('List check record', () => {
      test('Should load entry with existing value', async () => {
        setupTest(listCheck, true, 'Option2')
        openCheckRecord(listCheck)
        const fields = getFields()

        await waitFor(() => {
          expect(fields.dropdownEntry).toHaveTextContent('Option2')
        })
      })

      test('Submitting empty form should show validation for required field', async () => {
        setupTest(listCheck, true)
        openCheckRecord(listCheck)
        const fields = getFields()

        expect(fields.dropdownEntry).toHaveTextContent('Select value')

        userEvent.click(fields.submitButton!)

        await waitFor(() => {
          expect(fields.dropdownEntry).toHaveAttribute('data-error', 'true')
          expect(screen.getByText('Value is required')).toBeInTheDocument()

          expect(mockedSubmit).toHaveBeenCalledTimes(0)
          expect(mockedSubmitError).toHaveBeenCalledTimes(1)
        })
      })

      test('Successfully submit form with selected entry', async () => {
        setupTest(listCheck, true)
        openCheckRecord(listCheck)
        const fields = getFields()

        openDropdown(fields.dropdownEntry!)

        const allOptions = screen.getAllByRole('option')

        expect(
          allOptions.map((optionEl) => optionEl.textContent)
        ).toStrictEqual(['Option1', 'Option2', 'Option3'])
        userEvent.click(allOptions[0])

        await waitFor(() => {
          expect(fields.dropdownEntry).toHaveTextContent('Option1')
        })

        await submitAndAssertSubmitSuccess({
          checkRecord: {
            stringValue: 'Option1',
            notes: '',
          },
          attachmentEntries: [],
        })
      })
    })

    describe('Date check record', () => {
      test('Should load entry with existing value', async () => {
        setupTest(dateCheck, true, '2022-10-02T16:00:00.000Z')
        openCheckRecord(dateCheck)
        const fields = getFields()

        await waitFor(() => {
          expect(fields.dateEntry).toHaveTextContent(
            DateTime.fromISO('2022-10-02T16:00:00.000Z').toLocaleString(
              DateTime.DATE_MED
            )
          )
        })
      })

      test('Submitting empty form should show validation for required field', async () => {
        setupTest(dateCheck, true)
        openCheckRecord(dateCheck)
        const fields = getFields()

        expect(fields.dateEntry).toHaveTextContent('Date')

        await submitAndAssertSubmitError()

        await waitFor(() => {
          expect(fields.dateEntry).toHaveAttribute('data-error', 'true')
          expect(screen.getByText('Value is required')).toBeInTheDocument()
        })
      })

      test('Successfully submit form with date entry', async () => {
        setupTest(dateCheck, true)
        openCheckRecord(dateCheck)
        const fields = getFields()

        openDropdown(fields.dateEntry!)

        userEvent.click(screen.getByRole('button', { name: '21' }))

        await submitAndAssertSubmitSuccess({
          checkRecord: {
            dateValue:
              DateTime.now().startOf('day').set({ day: 21 }).toUTC().toISO() ??
              undefined,
            notes: '',
          },
          attachmentEntries: [],
        })
      })
    })
  })
})
