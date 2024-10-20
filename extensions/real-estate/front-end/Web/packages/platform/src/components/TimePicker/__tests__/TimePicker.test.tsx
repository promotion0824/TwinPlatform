import React, { useState } from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import {
  supportDropdowns,
  openDropdown,
} from '@willow/ui/utils/testUtils/dropdown'
import TimePicker, { parse } from '../TimePicker'

supportDropdowns()

describe('TimePicker', () => {
  test('parses value and puts its constituents into input fields', async () => {
    setup({
      value: '09:30:01',
    })

    openDropdown(screen.getByRole('button'))

    expect(getInputValues()).toEqual({ hr: '09', min: '30', sec: '01' })
  })

  test('Clear and unclear', async () => {
    setup({
      value: '09:30:01',
    })
    openDropdown(screen.getByRole('button'))

    // Clicking the clear button should empty all the text boxes
    userEvent.click(screen.getByText('plainText.clear'))
    expect(getInputValues()).toEqual({ hr: '', min: '', sec: '' })

    // When the text boxes are all empty, typing into one of them should set
    // all the others to zero.
    userEvent.type(screen.getByLabelText('plainText.min'), '51')
    expect(getInputValues()).toEqual({ hr: '0', min: '51', sec: '0' })
  })
})

describe('parse', () => {
  test('should parse correctly with no time zone', () => {
    expect(parse('12:34:56')).toEqual({
      hours: '12',
      minutes: '34',
      seconds: '56',
      offset: '',
    })
  })

  test('should parse correctly with time zone', () => {
    expect(parse('12:34:56+09:00')).toEqual({
      hours: '12',
      minutes: '34',
      seconds: '56',
      offset: '+09:00',
    })
  })
})

function Wrapper({ initialValue }) {
  const [value, setValue] = useState(initialValue)
  return (
    <BaseWrapper>
      <TimePicker
        id="some-id"
        ariaLabelledBy="something"
        value={value}
        onChange={setValue}
      />
    </BaseWrapper>
  )
}

function setup(props) {
  render(<Wrapper initialValue={props.value} />)
}

function getInputValues() {
  return Object.fromEntries(
    ['hr', 'min', 'sec'].map((field) => [
      field,
      (screen.getByLabelText(`plainText.${field}`) as HTMLInputElement).value,
    ])
  )
}
