import { render } from '../../../jest/testUtils'

import { DatePicker } from '.'

describe('DatePicker', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<DatePicker />)
    expect(baseElement).toBeTruthy()
  })
})
