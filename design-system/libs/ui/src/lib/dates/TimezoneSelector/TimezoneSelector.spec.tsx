import { screen, render, userEvent } from '../../../jest/testUtils'

import { TimezoneSelector } from './TimezoneSelector'
import { generateOptions, mockedTimezones } from './utils'

const mockedGenerateTimezones = generateOptions(mockedTimezones)
jest.mock('./utils', () => {
  return {
    __esModule: true,
    ...jest.requireActual('./utils'),
    generateTimezones: jest
      .fn()
      .mockImplementation(() => mockedGenerateTimezones),
  }
})

const getSelectionBox = () => screen.getByRole<HTMLInputElement>('textbox')
const getOptions = () => screen.queryAllByRole('option')
const toggleDropdown = async () => await userEvent.click(getSelectionBox())

describe('TimezoneSelector', () => {
  it('should return all mockedTimezones as options', async () => {
    render(<TimezoneSelector />)
    await toggleDropdown()

    const options = getOptions()
    expect(options.length).toBe(mockedTimezones.length)
  })

  it('should return filtered timezones as options', async () => {
    const filterText = 'America'
    render(
      <TimezoneSelector
        data={(options) =>
          options?.filter(({ label }) => label.includes(filterText))
        }
      />
    )

    await toggleDropdown()

    const options = getOptions()
    expect(options.length).toBe(
      mockedTimezones.filter((timezone) => timezone.includes(filterText)).length
    )
  })

  it('should return the customized options', async () => {
    const customOptions = [
      { label: 'UTC', value: 'UTC' },
      { label: 'UTC+1', value: 'UTC+1' },
    ]
    render(<TimezoneSelector data={customOptions} />)

    await toggleDropdown()

    const options = getOptions()
    expect(options.length).toEqual(customOptions.length)
  })
})
