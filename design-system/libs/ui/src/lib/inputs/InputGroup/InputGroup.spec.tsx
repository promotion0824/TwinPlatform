import { render } from '../../../jest/testUtils'

import { InputGroup } from '.'
import { Button } from '../../buttons/Button'
import { Select } from '../../inputs/Select'
import { TextInput } from '../TextInput'
import { inputGroupChildrenError } from './InputGroup'

describe('InputGroup', () => {
  it('should render successfully', () => {
    const { baseElement } = render(
      <InputGroup>
        <div />
        <div />
      </InputGroup>
    )
    expect(baseElement).toBeTruthy()
  })

  it('should throw error if only have one child', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() => {
      render(
        <InputGroup>
          <div />
        </InputGroup>
      )
    }).toThrowError(inputGroupChildrenError)
  })

  it('should have all children disabled if group is disabled', () => {
    const { getByRole, getAllByRole } = render(
      <InputGroup disabled>
        <TextInput defaultValue="default" />
        <Select
          data={[
            {
              label: 'option 1',
              value: 'option1',
            },
            {
              label: 'option 2',
              value: 'option2',
            },
          ]}
        />
        <Button>button</Button>
      </InputGroup>
    )

    expect(getAllByRole('textbox')[0]).toHaveAttribute('disabled')
    expect(getAllByRole('textbox')[1]).toHaveAttribute('disabled')
    expect(getByRole('button')).toHaveAttribute('disabled')
  })
})
