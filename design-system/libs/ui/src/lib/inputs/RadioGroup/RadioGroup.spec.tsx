import { render, screen } from '../../../jest/testUtils'
import userEvent from '@testing-library/user-event'

import { Radio } from '../Radio'
import { RadioGroup, RadioGroupProps } from '.'

const SampleRadioGroup = ({
  options,
  ...props
}: {
  options: string[]
} & Partial<RadioGroupProps>) => (
  <RadioGroup {...props}>
    {options.map((option) => (
      <Radio key={option} label={option} value={option} />
    ))}
  </RadioGroup>
)

function option(label: string) {
  return screen.getByLabelText(label)
}

function label(label: string) {
  return screen.getByText(label)
}

const arrowKeys = ['{ArrowLeft}', '{ArrowRight}', '{ArrowUp}', '{ArrowDown}']

const possibleArrowKeyCombinations = arrowKeys.reduce((combination, key1) => {
  arrowKeys.forEach((key2) => combination.push([key1, key2]))
  return combination
}, [] as [string, string][])

describe('RadioGroup', () => {
  it('should support initial checked radio', async () => {
    render(<SampleRadioGroup options={['v1', 'v2']} defaultValue="v1" />)

    expect(option('v1')).toBeChecked()
  })

  it('should check the clicked radio', async () => {
    render(<SampleRadioGroup options={['v1', 'v2']} defaultValue="v1" />)

    await userEvent.click(option('v2'))

    expect(option('v2')).toBeChecked()
  })

  it('should change selection by clicking options', async () => {
    render(<SampleRadioGroup options={['v1', 'v2']} defaultValue="v1" />)

    await userEvent.click(option('v2'))
    await userEvent.click(option('v1'))

    expect(option('v1')).toBeChecked()
  })

  it('should check the option of clicked label text', async () => {
    render(<SampleRadioGroup options={['v1', 'v2']} defaultValue="v1" />)

    await userEvent.click(label('v2'))

    expect(option('v2')).toBeChecked()
  })

  it('should change the option by clicking label text', async () => {
    render(<SampleRadioGroup options={['v1', 'v2']} defaultValue="v1" />)

    await userEvent.click(label('v2'))
    await userEvent.click(label('v1'))

    expect(option('v1')).toBeChecked()
  })

  it.each([['{ArrowLeft}'], ['{ArrowRight}'], ['{ArrowUp}'], ['{ArrowDown}']])(
    'should select another option when key %s pressed',
    async (key) => {
      render(<SampleRadioGroup options={['v1', 'v2']} />)
      await userEvent.click(option('v2')) // start with v2 to be selected and focused
      await userEvent.keyboard(key)

      // Since we only have 2 options,
      // and any odd number of keyboard actions will select another option.
      // we only test 1 keyboard action here
      expect(option('v1')).toBeChecked()
    }
  )

  it.each(possibleArrowKeyCombinations)(
    'should select the original option when key %s and %s pressed',
    async (key1, key2) => {
      render(<SampleRadioGroup options={['v1', 'v2']} />)
      await userEvent.click(option('v2')) // start with v2 to be selected and focused
      await userEvent.keyboard(key1)
      await userEvent.keyboard(key2)

      // Since we only have 2 options,
      // any even number of keyboard actions will select the initial selected option.
      // we only test 2 keyboard actions
      expect(option('v2')).toBeChecked()
    }
  )

  it('should be able to select the option with space key', async () => {
    render(<SampleRadioGroup options={['v1', 'v2']} defaultValue="v1" />)

    option('v1').focus()
    await userEvent.keyboard(' ')

    expect(option('v1')).toBeChecked()
  })

  it('should disabled the disabled option, and other options should still enabled', () => {
    const WithDisabledItem = () => {
      return (
        <RadioGroup>
          <Radio value="v1" label="v1" />
          <Radio value="v2" disabled label="v2" />
        </RadioGroup>
      )
    }
    render(<WithDisabledItem />)

    expect(option('v1')).not.toBeDisabled()
    expect(option('v2')).toBeDisabled()
  })

  it('should trigger the onChange function when change the selected option', async () => {
    const mockedOnChange = jest.fn()
    const WithOnChange = () => (
      <RadioGroup onChange={mockedOnChange}>
        <Radio value="v1" label="v1" />
        <Radio value="v2" label="v2" />
      </RadioGroup>
    )

    render(<WithOnChange />)

    await userEvent.click(option('v1'))
    await userEvent.click(option('v2'))

    expect(mockedOnChange.mock.calls.length).toBe(2)
  })
})
