import React, { useState } from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DurationInput from '../DurationInput'
import { OnClickOutsideIdsProvider } from '../../../providers'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import {
  supportDropdowns,
  openDropdown,
} from '../../../utils/testUtils/dropdown'

supportDropdowns()

describe('DurationInput', () => {
  test('takes properties from duration and puts them in the input fields', async () => {
    setup({
      value: {
        years: 6,
        months: 5,
        days: 4,
        hours: 3,
        minutes: 2,
        seconds: 1,
      },
    })

    openDropdown(screen.getByRole('button'))

    expect(getInputValues()).toEqual({
      years: '6',
      months: '5',
      days: '4',
      hours: '3',
      minutes: '2',
      seconds: '1',
    })
  })

  test('Clear and unclear', async () => {
    setup({
      value: {
        years: 6,
        months: 5,
        days: 4,
        hours: 3,
        minutes: 2,
        seconds: 1,
      },
    })
    openDropdown(screen.getByRole('button'))

    // Clicking the clear button should empty all the text boxes
    userEvent.click(screen.getByText('plainText.clear'))
    expect(getInputValues()).toEqual({
      years: '',
      months: '',
      days: '',
      hours: '',
      minutes: '',
      seconds: '',
    })

    // When the text boxes are all empty, typing into one of them should set
    // all the others to zero.
    userEvent.type(screen.getByLabelText('plainText.days'), '3')
    expect(getInputValues()).toEqual({
      years: '0',
      months: '0',
      days: '3',
      hours: '0',
      minutes: '0',
      seconds: '0',
    })
  })
})

function Wrapper({ initialValue }) {
  const [value, setValue] = useState(initialValue)
  return (
    <DurationInput
      id="id"
      ariaLabelledBy="label"
      value={value}
      onChange={setValue}
    />
  )
}

function setup(props) {
  render(
    <BaseWrapper>
      <Wrapper initialValue={props.value} />
    </BaseWrapper>
  )
}

function getInputValues() {
  return Object.fromEntries(
    ['years', 'months', 'days', 'hours', 'minutes', 'seconds'].map((field) => [
      field,
      (screen.getByLabelText(`plainText.${field}`) as HTMLInputElement).value,
    ])
  )
}
