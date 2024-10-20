import { render } from '../../../jest/testUtils'

import { Field } from '.'

describe('Field', () => {
  it('should render successfully', () => {
    const { baseElement } = render(
      <Field>
        <input />
      </Field>
    )
    expect(baseElement).toBeTruthy()
  })
})
