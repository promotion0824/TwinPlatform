import { render } from '../../../jest/testUtils'

import { Pill } from '.'

describe('Pill', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Pill />)
    expect(baseElement).toBeTruthy()
  })
})
