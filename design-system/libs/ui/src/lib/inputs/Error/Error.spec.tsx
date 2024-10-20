import { render } from '../../../jest/testUtils'

import { Error } from '.'

describe('Error', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Error />)
    expect(baseElement).toBeTruthy()
  })
})
