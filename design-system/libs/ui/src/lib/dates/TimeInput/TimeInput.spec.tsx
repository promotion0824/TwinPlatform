import {
  render,
  screen,
  waitFor,
  waitForElementToBeRemoved,
} from '../../../jest/testUtils'

import { TimeInput } from '.'

const defaultTime = '08:00'
const defaultLabel = 'Time input'
const inputWithDefaultLabel = () =>
  screen.getByLabelText(defaultLabel) as HTMLInputElement
const dropdown = () => screen.getByRole('dialog')
const queryForDropdown = () => screen.queryByRole('dialog')
const clickOption = async (time: string) =>
  waitFor(() => screen.getByRole('button', { name: time }).click())

describe('TimeInput', () => {
  it('should display default value if provided', () => {
    render(<TimeInput defaultValue={defaultTime} label={defaultLabel} />)
    expect(inputWithDefaultLabel().value).toBe(defaultTime)
  })

  it('should display value if provided', () => {
    render(<TimeInput value={defaultTime} label={defaultLabel} />)
    expect(inputWithDefaultLabel().value).toBe(defaultTime)
  })

  it('should display value instead of default value when selection made', async () => {
    render(<TimeInput defaultValue={defaultTime} label={defaultLabel} />)
    const timeInput = inputWithDefaultLabel()
    timeInput.click()
    await clickOption('01:00 pm')
    waitForElementToBeRemoved(dropdown())
    expect(timeInput.value).toBe('13:00')
  })

  it('should trigger the controlled onChange when making a selection', async () => {
    const onChange = jest.fn()
    render(
      <TimeInput value={defaultTime} onChange={onChange} label={defaultLabel} />
    )

    inputWithDefaultLabel().click()
    await clickOption('01:00 pm')

    expect(onChange).toHaveBeenCalledWith('13:00')
  })
})

describe('Dropdown for TimeInput', () => {
  it('should open when focused', async () => {
    render(<TimeInput label={defaultLabel} />)
    inputWithDefaultLabel().focus()
    await waitFor(() => expect(dropdown()).toBeInTheDocument())
  })

  it('should not open with readOnly when click', () => {
    const { queryByRole } = render(<TimeInput label={defaultLabel} readOnly />)
    inputWithDefaultLabel().click()
    expect(queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('should not open with readOnly when focus', () => {
    render(<TimeInput label={defaultLabel} readOnly />)
    inputWithDefaultLabel().focus()
    expect(queryForDropdown()).not.toBeInTheDocument()
  })

  it('should not open with disabled when click', () => {
    render(<TimeInput label={defaultLabel} disabled />)
    inputWithDefaultLabel().click()
    expect(queryForDropdown()).not.toBeInTheDocument()
  })

  it('should not open with disabled when focus', () => {
    render(<TimeInput label={defaultLabel} disabled />)
    inputWithDefaultLabel().focus()
    expect(queryForDropdown()).not.toBeInTheDocument()
  })

  it('should close after click a selection', async () => {
    render(<TimeInput label={defaultLabel} />)
    inputWithDefaultLabel().click()

    await clickOption('01:00 pm')
    await waitFor(() => expect(queryForDropdown()).not.toBeInTheDocument())
  })
})

describe('Time list', () => {
  it('should have correct default time list', async () => {
    const { getAllByRole, getByRole } = render(
      <TimeInput label={defaultLabel} />
    )
    inputWithDefaultLabel().click()

    await waitFor(() => {
      expect(getAllByRole('button')).toHaveLength(96)
      expect(getByRole('button', { name: '01:00 pm' })).toBeInTheDocument()
      expect(getByRole('button', { name: '02:15 am' })).toBeInTheDocument()
    })
  })

  it('should not contain filtered time', () => {
    const { queryByRole } = render(
      <TimeInput
        label={defaultLabel}
        getTimes={(times) => times.filter(({ value }) => value !== '13:00')}
      />
    )
    inputWithDefaultLabel().click()
    expect(queryByRole('button', { name: '01:00 pm' })).not.toBeInTheDocument()
  })

  it('should not be able to select disabled time', async () => {
    const { queryByRole } = render(
      <TimeInput
        label={defaultLabel}
        getTimes={(times) =>
          times.map((time) => {
            return {
              ...time,
              disabled: time.value.includes(':15'),
            }
          })
        }
      />
    )
    inputWithDefaultLabel().click()
    await waitFor(() =>
      expect(queryByRole('button', { name: '02:15 am' })).toBeDisabled()
    )
  })
})
